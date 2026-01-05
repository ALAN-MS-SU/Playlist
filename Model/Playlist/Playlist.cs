using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CaixaAPI.Model.User;
namespace CaixaAPI.Model.Playlist;
[Table("Playlist")]
public class Playlist
{
    [Key]
    [Required]
    [Column("ID")]
    public int ID { get; set; }
    
    [Required]
    [Column("PlaylistID")]
    public int PlaylistID { get; set; }
    
    [Column("Name")]
    [MaxLength(40)]
    [Required] 
    public string Name { get; set; }
    
    [Required] 
    [Column("User")]
    [ForeignKey(nameof(UserID) )]
    public int UserID { get; set; }
    
    [Required]
    public User.User User { get; set; }
    
    [Required]
    [Column("Link")]
    public string Link { get; set; }
}

public class CreatePlayList
{
    public string Name { get; set; }
    public int User { get; set; }
    
    public string Link { get; set; }
    
    public CreatePlayList(){}
    
}