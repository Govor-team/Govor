using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Govor.API.Common.Extensions;
using Govor.API.Hubs;
using Govor.Application.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

builder.AddLogger();// Serilog

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
#else
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#endif

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("secrets/firebase-adminsdk.json")
    // или FromStream(File.OpenRead("firebase-adminsdk.json"))
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.Configure<JwtAccessOption>(configuration.GetSection(nameof(JwtAccessOption)));

// Add services
builder.Services.AddSignalRConf();// signalR

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtAccessOption:SecretKey"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/chats"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(); 

builder.Services.AddControllers();

// Init DI
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddValidators();

builder.Services.AddOptionsConfiguration(configuration);

builder.Services.AddGovorDbContext(configuration); // GovorDbContext init

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Govor API", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
    //builder.WebHost.UseUrls("http://192.168.1.107:8080");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Map("/server/ping",
    () => new OkResult());

app.MapHub<ChatsHub>("/hubs/chats"); 
app.MapHub<FriendsHub>("/hubs/friends");
app.MapHub<ProfileHub>("/hubs/profiles");
app.MapHub<PresenceHub>("/hubs/presence");

app.MapSwagger()
    .RequireAuthorization();

app.Map("/", () => "Not for browsers");

app.Run();