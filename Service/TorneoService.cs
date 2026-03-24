using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamenII_Web.api.Models;
using ExamenII_Web.api.DTOs;

namespace ExamenII_Web.api.Services
{
    public class TorneoService : ITorneoService
    {
        private readonly FirestoreDb _firestoreDb;
        private const string Coleccion = "torneos";

        public TorneoService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<string> CrearTorneoAsync(TorneoDTO dto, string organizadorId)
        {
            if (dto.FechaInicio.ToUniversalTime() <= DateTime.UtcNow)
                throw new ArgumentException("La fecha de inicio debe ser posterior a hoy.");

            if (dto.FechaLimiteInscripcion >= dto.FechaInicio)
                throw new ArgumentException("La fecha límite de inscripción debe ser antes de la fecha de inicio.");

            DocumentSnapshot juegoDoc = await _firestoreDb.Collection("juegos").Document(dto.Juego).GetSnapshotAsync();
            if (!juegoDoc.Exists)
                throw new ArgumentException("El juego especificado no existe.");

            var torneo = new Torneo
            {
                Nombre = dto.Nombre,
                Juego = dto.Juego,
                Descripcion = dto.Descripcion,
                Formato = dto.Formato.ToLower(),
                MaxParticipantes = dto.MaxParticipantes,
                PrecioInscripcion = dto.PrecioInscripcion,
                PremioTotal = dto.PremioTotal,
                RequiereEquipo = dto.RequiereEquipo,
                TamanioEquipo = dto.TamanioEquipo,
                MinNivel = dto.MinNivel,
                MaxNivel = dto.MaxNivel,
                Organizador = organizadorId,
                ParticipantesActuales = 0,
                Estado = "próximo",
                ReglasModificadas = false,
                FechaCreacion = DateTime.UtcNow,
                FechaInicio = dto.FechaInicio.ToUniversalTime(),
                FechaFin = dto.FechaFin.ToUniversalTime(),
                FechaLimiteInscripcion = dto.FechaLimiteInscripcion.ToUniversalTime()
            };

            DocumentReference docRef = await _firestoreDb.Collection(Coleccion).AddAsync(torneo);
            return docRef.Id;
        }

        public async Task<List<Torneo>> ObtenerTorneosAsync(string juego, string estado, double? minPrecio, double? maxPrecio, int? minNivelReq, int? maxNivelReq, int page, int pageSize)
        {
            Query query = _firestoreDb.Collection(Coleccion)
                .WhereIn("estado", new[] { "próximo", "en progreso" })
                .OrderBy("fechaInicio");

            if (!string.IsNullOrEmpty(juego)) query = query.WhereEqualTo("juego", juego);
            if (!string.IsNullOrEmpty(estado)) query = query.WhereEqualTo("estado", estado);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            var torneos = snapshot.Documents.Select(d => d.ConvertTo<Torneo>()).ToList();

            if (minPrecio.HasValue) torneos = torneos.Where(t => t.PrecioInscripcion >= minPrecio.Value).ToList();
            if (maxPrecio.HasValue) torneos = torneos.Where(t => t.PrecioInscripcion <= maxPrecio.Value).ToList();
            if (minNivelReq.HasValue) torneos = torneos.Where(t => t.MinNivel >= minNivelReq.Value).ToList();
            if (maxNivelReq.HasValue) torneos = torneos.Where(t => t.MaxNivel <= maxNivelReq.Value).ToList();

            return torneos.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<bool> ActualizarTorneoAsync(string id, TorneoUpdateDto dto, string userId, bool isAdmin)
        {
            DocumentReference docRef = _firestoreDb.Collection(Coleccion).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) throw new KeyNotFoundException("Torneo no encontrado.");

            Torneo torneo = snapshot.ConvertTo<Torneo>();

            if (torneo.Organizador != userId && !isAdmin)
                throw new UnauthorizedAccessException("No tienes permisos para modificar este torneo.");

            if (torneo.Estado != "próximo")
                throw new InvalidOperationException("Solo se pueden actualizar torneos no iniciados.");

            if (dto.MaxParticipantes < torneo.ParticipantesActuales)
                throw new ArgumentException("No puedes reducir los cupos por debajo de los participantes actuales.");

            var updates = new Dictionary<string, object>
            {
                { "nombre", dto.Nombre },
                { "descripcion", dto.Descripcion },
                { "maxParticipantes", dto.MaxParticipantes },
                { "precioInscripcion", dto.PrecioInscripcion },
                { "fechaInicio", dto.FechaInicio.ToUniversalTime() },
                { "fechaLimiteInscripcion", dto.FechaLimiteInscripcion.ToUniversalTime() },
                { "minNivel", dto.MinNivel },
                { "maxNivel", dto.MaxNivel },
                { "reglasModificadas", true }
            };

            await docRef.UpdateAsync(updates);
            return true;
        }

        public async Task<bool> CancelarTorneoAsync(string id, string userId, bool isAdmin)
        {
            DocumentReference docRef = _firestoreDb.Collection(Coleccion).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) throw new KeyNotFoundException("Torneo no encontrado.");

            Torneo torneo = snapshot.ConvertTo<Torneo>();

            if (torneo.Organizador != userId && !isAdmin)
                throw new UnauthorizedAccessException("No tienes permisos para cancelar este torneo.");

            if (torneo.Estado != "próximo")
                throw new InvalidOperationException("Solo se pueden cancelar torneos que no han iniciado.");

            await docRef.UpdateAsync("estado", "cancelado");
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(string id, string nuevoEstado, string userId, bool isAdmin)
        {
            nuevoEstado = nuevoEstado.ToLower();
            if (nuevoEstado != "en progreso" && nuevoEstado != "finalizado")
                throw new ArgumentException("Estado inválido. Use 'en progreso' o 'finalizado'.");

            DocumentReference docRef = _firestoreDb.Collection(Coleccion).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) throw new KeyNotFoundException("Torneo no encontrado.");

            Torneo torneo = snapshot.ConvertTo<Torneo>();

            if (torneo.Organizador != userId && !isAdmin)
                throw new UnauthorizedAccessException("No tienes permisos para cambiar el estado.");

            if (torneo.Estado == "próximo" && nuevoEstado == "en progreso")
            {
                await docRef.UpdateAsync("estado", "en progreso");
                return true;
            }
            if (torneo.Estado == "en progreso" && nuevoEstado == "finalizado")
            {
                await docRef.UpdateAsync("estado", "finalizado");
                return true;
            }

            throw new InvalidOperationException($"Transición no válida de '{torneo.Estado}' a '{nuevoEstado}'.");
        }
    }
}