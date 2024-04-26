using System.Runtime.Serialization;
using MasterMind.Exceptions;
using MasterMind.Players;
using Newtonsoft.Json;

namespace MasterMind
{
    public class Match
    {
        private const int MAXTRY = 9;
        private const int SEQUENCEDIM = 4;

        public enum Symbols { WHITE, RED, BLUE, GREEN, PURPLE, GRAY, ORANGE, PINK };

        // turn can be from 1 to 9
        [JsonProperty]
        private byte _turns = 0;
        [JsonProperty]
        private bool _firstMove = true;
        private bool _playerTurn = true; // this will be used to identify wich player have to play now (true for Crypter false for Decrypter)

        [JsonProperty]
        private Symbols[] _codifiedSequence;
        private HPlayer _host;

        // Player array is formed from 2 player that can be either HPlayer and AI or HPlayer and HPlayer
        [JsonProperty]
        private Player[] _players;

        // This array will be formed from an array of 9 (that are all the try avaible for the Decrypter) and for each cell it will be present an array of 4 (the try of the player);
        [JsonProperty]
        private Symbols[][] _sequences = new Symbols[MAXTRY][];

        // as the previous this array will be formed from the 9 try and the response for that try (the response is of the form of (1B2W, 1 position and colour right and 2 colour right)
        [JsonProperty]
        private string[] _responses = new string[MAXTRY];

        [JsonIgnore]
        public Symbols[][] Sequences
        {
            get => _sequences;
        }

        [JsonIgnore]
        public string[] Responses { get => _responses; }

        [JsonIgnore]
        public Symbols[] CodifiedSequence
        {
            get => _codifiedSequence;
        }

        public bool PlayerTurn { get => _playerTurn; }

        public HPlayer Host { get => _host; }
        public Player[] Players { get => _players; }

        private bool ValidateResponse(string response)
        {
            byte B = 0, R = 0;
            for (byte i = 0; i < _codifiedSequence.Length; i++)
            {
                if (_codifiedSequence[i] == _sequences[_turns][i]) B++;
                else
                {
                    for (byte j = 0; j < _sequences[_turns].Length; j++)
                    {
                        if (_codifiedSequence[i] == _sequences[_turns][j])
                        {
                            R++;
                            break;
                        }
                    }
                }
            }

            return $"{B}B {R}R" == response.ToUpper();
        }

        private bool ValidateSequence(Symbols[] sequence)
        {
            for (byte i = 0; i < sequence.Length; i++)
            {
                var symbol = sequence[i];
                for (int j = i + 1; j < sequence.Length; j++)
                {
                    if (symbol == sequence[j]) return false;
                }
            }
            return true;
        }

        public bool GameIsEnded { get; private set; } = false;

        private void ChangeTurn() { _playerTurn = !_playerTurn; }

        private void GameEnded()
        {
            GameIsEnded = true;
            _players[0].UpdateCryptTimes();
            _players[1].UpdateDecryptTimes();
            // checking if the player is a real player, if it is it i'll set the score!
            if (_players[0] is not IAPlayer) SetCrypterScore();
            if (_players[1] is not IAPlayer) SetDecrypterScore();
        }

        private void SetDecrypterScore()
        {
            // the decrypter score is: the number of symble avaibles divided by the sequence lenght + the turns that the player used to guess the code
            if (_players[1] is not HPlayer decrypterPlayer) return;
            // to give an importance at the story of the player i'm adding the last score, and divide all by two, notice that if this is the first match of the player
            // and he have a lot of luck (suppose that he guessed the sequence at the first try) it will be 8 / 5 = 1.6 => 1.6 + 0 / 2 = 0.8
            double decrypterScore = (((double)Enum.GetNames(typeof(Symbols)).Length / (_sequences[_turns].Length + _turns)) + decrypterPlayer.LastScore) / 2;
            decrypterPlayer.UpdateLastScore(decrypterScore);
        }

