namespace CaixaAPI.Model.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
public class JWT
{
    private readonly IConfiguration Configuration;
    
    private readonly (
        string Audience,
        string Issuer,
        DateTime NotBefore,
        DateTime Expires,
        SigningCredentials Credentials
        ) Config;
    
    public JWT(IConfiguration configuration)
    {
        Configuration = configuration;
        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(Configuration["JWT:Expires"]!)
        );
        var Key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Configuration["JWT:Key"]!)
        );
        Config = (
            Audience: Configuration["JWT:Audience"]!,
            Issuer: Configuration["JWT:Issuer"]!,
            NotBefore: DateTime.UtcNow,
            Expires: expires,
            Credentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        );
    }

    private bool CheckConfig()
    {
       return this.Config switch
        {
            { Audience: null or "" } => throw new InvalidOperationException("Invalid Audience"),
            { Issuer: null or "" } => throw new InvalidOperationException("Invalid Issuer"),
            { Credentials: null } => throw new InvalidOperationException("Invalid Credentials"),
            _ => true
        };
        
    }
    public string? CreateJWT(int ID)
    {
        var Check = CheckConfig();
        if(!Check){return null;}
        var Claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, ID.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var Token = new JwtSecurityToken(claims: Claims, signingCredentials: Config.Credentials, audience: Config.Audience, 
            notBefore: Config.NotBefore, expires:  Config.Expires, issuer: Config.Issuer);

        return new JwtSecurityTokenHandler().WriteToken(Token);
    }

}