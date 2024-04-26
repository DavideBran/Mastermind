using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


[Controller]
public class UserController : ControllerBase
{
    UserManager _userManager;

    public UserController(UserManager userManager)
    {
        _userManager = userManager;
    }

    // Login 
    [HttpGet("[controller]/login")]
    public ActionResult<string> GetNonce([FromQuery] string email)
    {
        try
        {
            string nonce = _userManager.GetNonce(email); 
            // even if the email is not found i'll sent back the noce
            return Ok(JsonConvert.SerializeObject(nonce));
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("[controller]/login")]
    public ActionResult<string> LogIn([FromBody] UserLogInRequest user)
    {
        try
        {
            User usertToAuthenticate = _userManager.Get(user.Email) ?? throw new UserNotRegistered();
            string token = _userManager.VerifyUser(usertToAuthenticate, user.codifiedPassword);
            return Ok(JsonConvert.SerializeObject(token));
        }
        catch (UserNotRegistered e)
        {
            return BadRequest(e.Message);
        }
        catch (UserNotAuthenticate)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    // TokenVerify
    [HttpPost("[controller]/verify")] 
    public IActionResult VerifyUserByToken([FromBody] VerifyTokenRequest request) 
    {
        try
        {
            _userManager.VerifyToken(request.Email, request.Token);
            return Accepted();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (UserNotRegisteredIntoApp e)
        {
            return BadRequest(e.Message);
        }
    }

    // Registration
    [HttpPost("[controller]/register")]
    public ActionResult ResgisterUser([FromBody] UserRegisterRequest userToRegister)
    {
        try
        {
            _userManager.RegisterUser(userToRegister.Email, userToRegister.Name);
            return Ok();
        }
        catch (AlreadyRegistered)
        {
            return BadRequest();
        }
    }

    [HttpGet("[controller]/GetRole")]
    public ActionResult<string> GetRole(string email)
    {
        try
        {
            User retrivedUser = _userManager.Get(email);
            return Ok(retrivedUser.GetRole().ToString());
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet("[controller]/Get")]
    public ActionResult<UserInfoResponse> GetUser([FromQuery] string email)
    {
        try
        {
            User retrivedUser = _userManager.Get(email);
            if (email == retrivedUser.Email) return Ok(new UserInfoResponse(email, retrivedUser.Name, retrivedUser.GetRole().ToString()));
            return Unauthorized();
        }
        catch
        {
            return BadRequest();
        }
    }
}