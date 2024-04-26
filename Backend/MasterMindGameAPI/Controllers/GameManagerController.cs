using System.Text;
using MasterMind.Exceptions;
using MasterMind.Players;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Configuration;

namespace MasterMind.Controllers
{
    [Route("[controller]")]
    public class MasterMindController : Controller
    {
        private readonly GameManager _gameManager;

        private readonly HttpClient _client;
        private readonly string _authAPI = System.Configuration.ConfigurationManager.AppSettings["AuthEndPoint"] ?? "";


        private MatchView CreateMatchView(Match playerMatch, string playerID)
        {
            if (playerMatch.Players[0] is HPlayer humanPlayer && humanPlayer.PlayerID == playerID)
            {
                return new MatchView(FormatSequenceIntoString(playerMatch.CodifiedSequence), FormatIntoStringSequences(playerMatch.Sequences), playerMatch.Responses, "Crypter", playerMatch.Host.PlayerID, playerMatch.PlayerTurn, playerMatch.GameIsEnded);
            }
            else
            {
                return new MatchView(null, FormatIntoStringSequences(playerMatch.Sequences), playerMatch.Responses, "Decrypter", playerMatch.Host.PlayerID, playerMatch.PlayerTurn, playerMatch.GameIsEnded);
            }
        }

        private string[][] FormatIntoStringSequences(Match.Symbols[][] sequences)
        {
            string[][] sequencesToString = new string[sequences.Length][];
            for (byte i = 0; i < sequences.Length; i++)
            {
                if (sequences[i] == null) continue;
                string[] currentSequenceToString = new string[sequences[i].Length];
                for (byte j = 0; j < sequences[i].Length; j++)
                {
                    currentSequenceToString[j] = sequences[i][j].ToString();
                }
                sequencesToString[i] = currentSequenceToString;
            }
            return sequencesToString;
        }

        private string[] FormatSequenceIntoString(Match.Symbols[] sequence)
        {
            string[] formattedIntoStringSequence = new string[sequence.Length];
            for (byte i = 0; i < sequence.Length; i++)
            {
                formattedIntoStringSequence[i] = sequence[i].ToString().ToLower();
            }
            return formattedIntoStringSequence;
        }

        private Match.Symbols[] FormatSequenceIntoEnum(string[] codifiedSequence)
        {
            Match.Symbols[] formattedRequest = new Match.Symbols[codifiedSequence.Length];
            for (byte i = 0; i < codifiedSequence.Length; i++)
            {
                formattedRequest[i] = (Match.Symbols)Enum.Parse(typeof(Match.Symbols), codifiedSequence[i].ToUpper());
            }
            return formattedRequest;
        }

        private async Task<IActionResult> VerifyTokenAsync(string email, string token)
        {
            string authTokenVerify = $"{_authAPI}/User/verify";
            VerifyTokenRequest request = new(email, token);
            string jsonRequest = JsonConvert.SerializeObject(request);
            try
            {
                HttpContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(authTokenVerify, content);
                if (response.IsSuccessStatusCode) return Accepted();
                return Unauthorized();
            }
            catch
            {
                return NotFound();
            }
        }

        public MasterMindController(GameManager gameManager)
        {
            _client = new();
            _gameManager = gameManager;
        }

        [HttpPost("/StartMatch")]
        public async Task<IActionResult> StartMatch([FromBody] StartMatchRequest request)
        {
            try
            {
                IActionResult verifyTokenResponse = await VerifyTokenAsync(request.Email, request.Token);
                if (verifyTokenResponse is not AcceptedResult) return Unauthorized();

                Match playerMatch = _gameManager.StartMatch(request.Email, request.PlayerRole.ToUpper());
                return Ok(CreateMatchView(playerMatch, request.Email));
            }
            catch (NoMatchRetrived)
            {
                return BadRequest("Failed to load Last Player Match");
            }
        }

