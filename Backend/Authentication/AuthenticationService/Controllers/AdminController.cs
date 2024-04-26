using System.Net;
using Microsoft.AspNetCore.Mvc;

[Controller]
public class AdminController : ControllerBase
{
    UserManager _userManager;

    public AdminController(UserManager usermanager)
    {
        _userManager = usermanager;
    }

    // Update User
    [HttpPatch("[controller]/update")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
    {
        try
        {
            _userManager.VerifyToken(updateUserRequest.AdminEmail, updateUserRequest.AdminToken);

            _userManager.UpdateUser(updateUserRequest.UserToUpdateEmail, updateUserRequest.Name, updateUserRequest.Email);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("[controller]/GetUserReport")]
    public ActionResult<string> GetReport([FromQuery] string userEmail, [FromQuery] string adminEmail, [FromQuery] string adminToken)
    {
        try
        {
            _userManager.VerifyToken(adminEmail, adminToken);
            User retrivedUser = _userManager.Get(userEmail);
            return Ok($"{retrivedUser.GetInfo()} || Log Times: {retrivedUser.LogTime}");
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPatch("[controller]/ChangeUserStatus")]
    public IActionResult ChangeUserStatus([FromBody] ChangeUserStatusRequest request)
    {
        try
        {
            _userManager.VerifyToken(request.Email, request.Token);
            User admin = _userManager.Get(request.Email);
            User retrivedUser = _userManager.Get(request.UserEmail);
            if (!retrivedUser.ChangeStatus(request.Status, admin)) return BadRequest();
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet("[controller]/users")]
    public ActionResult<UsersResponse> GetAllUsers([FromQuery] string email, string token)
    {
        try
        {
            _userManager.VerifyToken(email, token);
            User admin = _userManager.Get(email);
            if (admin.GetRole() != global::User.Roles.ADMIN) return Unauthorized();
            List<User> usersList = _userManager.GetUsersView();
            List<UsersResponse> usersResponses = new();
            foreach (var user in usersList)
            {
                usersResponses.Add(new(user.GetInfo(), user.State.ToString(), user.GetRole().ToString()));
            }
            return Ok(usersResponses);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch
        {
            return BadRequest();
        }
    }
}