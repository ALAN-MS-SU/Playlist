
using CaixaAPI.Model.User;
namespace CaixaAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using CaixaAPI.DB;
using CaixaAPI.Model.JWT;
using Microsoft.EntityFrameworkCore;
[ApiController]
[Route("/User")]
public class UserController(Context context,JWT jwt) : ControllerBase
{
    private readonly Context Context = context;
    private readonly JWT Jwt = jwt;
    [HttpGet("{ID}")]
    public async Task<IActionResult> Get(int ID)
    {
        var Token = Jwt.CreateJWT(ID);
        if (Token == null)
        {
            return Unauthorized("T");
        }
        return Ok(Token);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        var FindUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (FindUser != null)
        {
            return Unauthorized("Email já está sendo usado.");
        }
        await Context.Users.AddAsync(user);
        
        int Row = await Context.SaveChangesAsync();
        if (Row > 0)
        {
            return StatusCode(201,"Usuário foi cadastrado");
        }

        return BadRequest("Erro ao cadastrar usuário");
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateUser data)
    {
        var user = await Context.Users.FindAsync(data.ID);
        if (user == null)
        {
            return Unauthorized("Usuaŕio não existe");
        }
        var FindUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
        if (FindUser != null)
        {
            return Unauthorized("Email já está sendo usado.");
        }

        if (data.Name != null)
        {
            user.Name = data.Name;
        }
        if (data.Email != null)
        {
            user.Email = data.Email;
        }

        await Context.SaveChangesAsync();
        return NoContent();

    }

    [HttpDelete("{ID}")]
    public async Task<IActionResult> Delete(int ID)
    {
        var user = await Context.Users.FindAsync(ID);
        if (user == null)
        {
            return Unauthorized("Usuaŕio não existe");
        }
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();
        return NoContent();
    }
    
}