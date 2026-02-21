using CaixaAPI.DB;
using CaixaAPI.Model.TOTP;
using CaixaAPI.Model.Argon;
using CaixaAPI.Model.TOTP.Access;
using CaixaAPI.Model.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CaixaAPI.Controllers;

[ApiController]
[Route("/User/Password")]
public class PasswordController(
    Context Context,
    TOTP Totp,
    Argon Argon,
    PAccess PAccess,
    IConfiguration Configuration
) : ControllerBase
{
    private readonly Context Context = Context;

    private readonly TOTP Totp = Totp;

    private readonly Argon Argon = Argon;
    
    private readonly PAccess PAccess = PAccess;
    
    private readonly  IConfiguration Configuration = Configuration;

    [HttpPost("{Email}")]
    public async Task<IActionResult> Init(string Email)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Email);
        if(User == null) return Unauthorized("User Not Found.");
        if (User.Secret == null) return StatusCode(403,"Secret not found.");
        var Count = await PAccess.Count(Email,true);
        if (Count >= int.Parse(Configuration["TOTP:Limit"]!)) return StatusCode(403,"Limit exceeded.");
        PAccess.Open(Email);
        return NoContent();
    }
    [HttpPost("2FA")]
    public async Task<IActionResult> TwoFA([FromBody] TOPTCode Body)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Body.Email);
        if (User == null) return Unauthorized("User Not Found.");
        if (User.Secret == null) return StatusCode(403,"Secret not found.");
        var Count = await PAccess.Count(Body.Email, true);
        if (Count >= int.Parse(Configuration["TOTP:Limit"]!)) return StatusCode(403,"Limit exceeded.");
        if(Count < 1) return StatusCode(403,"Credentials were not validated.");
        PAccess.Attempt(Body.Email);
        var Valid = Totp.Valid(User.Secret, Body.Code);
        if (Valid)
        {
            return Accepted();
        }
        return Unauthorized("Invalid Code.");
    }
    [HttpPatch]
    public async Task<IActionResult> ChangePassword([FromBody] Credentials Body)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Body.Email);
        if (User == null) return Unauthorized("User Not Found.");
        var Count = await PAccess.Count(Body.Email);
        if (Count >= int.Parse(Configuration["TOTP:Limit"]!)) return StatusCode(403,"Limit exceeded.");
        if(Count < 1) return StatusCode(403,"Credentials were not validated.");
        var Hash = Argon.GenerateHash(Body.Password);
        User.Password = Hash;
        var Save = await Context.SaveChangesAsync();
        PAccess.Remove(Body.Email);
        if (Save > 0) return NoContent();
        return BadRequest("Update Password Failed.");
    }
}
