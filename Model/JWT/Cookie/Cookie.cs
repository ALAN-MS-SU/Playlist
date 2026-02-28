namespace CaixaAPI.Model.JWT.Cookie;

public class Cookie
{
    private readonly IConfiguration Configuration;
    
    public Cookie(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public CookieOptions GetConfig()
    {
        var expires = DateTime.UtcNow.AddDays(
            double.Parse(Configuration["JWT:Expires"]!)
        );
        return new CookieOptions
        {
            Expires = expires,
            HttpOnly = Boolean.Parse(Configuration["JWT:HttpOnly"]!),
            Secure = Boolean.Parse(Configuration["JWT:Secure"]!),
            SameSite = SameSiteMode.Strict,
            IsEssential = Boolean.Parse(Configuration["JWT:IsEssential"]!)
        };
    }
}