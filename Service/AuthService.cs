
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Models;
using Google.Cloud.Firestore;
using Microsoft.IdentityModel.Tokens;

namespace ExamenII_Web.api.Service;

public class AuthService : IAuthService
{
   private readonly IConfiguration _configuration;
    private readonly FirebaseService _firebaseService;
    private readonly CollectionReference _usuariosCollection;
    private IAuthService _authServiceImplementation;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
        _firebaseService = firebaseService;
        _configuration = configuration;
        _usuariosCollection = _firebaseService.GetCollection("Jugadores"); // Asegúrate de que el nombre coincida con tu BD
    }
    
    

    public async Task<Jugador> Register(RegisterDto registerDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Pass))
            {
                throw new ArgumentException("Email y password son requeridos");
            }

            // 1. Verificar si el correo ya existe
            var query = await _usuariosCollection
                .WhereEqualTo("Correo", registerDto.Email)
                .GetSnapshotAsync();

            if (query.Count > 0)
            {
                throw new InvalidOperationException("El email ya está registrado");
            }

            // 2. Crear el objeto Usuario 
            var nuevoUsuario = new Jugador
            {
                Id = Guid.NewGuid().ToString(),
                UserName =  registerDto.UserName,
                UserLastName =  registerDto.UserLastName,
                BirthDate =  Timestamp.FromDateTime(registerDto.BirthDate.ToUniversalTime()),
                Email = registerDto.Email,
                Pass = HashPassword(registerDto.Pass), // Encriptamos la contraseña
                Contry = registerDto.Contry,
                NickName = registerDto.NickName,
                Role = "jugador", 
                IsOnline =  true,
                IsActive =  true,
                GlobalPoints = 0,
                TournamentWon = 0,
                LastConect = Timestamp.FromDateTime(DateTime.UtcNow),
                RegistrationDate = Timestamp.FromDateTime(DateTime.UtcNow),
            };

            // 3. Guardar en Firestore
            await _usuariosCollection.Document(nuevoUsuario.Id).SetAsync(nuevoUsuario);

            return nuevoUsuario;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al registrar usuario: {e.Message}");
            throw;
        }
    }

    public async Task<(JugadorDto user, string token)> Login(LoginDto loginDto)
    {
        try
        {
            // 1. Buscar usuario por Correo
            var query = await _usuariosCollection
                .WhereEqualTo("Email", loginDto.Email)
                .GetSnapshotAsync();

            if (query.Count == 0)
            {
                throw new InvalidOperationException("Credenciales incorrectas");
            }

            var userDoc = query.Documents[0];
            var usuario = userDoc.ConvertTo<Jugador>();

            // 2. Validar contraseña encriptada
            if (usuario.Pass!= HashPassword(loginDto.Pass))
            {
                throw new InvalidOperationException("Credenciales incorrectas");
            }

            // 3. Generar el token
            var token = GenerateJwtToken(usuario);

            // 4. Mapear a UserDto
            var userDto = new JugadorDto()
            {
                Id = usuario.Id,
                // Union Nombre y Apellido
                FullName = $"{usuario.UserName} {usuario.UserLastName}",
                NickName = usuario.NickName,
                Email = usuario.Email,
                GlobalPoints = usuario.GlobalPoints,
                TournamentWon = usuario.TournamentWon,
                IsOnline = true,
                Role = usuario.Role,
                // TRANSFORMACIÓN 2: Calcular edad en tiempo real desde la BirthDate
                Age = CalculateAge(usuario.BirthDate.ToDateTime())
            };

            return (userDto, token);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en login: {e.Message}");
            throw;
        }
    }
    
    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
    

    public async Task<bool> ValidateToken(string token)
    {
        // ... (Tu código de ValidateToken se mantiene igual)
        try
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey)) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<JugadorDto?> GetUserById(string userId)
    {
        var docRef = _usuariosCollection.Document(userId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        var user = snapshot.ConvertTo<Jugador>();

        // 1. Calculamos la edad desde BirthDate
        int calculatedAge = 0;
        if (user.BirthDate != null)
        {
            DateTime birthDate = user.BirthDate.ToDateTime();
            calculatedAge = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-calculatedAge)) calculatedAge--;
        }

        // 2. Mapeamos al DTO con los campos transformados
        return new JugadorDto()
        {
            Id = user.Id,
            Email = user.Email,
            // Unimos nombre y apellido
            FullName = $"{user.UserName} {user.UserLastName}",
            // Enviamos la edad calculada
            Age = calculatedAge,
            NickName = user.NickName,
            TournamentWon = user.TournamentWon,
            GlobalPoints = user.GlobalPoints,
            IsOnline = user.IsOnline,
            Role = user.Role,
            
        };
    }

    public string GenerateJwtToken(Jugador user)
    {
        // ... (Tu código de GenerateJwtToken se mantiene igual)
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey no configurado");

        var key = Encoding.ASCII.GetBytes(secretKey);
        string fullName = $"{user.UserName} {user.UserLastName}";

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id),
                new Claim("email", user.Email),
                new Claim("name", fullName),
                new Claim("role", user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // --- MÉTODO PRIVADO PARA ENCRIPTAR CONTRASEÑAS ---
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
    
}

