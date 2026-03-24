using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ExamenII_Web.api.Controllers;
/// AuthController maneja la autenticación del sistema del Hotel
/// Endpoints para registro de huéspedes e inicio de sesión
[ApiController]
[Route("api/[controller]")]
public class Escenario1Controller: ControllerBase
{
        private readonly IAuthService _authService;
        private readonly ILogger<Escenario1Controller> _logger;


        /// Constructor: Recibe IAuthService inyectado desde Program.cs
   
        public Escenario1Controller(IAuthService authService, ILogger<Escenario1Controller> logger)
        {
            _authService = authService;
            _logger = logger;
        }

   
        /// POST /api/auth/register
        /// Registra un nuevo huésped en el sistema

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto? registerDto)
        {
            try
            {
                if (registerDto == null)
                {
                    return BadRequest(new { message = "El cuerpo de la petición es requerido" });
                }

                if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Pass))
                {
                    return BadRequest(new { message = "Email y contraseña son requeridos" });
                }

                // El servicio ya devuelve el objeto Usuario mapeado correctamente
                var usuarioCreado = await _authService.Register(registerDto);

                _logger.LogInformation($"Nuevo usuario registrado en el hotel: {usuarioCreado.Email}");

                // Devolvemos 201 Created con el usuario
                return Created($"/api/auth/users/{usuarioCreado.Id}", usuarioCreado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en registro de hotel: {ex.Message}");
                // Agregamos "detail = ex.Message" para que Swagger nos diga qué falló exactamente
                return StatusCode(500, new { 
                    message = "Error interno al registrar el usuario", 
                    detail = ex.Message,
                    inner = ex.InnerException?.Message 
                });
            }
        }


        /// POST /api/auth/login
        /// Inicia sesión y devuelve un token JWT
 
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto? loginDto)
        {
            try
            {
                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email))
                {
                    return BadRequest(new { message = "Credenciales incompletas" });
                }

                // El servicio devuelve el UserDto (ya filtrado) y el token generado
                var (userDto, token) = await _authService.Login(loginDto);

                _logger.LogInformation($"Sesión iniciada: {userDto.Email}");

                var response = new AuthResponseDto
                {
                    Success = true,
                    Message = "Bienvenido la Plataforma de Juego y Torneos",
                    Token = token,
                    Jugador = userDto
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en login: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error al procesar el ingreso" });
            }
        }
        
       /// GET /api/auth/users/{userId}
       /// Obtiene el perfil de un usuario (Estudiante/Bibliotecario) por ID
       [HttpGet("users/{userId}")]
       public async Task<IActionResult> GetUser(string userId)
       {
           try
           {
               if (string.IsNullOrWhiteSpace(userId))
               {
                   return BadRequest(new { message = "El ID del usuario es requerido" });
               }

               // El servicio ya nos devuelve el UserDto procesado (con FullName y Age)
               var userDto = await _authService.GetUserById(userId);

               if (userDto == null)
               {
                   // Cambiamos "Huésped" por "Usuario" para el contexto de la biblioteca
                   return NotFound(new { message = "Usuario no encontrado" });
               }

               // Retornamos directamente el DTO que ya viene limpio del servicio
               return Ok(userDto);
           }
           catch (Exception ex)
           {
               // Usamos el logger que ya tienes configurado
               _logger.LogError($"Error al buscar usuario en biblioteca: {ex.Message}");
               return StatusCode(500, new { message = "Error interno al obtener los datos del perfil" });
           }
       }
       
       [HttpPut("jugadores/{id}/perfil")]
       [Authorize]
       public async Task<IActionResult> UpdatePerfil(string id, [FromBody] ActualizarPerfilDto dto)
       {
           
           var userIdFromToken = User.FindFirst("jugadorId")?.Value; 
           var userRole = User.FindFirst("rol")?.Value;

           
           if (userIdFromToken != id && userRole != "admin")
           {
               return Forbid();
           }

           
           var resultado = await _authService.ActualizarPerfil(id, dto);

           if (!resultado)
           {
               return NotFound(new { message = "Jugador no encontrado" }); 
           }

           return Ok(new { message = "Perfil actualizado exitosamente" });
       }
    }
