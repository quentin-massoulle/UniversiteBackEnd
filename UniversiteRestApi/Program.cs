using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.JeuxDeDonnees;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entities;
using UniversiteEFDataProvider.RepositoryFactories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

////////// Swagger avec prise en charge token jwt
// Modification de la ligne uilder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Projet Universite", Version = "v1" });

// Add Bearer Token Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
/////////////////// Fin config Swagger

////////////// Config système de log en console
builder.Services.AddLogging(options =>
{
    options.ClearProviders();
    options.AddConsole();
});
////////////////////Fin Log

///////////// Configuration des connexions à MySql
String connectionString = builder.Configuration.GetConnectionString("MySqlConnection") ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");

// Création du contexte de la base de données en utilisant la connexion MySql que l'on vient de définir
// Ce contexte est rajouté dans les services de l'application, toujours prêt à être utilisé par injection de dépendances
builder.Services.AddDbContext<UniversiteDbContext>(options =>options.UseMySQL(connectionString));
// La factory est rajoutée dans les services de l'application, toujours prête à être utilisée par injection de dépendances
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
//////////////// Fin connexion BD

////////// Sécurisation
builder.Services.AddIdentityCore<UniversiteUser>(options=>
    { 
        // A modifier en prod pour renforcer la sécurité!!! 
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddRoles<UniversiteRole>()
    .AddEntityFrameworkStores<UniversiteDbContext>()
    .AddApiEndpoints()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Création de tous les services qui sont stockés dans app
// app contient tous les objets de notre application
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configuration du serveur Web
app.UseHttpsRedirection();
app.MapControllers();

// Sécurisation
app.UseAuthentication();
app.UseAuthorization();

// Initialisation de la base de données
ILogger logger = app.Services.GetRequiredService<ILogger<BdBuilder>>();
logger.LogInformation("Chargement des données de test");
using(var scope = app.Services.CreateScope())
{
    UniversiteDbContext context = scope.ServiceProvider.GetRequiredService<UniversiteDbContext>();
    IRepositoryFactory repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory>();
    UserManager<UniversiteUser> userManager=scope.ServiceProvider.GetService<UserManager<UniversiteUser>>();
    RoleManager<UniversiteRole> roleManager =scope.ServiceProvider.GetService<RoleManager<UniversiteRole>>();
    
    //BdBuilder seedBD = new TestBdBuilder(repositoryFactory);
    BdBuilder seedBD = new BasicBdBuilder(repositoryFactory);
    await seedBD.BuildUniversiteBdAsync();
}

// Exécution de l'application
app.Run();

