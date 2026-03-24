using ExamenII_Web.api.Models;
using Google.Cloud.Firestore;
using ExamenII_Web.api.DTOs;
using ExamenII_Web.api.Service;

namespace ExamenII_Web.api.Service;

public class ParticipacionService : IParticipacionService
    {
        private readonly FirestoreDb _db;

        public ParticipacionService(FirebaseService firebase)
        {
            _db = firebase.GetDb();
        }

        public async Task InscribirseTorneo(string torneoId, string jugadorId, bool pagado)
        {
            var torneoRef = _db.Collection("torneos").Document(torneoId);
            var torneoDoc = await torneoRef.GetSnapshotAsync();

            if (!torneoDoc.Exists)
                throw new Exception("Torneo no existe");

            var torneo = torneoDoc.ToDictionary();

            if (torneo["estado"].ToString() != "próximo")
                throw new Exception("El torneo no esta disponible");

            if (Timestamp.GetCurrentTimestamp() >
                (Timestamp)torneo["fechaLimiteInscripcion"])
                throw new Exception("Inscripcion cerrada");

            int actuales = Convert.ToInt32(torneo["participantesActuales"]);
            int max = Convert.ToInt32(torneo["maxParticipantes"]);

            if (actuales >= max)
                throw new Exception("No hay cupos");

            var jugadorDoc = await _db.Collection("jugadores")
                .Document(jugadorId).GetSnapshotAsync();

            if (!jugadorDoc.Exists)
                throw new Exception("Jugador no existe");

            var jugador = jugadorDoc.ToDictionary();

            if (!(bool)jugador["activo"])
                throw new Exception("Jugador no activo");

            int nivel = Convert.ToInt32(jugador["nivel"]);

            int min = Convert.ToInt32(torneo["minNivel"]);
            int maxNivel = Convert.ToInt32(torneo["maxNivel"]);

            if (nivel < min || nivel > maxNivel)
                throw new Exception("Nivel no permitido");

            if (Convert.ToDouble(torneo["precioInscripcion"]) > 0 && !pagado)
                throw new Exception("Debe confirmar pago");

            var participaciones = _db.Collection("participaciones");

            var yaInscrito = await participaciones
                .WhereEqualTo("torneoId", torneoId)
                .WhereEqualTo("jugadorId", jugadorId)
                .GetSnapshotAsync();

            if (yaInscrito.Count > 0)
                throw new Exception("Ya esta inscrito");

            Participacion p = new Participacion
            {
                torneoId = torneoId,
                jugadorId = jugadorId,
                estado = "inscrito",
                victorias = 0,
                derrotas = 0,
                puntosObtenidos = 0,
                partidasJugadas = 0,
                posicionActual = 0,
                fechaInscripcion = Timestamp.GetCurrentTimestamp()
            };

            await participaciones.AddAsync(p);

            await torneoRef.UpdateAsync("participantesActuales", actuales + 1);
        }

        public async Task<List<ParticipanteDTO>> ObtenerParticipantes(string torneoId, int pagina, int limite)
        {
            var participaciones = await _db.Collection("participaciones")
                .WhereEqualTo("torneoId", torneoId)
                .OrderByDescending("puntosObtenidos")
                .Limit(limite)
                .Offset((pagina - 1) * limite)
                .GetSnapshotAsync();

            List<ParticipanteDTO> lista = new();

            foreach (var doc in participaciones.Documents)
            {
                var p = doc.ConvertTo<Participacion>();

                var jugadorDoc = await _db.Collection("jugadores")
                    .Document(p.jugadorId)
                    .GetSnapshotAsync();

                var j = jugadorDoc.ToDictionary();

                lista.Add(new ParticipanteDTO
                {
                    nombre = j["nombre"].ToString(),
                    nombreUsuario = j["nombreUsuario"].ToString(),
                    nivel = Convert.ToInt32(j["nivel"]),
                    victorias = p.victorias,
                    derrotas = p.derrotas,
                    posicion = p.posicionActual
                });
            }

            return lista;
        }

        public async Task ActualizarResultado(string torneoId, string participacionId, ResultadoPartidaDTO dto)
        {
            var refDoc = _db.Collection("participaciones").Document(participacionId);
            var doc = await refDoc.GetSnapshotAsync();

            var p = doc.ConvertTo<Participacion>();

            if (dto.victoria)
                p.victorias++;
            else
                p.derrotas++;

            p.puntosObtenidos += dto.puntosPartida;
            p.partidasJugadas++;

            var torneoDoc = await _db.Collection("torneos")
                .Document(torneoId)
                .GetSnapshotAsync();

            var torneo = torneoDoc.ToDictionary();

            if (torneo["tipo"].ToString() == "eliminacion_directa" && !dto.victoria)
            {
                p.estado = "eliminado";
                p.fechaEliminacion = Timestamp.GetCurrentTimestamp();
            }

            await refDoc.SetAsync(p);
        }

        public async Task<List<MisTorneosDTO>> ObtenerMisTorneos(string jugadorId)
        {
            var participaciones = await _db.Collection("participaciones")
                .WhereEqualTo("jugadorId", jugadorId)
                .OrderByDescending("fechaInscripcion")
                .GetSnapshotAsync();

            List<MisTorneosDTO> lista = new();

            foreach (var doc in participaciones.Documents)
            {
                var p = doc.ConvertTo<Participacion>();

                lista.Add(new MisTorneosDTO
                {
                    torneoId = p.torneoId,
                    estado = p.estado,
                    posicion = p.posicionActual,
                    puntos = p.puntosObtenidos,
                    victorias = p.victorias,
                    derrotas = p.derrotas,
                    fechaInscripcion = p.fechaInscripcion
                });
            }

            return lista;
        }

        public async Task AbandonarTorneo(string torneoId, string participacionId, string jugadorId)
        {
            var refDoc = _db.Collection("participaciones").Document(participacionId);
            var doc = await refDoc.GetSnapshotAsync();

            var p = doc.ConvertTo<Participacion>();

            if (p.jugadorId != jugadorId)
                throw new Exception("No autorizado");

            if (p.estado != "inscrito")
                throw new Exception("No puede abandonar");

            var torneoRef = _db.Collection("torneos").Document(torneoId);
            var torneoDoc = await torneoRef.GetSnapshotAsync();

            var torneo = torneoDoc.ToDictionary();

            if (torneo["estado"].ToString() != "proximo")
                throw new Exception("El torneo ya inicio");

            p.estado = "abandonado";

            await refDoc.SetAsync(p);

            int actuales = Convert.ToInt32(torneo["participantesActuales"]);

            await torneoRef.UpdateAsync("participantesActuales", actuales - 1);
        }
    }
