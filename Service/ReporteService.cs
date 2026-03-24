using Google.Cloud.Firestore;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Models;

namespace ExamenII_Web.api.Service
{
    public class ReporteService : IReporteService
    {
        private readonly FirestoreDb _firestoreDb;

        public ReporteService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<List<ClasificacionDto>> ObtenerClasificaciones(string juegoId, int pagina = 1, int? nivelMin = null, int? nivelMax = null)
        {
            var clasificacionesRef = _firestoreDb.Collection("clasificaciones");
            var query = clasificacionesRef.WhereEqualTo("JuegoId", juegoId);

            var snapshot = await query.GetSnapshotAsync();
            var clasificaciones = snapshot.Documents
                .Select(doc => doc.ConvertTo<Clasificacion>())
                .OrderBy(c => c.Posicion)
                .ToList();

            // Aplicar filtros de nivel
            if (nivelMin.HasValue)
                clasificaciones = clasificaciones.Where(c => c.Nivel >= nivelMin.Value).ToList();
            if (nivelMax.HasValue)
                clasificaciones = clasificaciones.Where(c => c.Nivel <= nivelMax.Value).ToList();

            // Paginación: máximo 50 por página
            int skip = (pagina - 1) * 50;
            clasificaciones = clasificaciones.Skip(skip).Take(50).ToList();

            // Obtener nombres de jugadores
            var jugadorIds = clasificaciones.Select(c => c.JugadorId).Distinct().ToList();
            var jugadores = await ObtenerJugadoresPorIds(jugadorIds);

            var result = clasificaciones.Select(c =>
            {
                var jugador = jugadores.FirstOrDefault(j => j.Id == c.JugadorId);
                return new ClasificacionDto
                {
                    Posicion = c.Posicion,
                    NombreJugador = jugador?.Nombre ?? "Desconocido",
                    Puntos = c.Puntos,
                    Nivel = c.Nivel,
                    RatioVictoria = c.RatioVictoria,
                    TotalPartidas = c.TotalPartidas,
                    RachaActual = c.RachaActual
                };
            }).ToList();

            return result;
        }

        public async Task<ClasificacionJugadorDto> ObtenerClasificacionJugador(string juegoId, string jugadorId)
        {
            var clasificacionesRef = _firestoreDb.Collection("clasificaciones");
            var query = clasificacionesRef
                .WhereEqualTo("JuegoId", juegoId)
                .WhereEqualTo("JugadorId", jugadorId);

            var snapshot = await query.GetSnapshotAsync();
            var clasificacion = snapshot.Documents
                .Select(doc => doc.ConvertTo<Clasificacion>())
                .FirstOrDefault();

            if (clasificacion == null) return null;

            var jugador = await ObtenerJugadorPorId(jugadorId);

            return new ClasificacionJugadorDto
            {
                Posicion = clasificacion.Posicion,
                Puntos = clasificacion.Puntos,
                Nivel = clasificacion.Nivel,
                Medallas = jugador?.Medallas ?? new List<string>(),
                Logros = jugador?.Logros ?? new List<string>()
            };
        }

        public async Task<List<TorneoPopularDto>> ObtenerTorneosPopulares()
        {
            var torneosRef = _firestoreDb.Collection("torneos");
            var fechaLimite = DateTime.UtcNow.AddDays(-30);
            var query = torneosRef.WhereGreaterThan("FechaCreacion", fechaLimite);

            var snapshot = await query.GetSnapshotAsync();
            var torneos = snapshot.Documents
                .Select(doc => doc.ConvertTo<Torneo>())
                .OrderByDescending(t => t.CantidadInscripciones)
                .Take(10)
                .ToList();

            // Obtener nombres de juegos
            var juegoIds = torneos.Select(t => t.JuegoId).Distinct().ToList();
            var juegos = await ObtenerJuegosPorIds(juegoIds);

            var result = torneos.Select(t =>
            {
                var juego = juegos.FirstOrDefault(j => j.Id == t.JuegoId);
                return new TorneoPopularDto
                {
                    Nombre = t.Nombre,
                    Juego = juego?.Titulo ?? "Desconocido",
                    CantidadInscripciones = t.CantidadInscripciones,
                    PremioTotal = t.PremioTotal,
                    Estado = t.Estado
                };
            }).ToList();

            return result;
        }

