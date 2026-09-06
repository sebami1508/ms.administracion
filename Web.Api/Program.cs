using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Negocio.Gestion;
using System.Text;
using Web.Api.Extension;
using Web.Api.Filtro;
using Web.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.ObjectDependencyInjector();
builder.ConfiguracionRegionalFecha("es-CO");

builder.Services.AddControllers(option => {
    option.Filters.Add<ExceptionFilter>();
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("SecretApiJwtKey").Value)),
    };

    // Los WebSocket del navegador no permiten cabeceras: el token llega por query
    // string (?access_token=...). Solo se aplica a las rutas del hub.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var myConfig = builder.Configuration.GetSection("MyConfig").Get<MyConfig>();
builder.Services.AddSingleton(myConfig);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// WebSockets antes de HttpsRedirection para no romper el handshake del hub (evita 307).
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrdenesHub>("/hubs/ordenes");

app.Run();
