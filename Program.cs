using Microsoft.EntityFrameworkCore;
using CaixaAPI.DB;
using CaixaAPI.Model.JWT;
using CaixaAPI.Model.TOPT;
//using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDataProtection();
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//  
//     options.ConfigurationOptions = ConfigurationOptions.Parse(
//         builder.Configuration.GetConnectionString("Redis")
//         ??throw new InvalidOperationException("Connection string 'Redis' not found.")
//         );
//     options.InstanceName = "PlaylistAPI";
// });

builder.Services.AddScoped<JWT>();
builder.Services.AddScoped<TOPT>();

builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres") ?? 
        throw new InvalidOperationException("Connection string 'Postgres' not found.")
    )
);

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    
    app.MapOpenApi();
}
app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthorization();


app.Run();
