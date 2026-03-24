using ExamenII_Web.api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamenII_Web.api.Services
{
    public interface IJuegoService
    {
        Task<Juego> AgregarJuego(Juego juego);
        Task<List<Juego>> ListarJuegos(string genero = null, string plataforma = null, string desarrollador = null);
        Task<Juego> ActualizarJuego(string id, Juego juegoActualizado);
        Task<Juego> ObtenerEstadisticas(string id);
        Task<Juego> ObtenerJuegoPorId(string id);
        Task EliminarJuego(string id);
        Task<List<Juego>> ObtenerTodosJuegos();
        
    }
}