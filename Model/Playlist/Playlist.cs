using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaixaAPI.Model.Playlist;

[Table("Playlist")]
public class Playlist
{
    [Key] [Required] [Column("ID")] public int ID { get; set; }

    [Required] [Column("PlaylistID")] public int PlaylistID { get; set; }

    [Column("Name")]
    [MaxLength(40)]
    [Required]
    public string Name { get; set; }

    [Required]
    [Column("User")]
    [ForeignKey(nameof(UserID))]
    public int UserID { get; set; }

    [Required] public User.User User { get; set; }

    [Required] [Column("Link")] public string Link { get; set; }
}

public class CreatePlayList
{
    public CreatePlayList(string Name, int User, string Link)
    {
        this.Name = Name;
        this.User = User;
        this.Link = Link;
    }

    public string Name { get; set; }
    public int User { get; set; }
    public string Link { get; set; }
}

public class UpdatePlayList
{
    public int ID { get; set; }

    public string Name { get; set; }
}

public class Item
{
    public Item(int ID, string Link)
    {
        this.ID = ID;
        this.Link = Link;
    }

    public int ID { get; set; }

    public string Link { get; set; }
}