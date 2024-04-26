using Newtonsoft.Json;

namespace MasterMind.Players
{
    public class HPlayer : Player
    {

        [JsonConstructor]
        public HPlayer(string playerID, int crypterTimes, int decrypterTimes, double lastScore, double playerScore) : base(lastScore, playerScore, crypterTimes, decrypterTimes, playerID) { }

        public HPlayer(string playerID) : base(playerID) { }
        public void UpdateLastScore(double score)
        {
            _lastScore = score;
            _playerScore += score;
        }
    }
}