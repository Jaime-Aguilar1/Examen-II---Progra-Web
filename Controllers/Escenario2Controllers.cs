using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Models;
using ExamenII_Web.api.Services;
using System;
using System.Threading.Tasks;
using ExamenII_Web.api.Service;

namespace ExamenII_Web.api.Controllers
{
   [ApiController]
    [Route("api/[controller]")]
    public class JuegosController : ControllerBase
    {
        private readonly IJuegoService _juegoService;

        public JuegosController(IJuegoService juegoService)
        {
            _juegoService = juegoService;
        }

        // POST: Agregar juego
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AgregarJuego([FromBody] JuegoDto juegoDto)
        {
            try
            {
                var juego = new Juego
                {
                    Titulo = juegoDto.Titulo,
                    Desarrollador = juegoDto.Desarrollador,
                    Genero = juegoDto.Genero,
                    Plataformas = juegoDto.Plataformas,
                    FechaLanzamiento = juegoDto.FechaLanzamiento,
                    Descripcion = juegoDto.Descripcion,
                };

                var result = await _juegoService.AgregarJuego(juego);

                return CreatedAtAction(nameof(ObtenerEstadisticas), new { id = result.Id }, new
                {
                    codigo = 201,
                    mensaje = "Juego creado correctamente",
                    datos = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { codigo = 409, mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { codigo = 400, mensaje = ex.Message });
            }
        }
        
        // GET: Listar juegos filtrados
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListarJuegos(string genero, string plataforma, string desarrollador)
        {
            try
            {
                var juegos = await _juegoService.ListarJuegos(genero, plataforma, desarrollador);

                return Ok(new
                {
                    codigo = 200,
                    mensaje = "Juegos obtenidos correctamente",
                    datos = juegos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = 500, mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: Todos los juegos (nuevo)
        [HttpGet("todos")]
        [Authorize]
        public async Task<IActionResult> ObtenerTodosJuegos()
        {
            try
            {
                // Debes agregar este método en el servicio IJuegoService
                var juegos = await _juegoService.ObtenerTodosJuegos();

                return Ok(new
                {
                    codigo = 200,
                    mensaje = "Todos los juegos obtenidos correctamente",
                    datos = juegos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = 500, mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // PUT: Actualizar juego
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActualizarJuego(string id, [FromBody] JuegoDto juegoDto)
        {
            try
            {
                var juegoExistente = await _juegoService.ObtenerJuegoPorId(id);
                if (juegoExistente == null)
                    return NotFound(new { codigo = 404, mensaje = "Juego no encontrado" });

                juegoExistente.Descripcion = juegoDto.Descripcion;
                juegoExistente.Genero = juegoDto.Genero;
                juegoExistente.Plataformas = juegoDto.Plataformas;
                juegoExistente.Titulo = juegoDto.Titulo;
                juegoExistente.Desarrollador = juegoDto.Desarrollador;
                juegoExistente.FechaLanzamiento = juegoDto.FechaLanzamiento;

                var result = await _juegoService.ActualizarJuego(id, juegoExistente);

                return Ok(new
                {
                    codigo = 200,
                    mensaje = "Juego actualizado correctamente",
                    datos = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { codigo = 404, mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { codigo = 409, mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { codigo = 400, mensaje = ex.Message });
            }
        }
        
        // DELETE: Eliminar juego
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EliminarJuego(string id)
        {
            try
            {
                await _juegoService.EliminarJuego(id);

                return Ok(new
                {
                    codigo = 200,
                    mensaje = "Juego eliminado correctamente"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { codigo = 404, mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { codigo = 409, mensaje = ex.Message });
            }
        }
        
        // GET estadísticas por ID
        [HttpGet("{id}/estadisticas")]
        [Authorize]
        public async Task<IActionResult> ObtenerEstadisticas(string id)
        {
            try
            {
                var juego = await _juegoService.ObtenerEstadisticas(id);

                return Ok(new
                {
                    codigo = 200,
                    mensaje = "Estadísticas obtenidas correctamente",
                    datos = juego
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { codigo = 404, mensaje = ex.Message });
            }
        }
    }
}

