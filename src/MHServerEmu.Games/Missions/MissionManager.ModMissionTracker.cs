namespace MHServerEmu.Games.Missions
{
    public partial class MissionManager
    {
        public IEnumerable<Mission> GetAllMissions()
        {
            return _missionDict.Values;
        }
    }
}