        [HttpGet("/Login")]
        public async Task<IActionResult> GetNonce([FromQuery] string email)
        {
            string authNonceURL = $"{_authAPI}/User/login/?email={email}";
            try
            {
                HttpResponseMessage response = await _client.GetAsync(authNonceURL);
                string nonce = await response.Content.ReadAsStringAsync();
                return Ok(nonce);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost("/Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            string authLogInURL = $"{_authAPI}/User/login";

            string jsonRequest = JsonConvert.SerializeObject(request, Formatting.None);
            HttpContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await _client.PostAsync(authLogInURL, content);
                if (!response.IsSuccessStatusCode) return Unauthorized();

                _gameManager.CheckUser(request.Email); 
                string logInResponse = await response.Content.ReadAsStringAsync();

                return Ok(logInResponse);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost("/SetSequence")]
        public async Task<IActionResult> SetSequence([FromBody] CrypterSetSequenceRequest request)
        {
            try
            {
                IActionResult verifyTokenResponse = await VerifyTokenAsync(request.PlayerID, request.Token);
                if (verifyTokenResponse is not AcceptedResult) return Unauthorized();

                Match.Symbols[] codifiedSequence = FormatSequenceIntoEnum(request.CodifiedSequence);

                Match playerMatch = _gameManager.SetMatchCodifiedSequence(request.PlayerID, codifiedSequence);
                return Ok(CreateMatchView(playerMatch, request.PlayerID));
            }
            catch (NoMatchRetrived)
            {
                return BadRequest("Need to start a match first");
            }
            catch (InvalidSymbol)
            {
                return BadRequest("Inserted Invalid Sequence");
            }
        }

        [HttpPost("/Decrypt")]
        public async Task<IActionResult> MakeDeCrypterMove([FromBody] DecrypterMove move)
        {
            try
            {
                IActionResult verifyTokenResponse = await VerifyTokenAsync(move.PlayerID, move.Token);
                if (verifyTokenResponse is not AcceptedResult) return Unauthorized();

                Match.Symbols[] formattedSequence = FormatSequenceIntoEnum(move.Sequence);

                Match playerMatch = _gameManager.MakeDecrypterMove(move.PlayerID, formattedSequence);
                return Ok(CreateMatchView(playerMatch, move.PlayerID));
            }
            catch (NoMatchRetrived)
            {
                return BadRequest($"{move.PlayerID} have to start a match first");
            }
            catch (NullReferenceException)
            {
                return NotFound($"Computer Error");
            }
            catch (InvalidSymbol)
            {
                return BadRequest($"{move.PlayerID} sent an invalid Sequence");
            }
            catch (InvalidMove)
            {
                return Unauthorized($"{move.PlayerID} trying to do a move but is not his turn!");
            }
        }

        [HttpPost("/Crypt")]
        public async Task<IActionResult> MakeCrypterMove([FromBody] CrypterMove move)
        {
            try
            {
                IActionResult verifyTokenResponse = await VerifyTokenAsync(move.PlayerID, move.Token);
                if (verifyTokenResponse is not AcceptedResult) return Unauthorized();

                Match playerMatch = _gameManager.MakeCrypterMove(move.PlayerID, move.Response);
                return Ok(CreateMatchView(playerMatch, move.PlayerID));
            }
            catch (NoMatchRetrived)
            {
                return BadRequest($"{move.PlayerID} have to start a match first");
            }
            catch (InvalidMove error)
            {
                if (error.Message != null && error.Message != "") return Unauthorized($"{error.Message}");
                return Unauthorized($"{move.PlayerID} trying to do a move but is not his turn!");
            }
            catch (SequenceNotInserted)
            {
                return BadRequest($"{move.PlayerID} Have to set the codifiedSequence First!");
            }
            catch (InvalidResponse)
            {
                return BadRequest($"{move.PlayerID} trying to insert an invalid Response");

            }
        }

        [HttpPost("/Match")]
        public async Task<IActionResult> GetPlayerLastMatch([FromBody] GetMatchRequest request)
        {
            try
            {
                IActionResult verifyTokenResponse = await VerifyTokenAsync(request.PlayerID, request.Token);
                if (verifyTokenResponse is not AcceptedResult) return Unauthorized();
                Match playerMatch = _gameManager.RestartMatch(request.PlayerID);
                return Ok(CreateMatchView(playerMatch, request.PlayerID));
            }
            catch (NoMatchRetrived)
            {
                return BadRequest($"{request.PlayerID} doasn't started any match");
            }
        }

        [HttpGet("/Leaderboard")]
        public IActionResult TakeLeaderboard()
        {
            return Ok(_gameManager.TakeLeaderboard());
        }

        [HttpGet("/Player")]
        public IActionResult GetPlayer([FromQuery] string playerID)
        {
            try
            {
                HPlayer player = _gameManager.RetrivePlayer(playerID);
                return Ok(player);
            }
            catch (NoPlayerRetrived)
            {
                return BadRequest($"{playerID} Not Found");
            }
        }

        [HttpGet("/Symbols")]
        public IActionResult GetAllSymbols()
        {
            return Ok(_gameManager.SymbolsAvaible());
        }
    }
}