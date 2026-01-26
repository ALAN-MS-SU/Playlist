using CaixaAPI.Model.Playlist;
using CaixaAPI.Model.User;
using Microsoft.EntityFrameworkCore;

namespace CaixaAPI.DB;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Playlist> Playlists { get; set; }
}