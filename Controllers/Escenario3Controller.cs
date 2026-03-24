using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Service;
using ExamenII_Web.api.Services; // Usando Service en singular según la estructura de tu proyecto

namespace ExamenII_Web.api.Controllers
{
    // Fijamos la ruta explícitamente para cumplir con la rúbrica del profesor
    [Route("api/torneos")]
    [ApiController]
    public class Escenario3Controller : ControllerBase
    {
        private readonly ITorneoService _torneoService;

        public Escenario3Controller(ITorneoService torneoService)
        {
            _torneoService = torneoService;
        }

        [HttpPost]
        [Authorize(Roles = "organizador,admin")]
        public async Task<IActionResult> CrearTorneo([FromBody] TorneoDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var torneoId = await _torneoService.CrearTorneoAsync(dto, userId);
                return Ok(new { Message = "Torneo creado exitosamente", Id = torneoId });
            }
            catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTorneos(
            [FromQuery] string juego = null, [FromQuery] string estado = null,
            [FromQuery] double? minPrecio = null, [FromQuery] double? maxPrecio = null,
            [FromQuery] int? minNivelReq = null, [FromQuery] int? maxNivelReq = null,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var torneos = await _torneoService.ObtenerTorneosAsync(juego, estado, minPrecio, maxPrecio, minNivelReq, maxNivelReq, page, pageSize);
                return Ok(torneos);
            }
            catch (Exception ex) { return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "organizador,admin")]
        public async Task<IActionResult> ActualizarTorneo(string id, [FromBody] TorneoUpdateDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("admin");
                
                await _torneoService.ActualizarTorneoAsync(id, dto, userId, isAdmin);
                return Ok(new { Message = "Torneo actualizado correctamente." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { Error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "organizador,admin")]
        public async Task<IActionResult> CancelarTorneo(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("admin");

                await _torneoService.CancelarTorneoAsync(id, userId, isAdmin);
                return Ok(new { Message = "El torneo ha sido cancelado." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { Error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message }); }
        }

        [HttpPatch("{id}/cambiar-estado")]
        [Authorize(Roles = "organizador,admin")]
        public async Task<IActionResult> CambiarEstado(string id, [FromBody] string nuevoEstado)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("admin");

                await _torneoService.CambiarEstadoAsync(id, nuevoEstado, userId, isAdmin);
                return Ok(new { Message = "Estado actualizado correctamente." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { Error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message }); }
        }
    }
}