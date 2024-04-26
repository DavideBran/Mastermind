using System.Collections.Concurrent;
using fileManager;
using MasterMind.Exceptions;
using MasterMind.LeaderBoard;
using MasterMind.Players;

namespace MasterMind
{
    public class GameManager
    {
        private ConcurrentBag<Match> _matches;
        private LeaderBoardManager _leaderBoard;
        private FileManager _fileManager;
        
        // Attribute For Data saving
        private DateTime _lastSaveDate;

        private HPlayer GetPlayer(string playerID)
        {
            HPlayer? retrivedPlayer = _leaderBoard.Leaderboard.FirstOrDefault((HPlayer player) => player.PlayerID == playerID);
            if (retrivedPlayer != null) return retrivedPlayer;

            // if this is the first time that player enter the game
            HPlayer newPlayer = new(playerID);
            _leaderBoard.UpdateLeadearboard(newPlayer);
            return newPlayer;
        }

        private bool MatchEnded(Match playerMatch, string playerID)
        {
            // removing the match from the list, and updating the leaderboard
            RemoveMatch(playerMatch);

            // taking the player score
            Player[] players = playerMatch.Players;
            HPlayer? currentPlayer = null;
            foreach (var player in players)
            {
                if (player is HPlayer matchPlayer && matchPlayer.PlayerID == playerID) currentPlayer = matchPlayer;
            }

            if (currentPlayer == null) throw new Exception();

            _leaderBoard.UpdateLeadearboard(currentPlayer);
            return playerMatch.GameIsEnded;
        }

        private ConcurrentBag<Match> LoadMatches()
        {
            List<Match>? retrivedMatches = (List<Match>?)_fileManager.PoPList<Match>("matches.json");
            if (retrivedMatches == null) return new();
            else return new ConcurrentBag<Match>(retrivedMatches);
        }

        private void SaveMatches()
        {
            try
            {
                _fileManager.SaveList<Match>(_matches.ToList(), "matches.json");
                _lastSaveDate = DateTime.Now;
            }
            catch (ArgumentException)
            {
                _fileManager.SaveList<Match>(_matches.ToList(), "matches.json".Trim());
                _lastSaveDate = DateTime.Now;
            }
        }

        private void RemoveMatch(Match playerMatch)
        {
            Parallel.ForEach(_matches,
                (match) =>
                {
                    if (match == playerMatch)
                    {
                        if (!_matches.TryTake(out Match? matchRemoved)) throw new Exception();
                    }
                });

            if (_lastSaveDate >= _lastSaveDate.AddHours(1))
            {
                SaveMatches();
                _lastSaveDate = DateTime.Now;
            }
        }

        private Match? GetPlayerMatch(string playerID)
        {
            return _matches.FirstOrDefault((Match match) => match.Host.PlayerID == playerID);
        }

        public Match StartMatch(string player, string role)
        {

            // if the player have some Match in pending i'll remove it from the bag
            Match? playerMatch = GetPlayerMatch(player);

            if (playerMatch != null) RemoveMatch(playerMatch);

            HPlayer hPlayer = GetPlayer(player);

            // selecting the IA role (the opposite of the Human Player Role)
            IAPlayer.Role IArole = role.ToUpper() == "CRYPTER" ? IAPlayer.Role.DECRYPTER : IAPlayer.Role.CRYPTER;

            // the first player of the players array is the Crypter
            if (role.ToUpper() == "DECRYPTER") playerMatch = new(new Player[] { new IAPlayer(IArole), hPlayer }, hPlayer);
            else playerMatch = new(new Player[] { hPlayer, new IAPlayer(IArole) }, hPlayer);

            _matches.Add(playerMatch);

            if (_lastSaveDate >= _lastSaveDate.AddHours(1))
            {
                SaveMatches();
                _lastSaveDate = DateTime.Now;
            }

            return playerMatch;
        }

        public Match RestartMatch(string player)
        {
            return GetPlayerMatch(player) ?? throw new NoMatchRetrived();
        }

        public Match SetMatchCodifiedSequence(string playerID, Match.Symbols[] codifiedSequence)
        {
            Match playerMatch = GetPlayerMatch(playerID) ?? throw new NoMatchRetrived();
            playerMatch.SetMatchCodifiedSequence(codifiedSequence, playerID);

            if (_lastSaveDate >= _lastSaveDate.AddHours(1))
            {
                SaveMatches();
                _lastSaveDate = DateTime.Now;
            }

            return playerMatch;
        }

        public Match MakeDecrypterMove(string playerID, Match.Symbols[] sequence)
        {
            try
            {
                // taking the match, if that doasn't exist the player is tring to make an invalid move (have to start a match before)
                Match playerMatch = GetPlayerMatch(playerID) ?? throw new NoMatchRetrived();
                // player is Decrypter

                // Making the move as Decrypter
                playerMatch.DecrypterMove(sequence);

                // checking if after the move the match Ended
                if (playerMatch.GameIsEnded) MatchEnded(playerMatch, playerID);

                if (_lastSaveDate >= _lastSaveDate.AddHours(1))
                {
                    SaveMatches();
                    _lastSaveDate = DateTime.Now;
                }

                return playerMatch;
            }
            catch (NoMatchRetrived error)
            {
                Console.WriteLine($"{playerID} Tried to make a move on a different match");
                throw error;
            }
            catch (InvalidSymbol error)
            {
                Console.WriteLine($"{playerID} Sent invalid Sequence");
                throw error;
            }
            catch (InvalidMove error)
            {
                Console.WriteLine($"{playerID} Made An Invalid Move");
                throw error;
            }
        }

        public Match MakeCrypterMove(string playerID, string response)
        {
            try
            {
                Match playerMatch = GetPlayerMatch(playerID) ?? throw new NoMatchRetrived();

                playerMatch.CrypterMove(response);

                // checking if after the move the match Ended
                if (playerMatch.GameIsEnded) MatchEnded(playerMatch, playerID);

                if (_lastSaveDate >= _lastSaveDate.AddHours(1))
                {
                    SaveMatches();
                    _lastSaveDate = DateTime.Now;
                }

                return playerMatch;
            }
            catch (NoMatchRetrived error)
            {
                Console.WriteLine($"{playerID} Tried to make a move on a different match\n{error.StackTrace}");
                throw error;
            }
            catch (InvalidMove error)
            {
                Console.WriteLine($"{playerID} Made An Invalid Move\n{error.StackTrace}");
                throw error;
            }
        }

        public void CheckUser(string playerID)
        {
            _leaderBoard.UpdateLeadearboard(new HPlayer(playerID));
        }

        // Information Getter Methods 
        public HPlayer RetrivePlayer(string playerID)
        {
            return _leaderBoard.Leaderboard.Find((player) => player.PlayerID == playerID) ?? throw new NoPlayerRetrived();
        }

        public List<HPlayer> TakeLeaderboard()
        {
            return _leaderBoard.Leaderboard;
        }

        public List<string> SymbolsAvaible()
        {
            return Enum.GetNames(typeof(Match.Symbols)).ToList();
        }

        public GameManager()
        {
            // Loading Data from File
            _leaderBoard = LeaderBoardManager.LeaderBoardManagerInstance;
            _fileManager = new("MasterMind");
            _matches = LoadMatches();
            _lastSaveDate = DateTime.Now;
        }
    }
}

