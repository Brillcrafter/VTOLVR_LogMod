global using static vtolLogMod.Logger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.Newtonsoft.Json;
using VTOLVR.Multiplayer;

namespace vtolLogMod;

[ItemId("vtolLogMod")] // Harmony ID for your mod, make sure this is unique
public class Main : VtolMod
{
    public string ModFolder;
    
    public static Main Instance;
    
    private Harmony _harmony;
    
    public string saveFolder;
    
    private PilotSave _pilotSave;
    
    private LogOutput _logOutput;
    
    private const string SaveFileName = "pilotLogSave.json";
    private const string BackupSaveFileName = "backupPilotLogSave.json";
    public const string LogFileName = "pilotLog.txt";
    

    private void Awake()
    {
        Instance = this;
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Log($"Awake at {ModFolder}");
        _harmony = new Harmony("Brillcrafter.vtolLogMod");
        saveFolder = Path.Combine(VTResources.gameRootDirectory, "Pilot Log");
        _logOutput = new LogOutput();
        if (!Directory.Exists(saveFolder))
        {
            Log("vtolLogMod: Pilot Log folder not found");
            Directory.CreateDirectory(saveFolder);
        }
        if (File.Exists(Path.Combine(saveFolder, SaveFileName)))
        {
            try
            {
                Log("vtolLogMod: Pilot Log save file found, loading file");
                var fileText = File.ReadAllText(Path.Combine(saveFolder, SaveFileName));
                _pilotSave = JsonConvert.DeserializeObject<PilotSave>(fileText);
            }
            catch (Exception e)
            {
                Log($"vtolLogMod: Error loading save file: {e}\n Renaming LogFile to " + BackupSaveFileName);
                File.Move(SaveFileName, BackupSaveFileName);
            }
        }
        var type = typeof(Actor);
        var method = type.GetMethod(nameof(Actor.H_OnDeath));

        var ejectType = typeof(EjectionSeat);
        var ejectMethod = ejectType.GetMethod(nameof(EjectionSeat.Eject));
        
        var playerFlightLoggerType = typeof(PlayerFlightLogger);
        var updateMethod = playerFlightLoggerType.GetMethod(nameof(PlayerFlightLogger.Update));
        
        var endMissionType = typeof(EndMission);
        var endMissionOnFinalWinner = endMissionType.GetMethod(nameof(EndMission.EndMission_OnFinalWinner));

        _harmony.Patch(endMissionOnFinalWinner,
            new HarmonyMethod(typeof(Main).GetMethod(nameof(EndMissionOnFinalWinnerPrefix))));
        _harmony.Patch(updateMethod, new HarmonyMethod(typeof(Main).GetMethod(nameof(UpdatePrefix))));
        _harmony.Patch(ejectMethod,null ,new HarmonyMethod(typeof(Main).GetMethod(nameof(EjectPostfix))));
        _harmony.Patch(method, new HarmonyMethod(typeof(Main).GetMethod(nameof(H_OnDeathPrefix))));
    }

    private static void EndMissionOnFinalWinnerPrefix(Teams obj)
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        var teams = Teams.Allied;
        if (FlightSceneManager.instance.playerActor)
        {
            teams = FlightSceneManager.instance.playerActor.team;
        }
        if (obj == teams)
        {
            Instance._pilotSave.NumberOfSuccessfulMissions++;
        }
        else
        {
            Instance._pilotSave.NumberOfFailedMissions++;
        }
        SaveSynchronous();
        Instance._logOutput.WriteText(Instance._pilotSave);
    }

    private static void SaveSynchronous()
    {
        var output = JsonConvert.SerializeObject(Instance._pilotSave);
        SaveAsynchronous(output);
    }

    private static async Task SaveAsynchronous(string saveOutput)
    {
        await Task.Run(() =>
        {
            Log("vtolLogMod: Saving Pilot Log");
            File.WriteAllText(Instance.saveFolder, saveOutput);
        });
    }
    
    private static void UpdatePrefix(PlayerFlightLogger __instance)
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        if (!__instance.recording || __instance.flightInfo.isLanded == __instance.isLanded ||
            !(Time.time - __instance.landedChangeTime > 3f)) return;
        var isLanded = __instance.flightInfo.isLanded;
        if (isLanded)
        {
            Instance._pilotSave.NumberOfLandings++;
            return;
        }
        Instance._pilotSave.NumberOfTakeoffs++;
    }

    private static void EjectPostfix()
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        Instance._pilotSave.NumberOfEjections++;
    }
    
    private static void H_OnDeathPrefix(Actor __instance)
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        if (__instance._gotIsHuman)
        {
            //Log("Actor is Human");
            Instance._pilotSave.NumberOfDeaths++;
            return;
        }

        if (!__instance.h.killedByActor._gotIsHuman)
        {
            //Log("killing Actor is not Human");
            return;
        }
        switch (__instance.role)
        {
            case Actor.Roles.None:
                //Log("Actor is None");
                break;
            case Actor.Roles.Air:
                //Log("Actor is Air");
                Instance._pilotSave.A2AKills++;
                break;
            case Actor.Roles.Ground:
                //Log("Actor is Ground");
                Instance._pilotSave.A2GKills++;
                break;
            case Actor.Roles.GroundArmor:
                Instance._pilotSave.A2GKills++;
                //Log("Actor is GroundArmor");
                break;
            case Actor.Roles.Missile:
                //Log("Actor is Missile");
                break;
            case Actor.Roles.Ship:
                //Log("Actor is Ship");
                Instance._pilotSave.A2ShipKills++;
                break;
            default:
                Log("vtolLogMod: Actor is default, something wrong has happened");
                break;
        }
    }

    public override void UnLoad()
    {
        // Destroy any objects
        SaveSynchronous();
        Instance._logOutput.WriteText(_pilotSave);
        _logOutput.FinishImage();
    }
}