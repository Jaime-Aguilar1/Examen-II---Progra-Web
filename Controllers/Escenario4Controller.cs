using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Service;

namespace ExamenII_Web.api.Controllers
{
    [ApiController]
    [Route("api")]
    public class Escenario4Controller : ControllerBase
    {
        private readonly IParticipacionService _service;

        public Escenario4Controller(IParticipacionService service)
        {
            _service = service;
        }

        // POST /api/torneos/{id}/inscribirse
        [Authorize]
        [HttpPost("torneos/{id}/inscribirse")]
        public async Task<IActionResult> Inscribirse(string id, [FromBody] InscripcionDTO dto)
        {
            string jugadorId = User.FindFirstValue("jugadorId");

            await _service.InscribirseTorneo(id, jugadorId, dto.pagado);

            return Ok(new { mensaje = "Inscripcion exitosa" });
        }

        // GET /api/torneos/{id}/participantes
        [Authorize]
        [HttpGet("torneos/{id}/participantes")]
        public async Task<IActionResult> Participantes(string id, int pagina = 1, int limite = 10)
        {
            var lista = await _service.ObtenerParticipantes(id, pagina, limite);

            return Ok(lista);
        }

        // PUT /api/torneos/{id}/participantes/{idParticipacion}/actualizar-resultado
        [Authorize(Roles = "admin,organizador")]
        [HttpPut("torneos/{id}/participantes/{idParticipacion}/actualizar-resultado")]
        public async Task<IActionResult> ActualizarResultado(
            string id,
            string idParticipacion,
            [FromBody] ResultadoPartidaDTO dto)
        {
            await _service.ActualizarResultado(id, idParticipacion, dto);

            return Ok(new { mensaje = "Resultado actualizado" });
        }

        // GET /api/jugador/mis-torneos
        [Authorize]
        [HttpGet("jugador/mis-torneos")]
        public async Task<IActionResult> MisTorneos()
        {
            string jugadorId = User.FindFirstValue("jugadorId");

            var data = await _service.ObtenerMisTorneos(jugadorId);

            return Ok(data);
        }

        // DELETE /api/torneos/{id}/participantes/{idParticipacion}/abandonar
        [Authorize]
        [HttpDelete("torneos/{id}/participantes/{idParticipacion}/abandonar")]
        public async Task<IActionResult> Abandonar(string id, string idParticipacion)
        {
            string jugadorId = User.FindFirstValue("jugadorId");

            await _service.AbandonarTorneo(id, idParticipacion, jugadorId);

            return Ok(new { mensaje = "Torneo abandonado correctamente" });
        }
    }
}