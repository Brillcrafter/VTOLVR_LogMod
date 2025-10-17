namespace vtolLogMod;

public class PilotSave
{
    public int A2AKills;
    public int A2GKills;
    public int A2ShipKills;
    public int NumberOfTakeoffs;
    public int NumberOfLandings;
    public int NumberOfEjections;
    public int NumberOfDeaths;
    public int NumberOfSuccessfulMissions;
    public int NumberOfFailedMissions;

    public PilotSave(int a2AKills, int a2GKills, int a2ShipKills ,int numberOfTakeoffs ,int numberOfLandings, 
        int numberOfEjections, int numberOfDeaths, int numberOfSuccessfulMissions, int numberOfFailedMissions)
    {
        A2AKills = a2AKills;
        A2GKills = a2GKills;
        A2ShipKills = a2ShipKills;
        NumberOfTakeoffs = numberOfTakeoffs;
        NumberOfLandings = numberOfLandings;
        NumberOfEjections = numberOfEjections;
        NumberOfDeaths = numberOfDeaths;
        NumberOfSuccessfulMissions = numberOfSuccessfulMissions;
        NumberOfFailedMissions = numberOfFailedMissions;
    }
}