using Newtonsoft.Json;

namespace MasterMind.Players
{
    abstract public class Player
    {
        protected double _lastScore;
        protected double _playerScore;
        private int _timesPlayedAsCrypter;
        private int _timesPlayedAsDecrypter;
        private string _playerID;

        public string PlayerID { get => _playerID; }
        public int CrypterTimes { get => _timesPlayedAsCrypter; }
        public int DecrypterTimes { get => _timesPlayedAsDecrypter; }
        public double LastScore { get => _lastScore; }
        public double PlayerScore { get => _playerScore; }

        public void UpdateDecryptTimes()
        {
            _timesPlayedAsDecrypter++;
        }

        public void UpdateCryptTimes()
        {
            _timesPlayedAsCrypter++;
        }

        public Player(string playerID)
        {
            _lastScore = 0;
            _playerScore = 0;
            _timesPlayedAsCrypter = 0;
            _timesPlayedAsDecrypter = 0;
            _playerID = playerID;
        }

        public Player()
        {
            _lastScore = 0;
            _playerScore = 0;
            _timesPlayedAsCrypter = 0;
            _timesPlayedAsDecrypter = 0;
            _playerID = "";
        }

        [JsonConstructor]
        public Player(double lastScore, double playerScore, int timesPlayedAsCrypter, int timesPlayedAsDecrypter, string playerID)
        {
            _lastScore = lastScore;
            _playerScore = playerScore;
            _timesPlayedAsCrypter = timesPlayedAsCrypter;
            _timesPlayedAsDecrypter = timesPlayedAsDecrypter;
            _playerID = playerID;
        }
    }
}