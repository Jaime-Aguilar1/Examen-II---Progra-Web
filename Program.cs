using System.Text;
using ExamenII_Web.api.Service;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
//using Proyecto_Grupo4.API.Services; // Namespace actualizado
//using Proyecto; // Para reconocer tu FirebaseService

var builder = WebApplication.CreateBuilder(args);


// ============================================
// CONFIGURACIÓN DE FIRESTORE 
// ============================================
// 1. Configurar la variable de entorno con tus credenciales
string path = Path.Combine(
    AppContext.BaseDirectory, 
    "Config", 
    "examenii-prograweb-firebase"
); 
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

// 2. Registrar FirestoreDb para que ReservationService lo pueda usar
builder.Services.AddSingleton<FirestoreDb>(sp => 
{
    return FirestoreDb.Create("examenii-prograweb"); 
});


// ============================================
// REGISTRAR SERVICIOS (Inyección de Dependencias)
// ============================================
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IRoomService, RoomService>();
//builder.Services.AddScoped<IReservationService, ReservationService>();

// ============================================
// CONFIGURAR CORS (Conexión con el Frontend)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL de tu app de Angular
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// CONFIGURAR AUTENTICACIÓN JWT
// ============================================
var jwtKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("JWT SecretKey no configurada en appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================
// CONFIGURAR SWAGGER CON BOTÓN DE AUTORIZACIÓN
// ============================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PLATAFORMA DE JUEGOS Y TORNEOS API", Version = "v1" });
    
    // Configuración para poder pegar el Token en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando Bearer scheme. \r\n\r\n Introduce 'Bearer' [espacio] y después tu token.\r\n\r\nEjemplo: \"Bearer 12345abcdef\"",
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

var app = builder.Build();

// ============================================
// CONFIGURACIÓN DEL PIPELINE (Middleware)
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// EL ORDEN ES VITAL: CORS -> Autenticación -> Autorización
app.UseCors("AllowAngular");
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();