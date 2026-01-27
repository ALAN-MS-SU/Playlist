using CaixaAPI.DB;
using CaixaAPI.Model.JWT;
using CaixaAPI.Model.TOTP;
using CaixaAPI.Model.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CaixaAPI.Controllers;

[ApiController]
[Route("/User")]
public class UserController(
    Context context,
    JWT jwt,
    TOTP totp
) : ControllerBase
{
    private readonly Context Context = context;

    private readonly JWT Jwt = jwt;

    private readonly TOTP Totp = totp;
    
    [HttpGet("{Email}")]
    public async Task<IActionResult> Get(string Email)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Email);
        var Res= await Totp.Count(Email);
        if (User == null) return BadRequest("Not Found.");
        return Ok(Res);
    }

    [HttpPost("{Email}")]
    public async Task<IActionResult> TwoFactor(string Email)
    {
        var User = await Context.Users.FirstOrDefaultAsync(user => user.Email == Email);
        if (User == null) return BadRequest("Not Found.");
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
        if (User.Secret == null) return Forbid("Secret not found.");
        var Count = await Totp.Count(Body.Email);
        if (Count >= 5) return StatusCode(403,"Limit exceeded.");
        Totp.Attempt(Body.Email);
        var Valid = Totp.Valid(User.Secret, Body.Code);
        if (Valid)
        {
            var JWT = Jwt.CreateJWT(User.ID);
            if (JWT == null)
                return Unauthorized("JWT Err.");
            return Ok(new { JWT });
        }
        return Unauthorized("Invalid Code.");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        var FindUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (FindUser != null) return Unauthorized("Email já está sendo usado.");
        await Context.Users.AddAsync(user);

        var Row = await Context.SaveChangesAsync();
        if (Row > 0) return StatusCode(201, "Usuário foi cadastrado");

        return BadRequest("Erro ao cadastrar usuário");
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateUser data)
    {
        var user = await Context.Users.FindAsync(data.ID);
        if (user == null) return Unauthorized("Usuaŕio não existe");
        var FindUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
        if (FindUser != null) return Unauthorized("Email já está sendo usado.");

        if (data.Name != null) user.Name = data.Name;
        if (data.Email != null) user.Email = data.Email;

        await Context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{ID}")]
    public async Task<IActionResult> Delete(int ID)
    {
        var user = await Context.Users.FindAsync(ID);
        if (user == null) return Unauthorized("Usuaŕio não existe");
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();
        return NoContent();
    }
}