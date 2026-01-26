using CaixaAPI.DB;
using CaixaAPI.Model.Playlist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaixaAPI.Controllers;

[ApiController]
[Route("/Playlist")]
public class PlaylistController(Context context) : ControllerBase
{
    private readonly Context Context = context;

    [HttpGet("{ID}")]
    public async Task<IActionResult> GetPlaylist(int ID)
    {
        var FullList = await Context.Playlists.Where(playlist => playlist.PlaylistID == ID)
            .Select(playlist => new
            {
                playlist.ID,
                playlist.PlaylistID,
                playlist.Name, User = playlist.User.Name,
                playlist.Link
            })
            .ToListAsync();
        if(FullList.Count < 1) return BadRequest("Playlist not found.");
        var Playlist = new
        {
            ID = FullList[0].PlaylistID,
            FullList[0].Name,
            FullList[0].User,
            Items = FullList.Select(playlist => new { playlist.ID, playlist.Link })
        };
        return Ok(Playlist);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var Playlists = await Context.Playlists.GroupBy(playlist => playlist.PlaylistID)
            .Select(playlist => new
            {
                ID = playlist.Key,
                playlist.First().Name, User = playlist.First().User.Name
            }).ToListAsync();

        return Ok(Playlists);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePlayList playlist)
    {
        var user = await Context.Users.FindAsync(playlist.User);
        if (user == null) return Unauthorized("Usuaŕio não existe");

        var ID = Context.Playlists.Select(p => (int?)p.PlaylistID).Max() ?? 1;
        await Context.Playlists.AddAsync(
            new Playlist { PlaylistID = ID, Name = playlist.Name, UserID = playlist.User, Link = playlist.Link });
        var row = await Context.SaveChangesAsync();
        if (row > 0) return StatusCode(201, "Playlist foi Criada");

        return BadRequest("Erro ao criar playlist");
    }

    [HttpPost("Item")]
    public async Task<IActionResult> AddItem([FromBody] Item Body)
    {
        var Playlist = await Context.Playlists.FirstOrDefaultAsync(playlist => playlist.PlaylistID == Body.ID);
        if (Playlist == null) return Unauthorized("Playlist not found.");

        await Context.Playlists.AddAsync(
            new Playlist { PlaylistID = Body.ID, Name = Playlist.Name, UserID = Playlist.UserID, Link = Body.Link });
        var row = await Context.SaveChangesAsync();
        if (row > 0)
            return StatusCode(201, "Item has been added.");
        return BadRequest("Error when adding new item.");
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePlayList Body)
    {
        var Playlists = await Context.Playlists.Where(playlist => playlist.PlaylistID == Body.ID).ToListAsync();
        if (Playlists.Count < 1) return Unauthorized("Playlist not found.");

        foreach (var playlist in Playlists) playlist.Name = Body.Name;

        var row = await Context.SaveChangesAsync();
        if (row > 0)
            return NoContent();
        return BadRequest("Error when updating playlist.");
    }

    [HttpDelete("{ID}")]
    public async Task<IActionResult> DeletePlaylist(int ID)
    {
        var Playlists = await Context.Playlists.Where(playlist => playlist.PlaylistID == ID).ToListAsync();
        if (Playlists.Count < 1) return Unauthorized("Playlist not found.");

        Context.Playlists.RemoveRange(Playlists);
        var row = await Context.SaveChangesAsync();
        if (row > 0)
            return NoContent();
        return BadRequest("Error when deleting playlist.");
    }
    [HttpDelete("Item/{ID}")]
    public async Task<IActionResult> DeleteItem(int ID)
    {
        var Playlist = await Context.Playlists.FirstOrDefaultAsync(playlist => playlist.ID == ID);
        if (Playlist == null) return Unauthorized("Item not found.");
        Context.Playlists.Remove(Playlist);
        var row = await Context.SaveChangesAsync();
        if (row > 0)
            return NoContent();
        return BadRequest("Error when deleting item.");
    }

}