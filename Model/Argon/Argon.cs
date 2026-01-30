using Isopoh.Cryptography.Argon2;
namespace CaixaAPI.Model.Argon;

public class Argon
{
    public string GenerateHash(string Password)
    {
        return Argon2.Hash(Password);
    }
    
    public bool Verify(string Hash, string Password)
    {
        return Argon2.Verify(Hash, Password);
    }
}