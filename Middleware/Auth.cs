namespace CaixaAPI.Middleware;

public class Auth
{
    private readonly RequestDelegate Next;

    public Auth(RequestDelegate Next)
    {
        this.Next = Next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
        var Token = context.Request.Cookies["PAPI-Token"];

        if (Token == null)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("No auth.");
          return;

        }
        
        await Next(context);
    }
}