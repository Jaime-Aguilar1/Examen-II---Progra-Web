

using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Models;

namespace ExamenII_Web.api.Service;

public interface IAuthService
{
    /**
    * Register:
    * 1. Verificar que el correo no esté duplicado en Firestore
    * 2. Crear el objeto Usuario con los datos del hotel
    * 3. Guardar el documento en la colección "Usuarios"
    * * Retorna: El usuario creado (modelo Usuario)
    */
    Task<Jugador> Register(RegisterDto registerDto);

    /**
     * Login:
     * 1. Validar que el email existe en Firestore
     * 2. Verificar la contraseña (campo 'Costrasena')
     * 3. Si son correctas, generar el JWT
     * * Retorna: Una tupla con los datos del usuario mapeados al DTO y el token generado
     */
    Task<(JugadorDto user, string token)> Login(LoginDto loginDto);

    /**
     * Obtiene los datos de un usuario específico por su ID de documento
     */
    Task<JugadorDto?> GetUserById(string userId);

    /**
     * Valida si un token JWT recibido es legítimo
     */
    Task<bool> ValidateToken(string token);

    /**
     * Genera la cadena del token JWT basada en los datos del usuario y su rol (huésped/gerente)
     */
    string GenerateJwtToken(Jugador user);
}