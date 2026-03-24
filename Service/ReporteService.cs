using Google.Cloud.Firestore;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Models;

namespace ExamenII_Web.api.Service
{
    public class ReporteService : IReporteService
    {
        private readonly FirestoreDb _db;

        public ReporteService(FirebaseService firebase)
        {
            _db = firebase.GetDb();
        }

        // Obtener ranking global de un juego
        public async Task<List<ClasificacionDto>> ObtenerClasificaciones(string juegoId, int pagina = 1,
            int? nivelMin = null, int? nivelMax = null)
        {
            var clasificacionesRef = _db.Collection("clasificaciones");
            var query = clasificacionesRef.WhereEqualTo("juegoId", juegoId);

            var snapshot = await query.GetSnapshotAsync();
            var clasificaciones = snapshot.Documents
                .Select(doc => doc.ConvertTo<Clasificacion>())
                .OrderBy(c => c.Posicion)
                .ToList();

            if (nivelMin.HasValue)
                clasificaciones = clasificaciones.Where(c => c.NivelJuego >= nivelMin.Value).ToList();
            if (nivelMax.HasValue)
                clasificaciones = clasificaciones.Where(c => c.NivelJuego <= nivelMax.Value).ToList();

            int skip = (pagina - 1) * 50;
            clasificaciones = clasificaciones.Skip(skip).Take(50).ToList();

            var jugadorIds = clasificaciones.Select(c => c.JugadorId).Distinct().ToList();
            var jugadores = await ObtenerJugadoresPorIds(jugadorIds);

            return clasificaciones.Select(c =>
            {
                var jugador = jugadores.FirstOrDefault(j => j.Id == c.JugadorId);
                return new ClasificacionDto
                {
                    Posicion = c.Posicion,
                    NombreJugador = jugador?.Nombre ?? "Desconocido",
                    Puntos = c.PuntosJuego,
                    Nivel = c.NivelJuego,
                    RatioVictoria = c.TotalPartidas > 0 ? (double)c.RatioVictoria / 100 : 0,
                    TotalPartidas = c.TotalPartidas,
                    RachaActual = c.Racha
                };
            }).ToList();
        }

        // Obtener ranking de un jugador en un juego
        public async Task<ClasificacionJugadorDto> ObtenerClasificacionJugador(string juegoId, string jugadorId)
        {
            var query = _db.Collection("clasificaciones")
                .WhereEqualTo("juegoId", juegoId)
                .WhereEqualTo("jugadorId", jugadorId);

            var snapshot = await query.GetSnapshotAsync();
            var clasificacion = snapshot.Documents.Select(doc => doc.ConvertTo<Clasificacion>()).FirstOrDefault();
            if (clasificacion == null) return null;

            return new ClasificacionJugadorDto
            {
                Posicion = clasificacion.Posicion,
                Puntos = clasificacion.PuntosJuego,
                Nivel = clasificacion.NivelJuego,
                Medallas = new List<string>
                {
                    $"Oro:{clasificacion.MedallasOro}",
                    $"Plata:{clasificacion.MedallaPlata}",
                    $"Bronce:{clasificacion.MedallaBronce}"
                },
                Logros = clasificacion.Logros ?? new List<string>()
            };
        }

        // Top 10 torneos más inscritos últimos 30 días
        public async Task<List<TorneoPopularDto>> ObtenerTorneosPopulares()
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-30);
            var snapshot = await _db.Collection("torneos")
                .WhereGreaterThan("fechaCreacion", fechaLimite)
                .GetSnapshotAsync();

            var torneos = snapshot.Documents.Select(doc => doc.ConvertTo<Torneo>())
                .OrderByDescending(t => t.ParticipantesActuales)
                .Take(10)
                .ToList();

            var juegoIds = torneos.Select(t => t.JuegoId).Distinct().ToList();
            var juegos = await ObtenerJuegosPorIds(juegoIds);

