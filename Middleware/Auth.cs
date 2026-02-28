using System.IdentityModel.Tokens.Jwt;
namespace CaixaAPI.Middleware;

public class Auth
{
    private readonly IConfiguration Config;
    private readonly RequestDelegate Next;

    public Auth(RequestDelegate Next,  IConfiguration Config)
    {
        this.Next = Next;
        this.Config = Config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var Token = context.Request.Cookies[this.Config["JWT:Name"]!];
        if (Token != null)
        {
             var Handler = new JwtSecurityTokenHandler();
             var JWT = Handler.ReadJwtToken(Token);
             if (context.Request.Method == "GET")
             {
                 var Sub = JWT.Claims.First(claim => claim.Type == "sub").Value;
                 var ID = context.Request.RouteValues["ID"]!.ToString();
                 if (ID != Sub)
                 {
                     context.Response.StatusCode = 403;
                     await context.Response.WriteAsync("No auth.");
                     return;
                 }
             }
             await Next(context);
             return;
        }
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("No auth.");
        return;
       
    }
}