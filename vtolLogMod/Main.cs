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
using Valve.Newtonsoft.Json;
using VTOLVR.Multiplayer;

namespace vtolLogMod;

[ItemId("vtolLogMod")] // Harmony ID for your mod, make sure this is unique
public class Main : VtolMod
{
    public string ModFolder;
    
    private static Main _instance;
    
    private Harmony _harmony;
    
    private string _saveFolder;
    
    private PilotSave _pilotSave;
    
    private const string SaveFileName = "pilotLogSave.json";
    private const string BackupSaveFileName = "backupPilotLogSave.json";
    private const string LogFileName = "pilotLog.txt";
    

    private void Awake()
    {
        _instance = this;
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Log($"Awake at {ModFolder}");
        _harmony = new Harmony("Brillcrafter.vtolLogMod");
        _saveFolder = Path.Combine(VTResources.gameRootDirectory, "Pilot Log");
        if (!Directory.Exists(_saveFolder))
        {
            Log("vtolLogMod: Pilot Log folder not found");
            Directory.CreateDirectory(_saveFolder);
        }
        if (File.Exists(Path.Combine(_saveFolder, SaveFileName)))
        {
            try
            {
                Log("vtolLogMod: Pilot Log save file found, loading file");
                var fileText = File.ReadAllText(Path.Combine(_saveFolder, SaveFileName));
                _pilotSave = JsonConvert.DeserializeObject<PilotSave>(fileText);
            }
            catch (Exception e)
            {
                Log($"vtolLogMod: Error loading save file: {e}\n Renaming LogFile to " + BackupSaveFileName);
                File.Move(SaveFileName, BackupSaveFileName);
            }
        }
        var type = typeof(Actor);
        var method = type.GetMethod("H_OnDeath");
        if (method == null)
        {
            Log("vtolLogMod: H_OnDeath not found");
            return;
        }

        var ejectType = typeof(EjectionSeat);
        var ejectMethod = ejectType.GetMethod(nameof(EjectionSeat.Eject));
        if (ejectMethod == null)
        {
            Log("vtolLogMod: Eject not found");
            return;
        }
        
        var playerFlightLoggerType = typeof(PlayerFlightLogger);
        var updateMethod = playerFlightLoggerType.GetMethod(nameof(PlayerFlightLogger.Update));
        if (updateMethod == null)
        {
           Log("vtolLogMod: PFL Update not found"); 
           return;
        }
        var endMissionType = typeof(EndMission);
        var endMissionOnFinalWinner = endMissionType.GetMethod(nameof(EndMission.EndMission_OnFinalWinner));
        if (endMissionOnFinalWinner == null)
        {
            Log("vtolLogMod: EndMission_OnFinalWinner not found");
            return;
        }

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
            _instance._pilotSave.NumberOfSuccessfulMissions++;
        }
        else
        {
            _instance._pilotSave.NumberOfFailedMissions++;
        }
        SaveSynchronous();
    }

    private static void SaveSynchronous()
    {
        var output = JsonConvert.SerializeObject(_instance._pilotSave);
        SaveAsynchronous(output);
    }

    private static async Task SaveAsynchronous(string saveOutput)
    {
        await Task.Run(() =>
        {
            Log("vtolLogMod: Saving Pilot Log");
            File.WriteAllText(_instance._saveFolder, saveOutput);
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
            _instance._pilotSave.NumberOfLandings++;
            return;
        }
        _instance._pilotSave.NumberOfTakeoffs++;
    }

    private static void EjectPostfix()
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        _instance._pilotSave.NumberOfEjections++;
    }
    
    private static void H_OnDeathPrefix(Actor __instance)
    {
        if (VTOLMPUtils.IsMultiplayer()) return;
        if (__instance._gotIsHuman)
        {
            Log("Actor is Human");
            _instance._pilotSave.NumberOfDeaths++;
            return;
        }

        if (!__instance.h.killedByActor._gotIsHuman)
        {
            Log("killing Actor is not Human");
            return;
        }
        switch (__instance.role)
        {
            case Actor.Roles.None:
                Log("Actor is None");
                break;
            case Actor.Roles.Air:
                Log("Actor is Air");
                _instance._pilotSave.A2AKills++;
                break;
            case Actor.Roles.Ground:
                Log("Actor is Ground");
                _instance._pilotSave.A2GKills++;
                break;
            case Actor.Roles.GroundArmor:
                _instance._pilotSave.A2GKills++;
                Log("Actor is GroundArmor");
                break;
            case Actor.Roles.Missile:
                //Log("Actor is Missile");
                break;
            case Actor.Roles.Ship:
                Log("Actor is Ship");
                _instance._pilotSave.A2ShipKills++;
                break;
            default:
                Log("Actor is default, something wrong has happened");
                break;
        }
    }

    public override void UnLoad()
    {
        // Destroy any objects
    }
}