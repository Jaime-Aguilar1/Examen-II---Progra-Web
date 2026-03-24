using ExamenII_Web.api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExamenII_Web.api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReporteController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        // GET: api/clasificaciones/{juegoId}?pagina=1&nivelMin=1&nivelMax=10
        [HttpGet("clasificaciones/{juegoId}")]
        public async Task<IActionResult> ObtenerClasificaciones(string juegoId, [FromQuery] int pagina = 1, [FromQuery] int? nivelMin = null, [FromQuery] int? nivelMax = null)
        {
            var clasificaciones = await _reporteService.ObtenerClasificaciones(juegoId, pagina, nivelMin, nivelMax);
            return Ok(clasificaciones);
        }

        // GET: api/jugador/clasificacion/{juegoId}
        [HttpGet("jugador/clasificacion/{juegoId}")]
        [Authorize] // Solo para el jugador autenticado
        public async Task<IActionResult> ObtenerClasificacionJugador(string juegoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var clasificacion = await _reporteService.ObtenerClasificacionJugador(juegoId, userId);
            if (clasificacion == null)
                return NotFound("Clasificación no encontrada");

            return Ok(clasificacion);
        }

        // GET: api/reportes/torneos-populares
        [HttpGet("reportes/torneos-populares")]
        [Authorize(Roles = "Organizador,Administrador")] // Solo organizadores y admin
        public async Task<IActionResult> ObtenerTorneosPopulares()
        {
            var torneos = await _reporteService.ObtenerTorneosPopulares();
            return Ok(torneos);
        }

        // GET: api/reportes/jugadores-destacados
        [HttpGet("reportes/jugadores-destacados")]
        [Authorize] // Usuarios autenticados
        public async Task<IActionResult> ObtenerJugadoresDestacados()
        {
            var jugadores = await _reporteService.ObtenerJugadoresDestacados();
            return Ok(jugadores);
        }

        // GET: api/reportes/mi-desempeno/{juegoId}
        [HttpGet("reportes/mi-desempeno/{juegoId}")]
        [Authorize] // Solo para el jugador autenticado
        public async Task<IActionResult> ObtenerMiDesempeno(string juegoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var desempeno = await _reporteService.ObtenerMiDesempeno(juegoId, userId);
            if (desempeno == null)
                return NotFound("Desempeño no encontrado");

            return Ok(desempeno);
        }

        // GET: api/reportes/tendencias
        [HttpGet("reportes/tendencias")]
        [Authorize(Roles = "Administrador")] // Solo admin
        public async Task<IActionResult> ObtenerTendencias()
        {
            var tendencias = await _reporteService.ObtenerTendencias();
            return Ok(tendencias);
        }
    }
}
