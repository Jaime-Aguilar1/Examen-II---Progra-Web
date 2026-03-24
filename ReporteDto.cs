
namespace ExamenII_Web.api.DTOs
{
    public class ClasificacionDto
    {
        public int Posicion { get; set; }
        public string NombreJugador { get; set; }
        public int Puntos { get; set; }
        public int Nivel { get; set; }
        public double RatioVictoria { get; set; }
        public int TotalPartidas { get; set; }
        public int RachaActual { get; set; }
    }

    public class ClasificacionJugadorDto
    {
        public int Posicion { get; set; }
        public int Puntos { get; set; }
        public int Nivel { get; set; }
        public List<string> Medallas { get; set; }
        public List<string> Logros { get; set; }
    }

    public class TorneoPopularDto
    {
        public string Nombre { get; set; }
        public string Juego { get; set; } // Nombre del juego
        public int CantidadInscripciones { get; set; }
        public double PremioTotal { get; set; }
        public string Estado { get; set; }
    }

    public class JugadorDestacadoDto
    {
        public string Nombre { get; set; }
        public int PuntosGlobales { get; set; }
        public int TorneosGanados { get; set; }
        public int CantidadJuegos { get; set; }
    }

    public class DesempenoDto
    {
        public int NivelActual { get; set; }
        public int PosicionRanking { get; set; }
        public double ProgresoSiguienteNivel { get; set; } // Porcentaje
        public double RatioVictoria { get; set; }
        public int RachaActual { get; set; }
        public List<string> MedallasObtenidas { get; set; }
        public List<TorneoTopDto> MejoresTorneos { get; set; }
    }

    public class TorneoTopDto
    {
        public string NombreTorneo { get; set; }
        public int Puntuacion { get; set; }
    }

    public class TendenciasDto
    {
        public List<JuegoPopularDto> JuegosMasPopulares { get; set; }
        public Dictionary<string, int> GenerosConMasTorneos { get; set; }
        public string HoraPicoActividad { get; set; }
    }

    public class JuegoPopularDto
    {
        public string Nombre { get; set; }
        public int Popularidad { get; set; } // e.g., número de jugadores activos
    }
}