        private void SetCrypterScore()
        {
            // the crypter score uses the length of the sequence he used + the turns that the decrypter used to guess the sequence, divided by the number of allowed symbols for the match 
            if (_players[0] is not HPlayer crypterPlayer) return;

            // notice that if the player is playing crypter against the IA he'll have a bonus on the score: 
            if (_players[1] is IAPlayer)
            {
                // the main problem of a match played as Crypter is that the computer can spent 5 turn or 6 or 7 with the same sequence
                // inserted so i'll give the user a score of 0.2 as a gift for the player
                crypterPlayer.UpdateLastScore(0.2);
            }
        }

        private void CrypterIsAI()
        {
            if (_players[0] is IAPlayer IA)
            {
                _firstMove = !_firstMove;
                _codifiedSequence = IA.TakeSequence();
                ChangeTurn();
            }
        }

        public void DecrypterMove(Symbols[] sequence)
        {
            if (_playerTurn || GameIsEnded) throw new InvalidMove();

            // checking if the move if valid
            if (sequence.Length < _codifiedSequence.Length) throw new InvalidSymbol();
            // if the Sequence sented is bigger then the _codifiedSequence setted, i'll consider the first _codifiedSequence.Lenght value
            if (sequence.Length > _codifiedSequence.Length) _sequences[_turns] = sequence.Take(_codifiedSequence.Length).ToArray();
            // sequence is the exact Length
            else _sequences[_turns] = sequence;

            // switching the turn to the Crypter
            ChangeTurn();

            // making the IA move (if the next player is an IA)
            if (_players[0] is IAPlayer otherPlayer)
            {
                string response = otherPlayer.MakeMoveAsCrypter(sequence);
                CrypterMove(response);
            }
        }

        public void CrypterMove(string response)
        {
            // Validating the move (checking the turn, if the game is already ended, if the sequence has been setted)
            if (!_playerTurn || GameIsEnded || _firstMove || _sequences[_turns] == null)
            {
                if (_firstMove) throw new SequenceNotInserted();
                if (GameIsEnded) throw new InvalidMove("The game is already Ended");
                throw new InvalidMove();
            }
            // Validating the response, Checking if the response sent is a correct response
            if (!ValidateResponse(response)) throw new InvalidResponse($"Inserted an Invalid response");

            _responses[_turns] = response.ToUpper();

            // Match end 
            if (_turns == MAXTRY - 1 || response.ToUpper() == $"{SEQUENCEDIM}B 0R")
            {
                GameEnded();
                return;
            }

            // notice that the turn is completed when the Crypter have Done is Move. (in other word we are assuming that the turn is completed when whe have a Decrypter Try and the Crypter Response)
            _turns++;

            // switching the turn to the Decrypter
            ChangeTurn();
            if (_players[1] is IAPlayer IA) DecrypterMove(IA.MakeMoveAsDecrypter(_responses[_turns - 1]));
        }

        public void SetMatchCodifiedSequence(Symbols[] sequence, string playerID)
        {
            // Checking that this is the first move, and the crypter player is the one that is trying to set the sequence
            if (_firstMove && _players[0] is HPlayer player && player.PlayerID == playerID)
            {
                // checking if the sequence is valid
                if (sequence.Length != SEQUENCEDIM || !ValidateSequence(sequence)) throw new InvalidSymbol();
                _codifiedSequence = sequence;
                _firstMove = !_firstMove;
                ChangeTurn();
                if (_players[1] is IAPlayer otherPlayer) DecrypterMove(otherPlayer.GetNextSequence());
                return;
            }
        }

        public Match(Player[] players, HPlayer host)
        {
            if (players.Length != 2) throw new TooManyPlayer();
            _codifiedSequence = new Match.Symbols[SEQUENCEDIM];
            // check if the Crypter is an AI, if it is i have to set the codified sequence so the player can sent the try
            _players = players;
            CrypterIsAI();
            _host = host;
        }

        [JsonConstructor]
        public Match(Player[] players, HPlayer host, byte turns, bool playerTurn, bool firstMove, Symbols[][] sequences, Symbols[] codifiedSequence, string[] responses)
        {
            _players = players;
            _host = host;
            _turns = turns;
            _playerTurn = playerTurn;
            _firstMove = firstMove;
            _sequences = sequences;
            _codifiedSequence = codifiedSequence;
            _responses = responses;
        }
    }
}