        public async Task<List<JugadorDestacadoDto>> ObtenerJugadoresDestacados()
        {
            var jugadoresRef = _firestoreDb.Collection("jugadores");
            var snapshot = await jugadoresRef.GetSnapshotAsync();
            var jugadores = snapshot.Documents
                .Select(doc => doc.ConvertTo<Jugador>())
                .OrderByDescending(j => j.PuntosGlobales)
                .Take(20)
                .ToList();

            var result = jugadores.Select(j => new JugadorDestacadoDto
            {
                Nombre = j.Nombre,
                PuntosGlobales = j.PuntosGlobales,
                TorneosGanados = j.TorneosGanados,
                CantidadJuegos = j.JuegosDondeJuega.Count
            }).ToList();

            return result;
        }

        public async Task<DesempenoDto> ObtenerMiDesempeno(string juegoId, string jugadorId)
        {
            var clasificacion = await ObtenerClasificacionJugador(juegoId, jugadorId);
            var jugador = await ObtenerJugadorPorId(jugadorId);

            if (clasificacion == null || jugador == null) return null;

            // Calcular progreso (simplificado: asumir 1000 puntos por nivel)
            int puntosParaSiguiente = clasificacion.Nivel * 1000;
            double progreso = (double)clasificacion.Puntos / puntosParaSiguiente * 100;

            // Mejores torneos: simular top 3 (necesitaría datos reales)
            var mejoresTorneos = new List<TorneoTopDto>
            {
                new TorneoTopDto { NombreTorneo = "Torneo 1", Puntuacion = 1500 },
                new TorneoTopDto { NombreTorneo = "Torneo 2", Puntuacion = 1400 },
                new TorneoTopDto { NombreTorneo = "Torneo 3", Puntuacion = 1300 }
            };

            return new DesempenoDto
            {
                NivelActual = clasificacion.Nivel,
                PosicionRanking = clasificacion.Posicion,
                ProgresoSiguienteNivel = progreso,
                RatioVictoria = clasificacion.Puntos > 0 ? (double)clasificacion.TotalPartidas / clasificacion.Puntos : 0, // Simplificado
                RachaActual = clasificacion.RachaActual,
                MedallasObtenidas = jugador.Medallas,
                MejoresTorneos = mejoresTorneos
            };
        }

        public async Task<TendenciasDto> ObtenerTendencias()
        {
            // Juegos más populares: top 5 por jugadores activos
            var juegosRef = _firestoreDb.Collection("juegos");
            var snapshot = await juegosRef.GetSnapshotAsync();
            var juegos = snapshot.Documents
                .Select(doc => doc.ConvertTo<Juego>())
                .OrderByDescending(j => j.JugadoresActivos)
                .Take(5)
                .ToList();

            var juegosPopulares = juegos.Select(j => new JuegoPopularDto
            {
                Nombre = j.Titulo,
                Popularidad = j.JugadoresActivos
            }).ToList();

            // Géneros con más torneos activos
            var torneosRef = _firestoreDb.Collection("torneos");
            var torneosSnapshot = await torneosRef.WhereEqualTo("Estado", "activo").GetSnapshotAsync();
            var torneosActivos = torneosSnapshot.Documents.Select(doc => doc.ConvertTo<Torneo>()).ToList();

            var generos = new Dictionary<string, int>();
            foreach (var torneo in torneosActivos)
            {
                var juego = await ObtenerJuegoPorId(torneo.JuegoId);
                if (juego != null)
                {
                    if (generos.ContainsKey(juego.Genero))
                        generos[juego.Genero]++;
                    else
                        generos[juego.Genero] = 1;
                }
            }

            // Hora pico: simplificado, asumir 20:00
            string horaPico = "20:00";

            return new TendenciasDto
            {
                JuegosMasPopulares = juegosPopulares,
                GenerosConMasTorneos = generos,
                HoraPicoActividad = horaPico
            };
        }

        // Métodos auxiliares
        private async Task<List<Jugador>> ObtenerJugadoresPorIds(List<string> ids)
        {
            var jugadores = new List<Jugador>();
            foreach (var id in ids)
            {
                var jugador = await ObtenerJugadorPorId(id);
                if (jugador != null) jugadores.Add(jugador);
            }
            return jugadores;
        }

        private async Task<Jugador> ObtenerJugadorPorId(string id)
        {
            var docRef = _firestoreDb.Collection("jugadores").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Jugador>() : null;
        }

        private async Task<List<Juego>> ObtenerJuegosPorIds(List<string> ids)
        {
            var juegos = new List<Juego>();
            foreach (var id in ids)
            {
                var juego = await ObtenerJuegoPorId(id);
                if (juego != null) juegos.Add(juego);
            }
            return juegos;
        }

        private async Task<Juego> ObtenerJuegoPorId(string id)
        {
            var docRef = _firestoreDb.Collection("juegos").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Juego>() : null;
        }
    }
}
