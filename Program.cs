using CaixaAPI.DB;
using CaixaAPI.Model.JWT;
using CaixaAPI.Model.TOTP;
using Microsoft.EntityFrameworkCore;
using CaixaAPI.Middleware;
using CaixaAPI.Model.TOTP.Access;
using CaixaAPI.Model.Argon;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDataProtection();
builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
 {
    var Config  = ConfigurationOptions.Parse(
         builder.Configuration.GetConnectionString("Redis")
         ?? throw new InvalidOperationException("Connection string 'Redis' not found.")
        );
        Config.ClientName = "PlaylistAPI";
    return ConnectionMultiplexer.Connect(Config);
 });
builder.Services.AddScoped<Argon>();
builder.Services.AddScoped<JWT>();
builder.Services.AddScoped<TOTP>();
builder.Services.AddScoped<TFAccess>();
builder.Services.AddScoped<SIAccess>();
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres") ??
        throw new InvalidOperationException("Connection string 'Postgres' not found.")
    )
);

builder.Services.AddOpenApi();
var app = builder.Build();
app.UseWhen(
    context =>
        !(
            (context.Request.Path.StartsWithSegments("/User") && 
             !context.Request.Path.StartsWithSegments("/User/Profile")) &&
            (context.Request.Method == HttpMethods.Get ||
             context.Request.Method == HttpMethods.Post) 
        ) && !(context.Request.Path.StartsWithSegments("/Playlist") && context.Request.Method == HttpMethods.Get),
    appBuilder =>
    {
        appBuilder.UseMiddleware<Auth>();
    }
);
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthorization();


app.Run();