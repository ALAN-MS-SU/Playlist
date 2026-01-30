using CaixaAPI.DB;
using CaixaAPI.Model.JWT;
using CaixaAPI.Model.TOTP;
using CaixaAPI.Model.User;
using CaixaAPI.Model.Argon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CaixaAPI.Controllers;

[ApiController]
[Route("/User")]
public class UserController(
    Context Context,
    JWT Jwt,
    TOTP Totp,
    Argon Argon,
    TFAccess TFAccess,
    SIAccess SIAccess,
    IConfiguration Configuration
) : ControllerBase
{
    private readonly Context Context = Context;

    private readonly JWT Jwt = Jwt;

    private readonly TOTP Totp = Totp;

    private readonly Argon Argon = Argon;
    
    private readonly TFAccess TFAccess = TFAccess;
    
    private readonly SIAccess SIAccess = SIAccess;
    
    private readonly  IConfiguration Configuration = Configuration;
    
    [HttpGet("{Email}")]
    public async Task<IActionResult> Get(string Email)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Email);
   
        if (User == null) return BadRequest("Not Found.");
        return NoContent();
    }

    [HttpGet("Profile/{ID}")]

    public async Task<IActionResult> GetProfile(int ID)
    {
        var User = await Context.Users.Select(user => new {user.ID,user.Email,user.Name,})
            .FirstOrDefaultAsync(user => user.ID == ID);
        if (User == null) return BadRequest("Not Found.");
        return Ok(User);
    }
    
    [HttpPost("QrCode/{Email}")]
    public async Task<IActionResult> QRCode(string Email)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Email);
        if (User == null) return BadRequest("User Not Found.");
        if (User.Secret == null)
        {
            var Secrets = Totp.CreateSecret();
            User.Secret = Secrets.Encrypt;
            await Context.SaveChangesAsync();
        }

        var QRCode = Totp.GenerateQRCode(User.Email, User.Secret, Totp.Issuer);
        return File(QRCode, "image/png");
    }

    [HttpPost("2FA")]
    public async Task<IActionResult> TwoFA([FromBody] TOPTCode Body)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Body.Email);
        if (User == null) return Unauthorized("User Not Found.");
        if (User.Secret == null) return StatusCode(403,"Secret not found.");
        var Count = await TFAccess.Count(Body.Email);
        if (Count >= int.Parse(Configuration["TOTP:Limit"]!)) return StatusCode(403,"Limit exceeded.");
        var Open = (await SIAccess.Count(Body.Email)) > 0;
        if(!Open) return StatusCode(403,"Credentials were not validated.");
        TFAccess.Attempt(Body.Email);
        var Valid = Totp.Valid(User.Secret, Body.Code);
        if (Valid)
        {
            SIAccess.Remove(Body.Email);
            var JWT = Jwt.CreateJWT(User.ID);
            if (JWT == null)
                return Unauthorized("JWT Err.");
            return Ok(new { JWT });
        }
        return Unauthorized("Invalid Code.");
    }

    [HttpPost("SingIn")]
    public async Task<IActionResult> SignIn([FromBody] Credentials Body)
    {
        Console.WriteLine(Body.Email);
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Body.Email);
        if (User == null) return Unauthorized("User Not Found.");
        var Count = await SIAccess.Count(Body.Email);
        if (Count >= int.Parse(Configuration["TOTP:Limit"]!)) return StatusCode(403,"Limit exceeded.");
        SIAccess.Attempt(Body.Email);
        if (Argon.Verify(User.Password, Body.Password))
        {
            SIAccess.Attempt(Body.Email);
            return Accepted();
        }
        return Unauthorized("Invalid Password.");
    } 
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User Body)
    {
        var FindUser = await Context.Users.FirstOrDefaultAsync(User => User.Email == Body.Email);
        if (FindUser != null) return Unauthorized("Email is already in use.");
        Body.Password = Argon.GenerateHash(Body.Password);
        await Context.Users.AddAsync(Body);
        var Row = await Context.SaveChangesAsync();
        if (Row > 0) return StatusCode(201, "User was been created.");
        return BadRequest("Create user err.");
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateUser Body)
    {
        var user = await Context.Users.FindAsync(Body.ID);
        if (user == null) return Unauthorized("User not found.");
        var FindUser = await Context.Users.FirstOrDefaultAsync(User => User.Email == Body.Email);
        if (FindUser != null) return Unauthorized("Email is already in use.");
        if (Body.Name != null) user.Name = Body.Name;
        if (Body.Email != null) user.Email = Body.Email;
        await Context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{ID}")]
    public async Task<IActionResult> Delete(int ID)
    {
        var User = await Context.Users.FindAsync(ID);
        if (User == null) return Unauthorized("User not found.");
        Context.Users.Remove(User);
        await Context.SaveChangesAsync();
        return NoContent();
    }
}