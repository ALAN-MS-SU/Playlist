using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace CaixaAPI.Model.User;

[Table("Users")]
[Index(nameof(Email), IsUnique =  true)]
public class User 
{
    
    [Key]
    [Required]
    [Column("ID")]
    public int ID { get; set; }
    [Required]
    [MaxLength(40)]
    
    [Column("Name")]
    public string Name { get; set; } 
    [EmailAddress]
    [Required]
    [MaxLength(50)]
    
    [Column("Email")]
    public string Email { get; set; } 
    [Required]
    
    [Column("Password")]
    public string Password { get; set; }
    
    public User(){}
    protected User(int ID, string Email,string Name, string Password)
    {
        this.ID = ID;
        this.Name = Name;
        this.Email = Email;
        this.Password = Password;
    }
}

public class UpdateUser 
{
   
    public int ID { get; set; } 
    public string? Name { get; set; } 
    public string? Email { get; set; }
    public string? Password { get; set; }
                                        
}