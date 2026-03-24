using ExamenII_Web.api.DTOs;

namespace ExamenII_Web.api.Service;

public interface IParticipacionService
{
    Task InscribirseTorneo(string torneoId, string jugadorId, bool pagado);

    Task<List<ParticipanteDTO>> ObtenerParticipantes(string torneoId, int pagina, int limite);

    Task ActualizarResultado(string torneoId, string participacionId, ResultadoPartidaDTO dto);

    Task<List<MisTorneosDTO>> ObtenerMisTorneos(string jugadorId);

    Task AbandonarTorneo(string torneoId, string participacionId, string jugadorId);
}