            return torneos.Select(t =>
            {
                var juego = juegos.FirstOrDefault(j => j.Id == t.JuegoId);
                return new TorneoPopularDto
                {
                    Nombre = t.Nombre,
                    Juego = juego?.Titulo ?? "Desconocido",
                    CantidadInscripciones = t.ParticipantesActuales,
                    PremioTotal = t.PremioTotal,
                    Estado = t.Estado
                };
            }).ToList();
        }

        // Top 20 jugadores por puntos globales
        public async Task<List<JugadorDestacadoDto>> ObtenerJugadoresDestacados()
        {
            var snapshot = await _db.Collection("jugadores").GetSnapshotAsync();
            var jugadores = snapshot.Documents
                .Select(doc => doc.ConvertTo<Jugador>())
                .OrderByDescending(j => j.PuntosGlobales)
                .Take(20)
                .ToList();

            var jugadoresDestacados = new List<JugadorDestacadoDto>();
            foreach (var j in jugadores)
            {
                var participacionesSnapshot = await _db.Collection("participaciones")
                    .WhereEqualTo("jugadorId", j.Id)
                    .GetSnapshotAsync();

                int cantidadJuegos = participacionesSnapshot.Documents
                    .Select(p => p.GetValue<string>("torneoId"))
                    .Distinct()
                    .Count();

                jugadoresDestacados.Add(new JugadorDestacadoDto
                {
                    Nombre = j.Nombre,
                    PuntosGlobales = j.PuntosGlobales,
                    TorneosGanados = j.TorneosGanados,
                    CantidadJuegos = cantidadJuegos
                });
            }

            return jugadoresDestacados;
        }

        // Desempeño del jugador en un juego específico
        public async Task<DesempenoDto> ObtenerMiDesempeno(string juegoId, string jugadorId)
        {
            var clasificacion = await ObtenerClasificacionJugador(juegoId, jugadorId);
            if (clasificacion == null) return null;

            var participacionesSnapshot = await _db.Collection("participaciones")
                .WhereEqualTo("jugadorId", jugadorId)
                .GetSnapshotAsync();

            var mejoresTorneos = participacionesSnapshot.Documents
                .Select(p => new TorneoTopDto
                {
                    NombreTorneo = p.GetValue<string>("torneoId"),
                    Puntuacion = p.GetValue<int>("puntosObtenidos")
                })
                .OrderByDescending(t => t.Puntuacion)
                .Take(3)
                .ToList();

            int puntosParaSiguiente = clasificacion.Nivel * 1000;
            double progreso = clasificacion.Puntos > 0 ? (double)clasificacion.Puntos / puntosParaSiguiente * 100 : 0;

            return new DesempenoDto
            {
                NivelActual = clasificacion.Nivel,
                PosicionRanking = clasificacion.Posicion,
                ProgresoSiguienteNivel = progreso,
                RatioVictoria = clasificacion.RatioVictoria,
                RachaActual = clasificacion.RachaActual,
                MedallasObtenidas = clasificacion.Medallas,
                MejoresTorneos = mejoresTorneos
            };
        }

        // Tendencias de la plataforma
        public async Task<TendenciasDto> ObtenerTendencias()
        {
            var snapshotJuegos = await _db.Collection("juegos").GetSnapshotAsync();
            var juegos = snapshotJuegos.Documents
                .Select(doc => doc.ConvertTo<Juego>())
                .OrderByDescending(j => j.JugadoresActivos)
                .Take(5)
                .ToList();

            var juegosPopulares = juegos.Select(j => new JuegoPopularDto
            {
                Nombre = j.Titulo,
                Popularidad = j.JugadoresActivos
            }).ToList();

            var torneosSnapshot = await _db.Collection("torneos")
                .WhereIn("estado", new List<string> { "próximo", "en progreso" })
                .GetSnapshotAsync();

            var generos = new Dictionary<string, int>();
            foreach (var torneoDoc in torneosSnapshot.Documents)
            {
                var torneo = torneoDoc.ConvertTo<Torneo>();
                var juego = await ObtenerJuegoPorId(torneo.JuegoId);
                if (juego != null)
                {
                    if (generos.ContainsKey(juego.Genero))
                        generos[juego.Genero]++;
                    else
                        generos[juego.Genero] = 1;
                }
            }

            return new TendenciasDto
            {
                JuegosMasPopulares = juegosPopulares,
                GenerosConMasTorneos = generos,
                HoraPicoActividad = "20:00"
            };
        }

        // Auxiliares
        private async Task<List<Jugador>> ObtenerJugadoresPorIds(List<string> ids)
        {
            var tasks = ids.Select(id => ObtenerJugadorPorId(id));
            var jugadores = await Task.WhenAll(tasks);
            return jugadores.Where(j => j != null).ToList();
        }

        private async Task<Jugador> ObtenerJugadorPorId(string id)
        {
            var snapshot = await _db.Collection("jugadores").Document(id).GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Jugador>() : null;
        }

        private async Task<List<Juego>> ObtenerJuegosPorIds(List<string> ids)
        {
            var tasks = ids.Select(id => ObtenerJuegoPorId(id));
            var juegos = await Task.WhenAll(tasks);
            return juegos.Where(j => j != null).ToList();
        }

        private async Task<Juego> ObtenerJuegoPorId(string id)
        {
            var snapshot = await _db.Collection("juegos").Document(id).GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Juego>() : null;
        }
    }
}