using ExamenII_Web.api.DTOs;

namespace ExamenII_Web.api.Service
{
    public interface IReporteService
    {
        Task<List<ClasificacionDto>> ObtenerClasificaciones(string juegoId, int pagina = 1, int? nivelMin = null, int? nivelMax = null);
        Task<ClasificacionJugadorDto> ObtenerClasificacionJugador(string juegoId, string jugadorId);
        Task<List<TorneoPopularDto>> ObtenerTorneosPopulares();
        Task<List<JugadorDestacadoDto>> ObtenerJugadoresDestacados();
        Task<DesempenoDto> ObtenerMiDesempeno(string juegoId, string jugadorId);
        Task<TendenciasDto> ObtenerTendencias();
    }
}
