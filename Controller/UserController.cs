using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("names")]
    public async Task<IActionResult> GetNames([FromBody] string[] ids)
    {
        var users = await Task.WhenAll(
            ids.Select(id => _userManager.FindByIdAsync(id))
        );

        return Ok(users
            .Where(u => u != null)
            .Select(u => u!.UserName));
    }
}
