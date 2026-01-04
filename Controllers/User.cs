using System.Net;
using CaixaAPI.Model.User;

namespace CaixaAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using CaixaAPI.Model;
using CaixaAPI.DB;
using Microsoft.EntityFrameworkCore;
[ApiController]
[Route("/User")]
public class UserController(Context context) : ControllerBase
{
    private readonly Context Context = context;
   
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var Users = await Context.Users.Select(user => new{user.ID,user.Email,user.Name}).ToListAsync();
       

        return Ok(Users);

    }

    [HttpPost]
    public async Task<IActionResult> PostUser([FromBody] User user)
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
    public async Task<IActionResult> PutUser([FromBody] UpdateUser data)
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
    
}