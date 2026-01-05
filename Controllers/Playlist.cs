using Microsoft.AspNetCore.Mvc;
using CaixaAPI.DB;
using CaixaAPI.Model.Playlist;

namespace CaixaAPI.Controllers;

[ApiController]
[Route("/Playlist")]
public class PlaylistController(Context context) : ControllerBase
{
    private Context Context = context;

    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePlayList playlist)
    {
        var user = await Context.Users.FindAsync(playlist.User);
        if (user == null)
        {
            return Unauthorized("Usuaŕio não existe");
        }
        var ID = Context.Playlists.Select(p=>(int?)p.PlaylistID).Max()??1;
        Console.WriteLine(ID.ToString());
        await Context.Playlists.AddAsync(new Playlist{PlaylistID = ID,Name = playlist.Name, UserID = playlist.User, Link = playlist.Link});
        int row = await Context.SaveChangesAsync();
        if (row > 0)
        {
            return StatusCode(201,"Playlist foi Criada");
        }
        return BadRequest("Erro ao criar playlist");
    }
}