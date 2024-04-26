using System.Collections.Concurrent;
using fileManager;
using MasterMind.Players;

namespace MasterMind.LeaderBoard
{
    public class LeaderBoardManager
    {
        private ConcurrentBag<HPlayer> _players;
        private static LeaderBoardManager? _instance = null;
        private FileManager _fileManager;

        private DateTime _lastSaveDate;
        private static object _instanceLock = new();

        private void SaveLeaderBoard()
        {
            try
            {
                _fileManager.SaveList<HPlayer>(_players.ToList(), "leaderboard.json");
                _lastSaveDate = DateTime.Now;
            }
            catch (ArgumentNullException)
            {
                _fileManager.SaveList<HPlayer>(_players.ToList(), "leaderboard.json".Trim());
                _lastSaveDate = DateTime.Now;
            }
        }

        private int ComparePlayerByScore(Player p1, Player p2)
        {
            // For sorting the list i have to implent a comparison method for the players, 
            // the list will be sorted from the highest score to the lowest

            if (p1.PlayerScore > p2.PlayerScore) return 1;
            if (p1.PlayerScore < p2.PlayerScore) return -1;
            else return 0;
        }

        public static LeaderBoardManager LeaderBoardManagerInstance
        {
            get
            {
                // first check on instance variable to avoid usless lock
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new LeaderBoardManager();
                    }
                }
                return _instance;
            }
        }

        public List<HPlayer> Leaderboard { get => _players.ToList(); }

        private LeaderBoardManager()
        {
            // loading LeaderBoard
            _fileManager = new("MasterMind");
            _players = LoadPlayers();

            _lastSaveDate = DateTime.Now;
        }

        private ConcurrentBag<HPlayer> LoadPlayers()
        {
            List<HPlayer>? retrivedPlayers = (List<HPlayer>?)_fileManager.PoPList<HPlayer>("leaderboard.json");
            if (retrivedPlayers == null) return new();
            else return new ConcurrentBag<HPlayer>(retrivedPlayers);
        }

        public void UpdateLeadearboard(HPlayer player)
        {
            if (_players.FirstOrDefault((leaderboardPlayer) => leaderboardPlayer.PlayerID == player.PlayerID) == default(HPlayer)) _players.Add(player);
            List<HPlayer> ordinatedPlayers = _players.ToList();
            ordinatedPlayers.Sort(ComparePlayerByScore);

            _players = new ConcurrentBag<HPlayer>(ordinatedPlayers);

            if (_lastSaveDate >= _lastSaveDate.AddHours(1))
            {
                SaveLeaderBoard();
                _lastSaveDate = DateTime.Now;
            }
        }
    }
}