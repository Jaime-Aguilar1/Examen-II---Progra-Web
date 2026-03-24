using System.Collections.Generic;
using System.Threading.Tasks;
using ExamenII_Web.api.Models;
using ExamenII_Web.api.DTOs;

namespace ExamenII_Web.api.Services
{
    public interface ITorneoService
    {
        Task<string> CrearTorneoAsync(TorneoDTO dto, string organizadorId);
        Task<List<Torneo>> ObtenerTorneosAsync(string juego, string estado, double? minPrecio, double? maxPrecio, int? minNivelReq, int? maxNivelReq, int page, int pageSize);
        Task<bool> ActualizarTorneoAsync(string id, TorneoUpdateDto dto, string userId, bool isAdmin);
        Task<bool> CancelarTorneoAsync(string id, string userId, bool isAdmin);
        Task<bool> CambiarEstadoAsync(string id, string nuevoEstado, string userId, bool isAdmin);
    }
}