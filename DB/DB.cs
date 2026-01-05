using CaixaAPI.Model.Playlist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CaixaAPI.Model.User;
namespace CaixaAPI.DB
{
    public class Context : DbContext
    {
     
 public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        
        public DbSet<Playlist> Playlists { get; set; }

       

       
    }
}