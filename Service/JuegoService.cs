using Google.Cloud.Firestore;
using ExamenII_Web.api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamenII_Web.api.Service;

namespace ExamenII_Web.api.Service
{
    public class JuegoService : IJuegoService

{
        private readonly FirestoreDb _firestoreDb;
        private const string Coleccion = "juegos";

        public JuegoService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // ================================
        // Agregar juego
        // ================================
        public async Task<Juego> AgregarJuego(Juego juego)
        {
            var coleccion = _firestoreDb.Collection(Coleccion);

            juego.Titulo = juego.Titulo.Trim().ToLower();

            var snapshot = await coleccion
                .WhereEqualTo("Titulo", juego.Titulo)
                .GetSnapshotAsync();

            if (snapshot.Count > 0)
                throw new InvalidOperationException("Ya existe un juego con ese título");

            var plataformasValidas = new List<string> { "PC", "PS5", "Xbox", "Switch" };
            if (juego.Plataformas == null || juego.Plataformas.Count == 0 || juego.Plataformas.Any(p => !plataformasValidas.Contains(p)))
                throw new ArgumentException("Plataformas inválidas");

            if (string.IsNullOrWhiteSpace(juego.Descripcion) || juego.Descripcion.Length < 20)
                throw new ArgumentException("La descripción debe tener al menos 20 caracteres");

            juego.Estado = "disponible";
            juego.PuntuacionPromedio = 0.0;
            juego.JugadoresActivos = 0;
            juego.TorneoActivos = 0;
            juego.FechaAgreg = DateTime.UtcNow;

            var docRef = await coleccion.AddAsync(juego);
            juego.Id = docRef.Id;

            return juego;
        }

        // ================================
        // Listar juegos con filtros
        // ================================
        public async Task<List<Juego>> ListarJuegos(string genero = null, string plataforma = null, string desarrollador = null)
        {
            var coleccion = _firestoreDb.Collection(Coleccion);
            var snapshot = await coleccion.GetSnapshotAsync();

            var juegos = snapshot.Documents
                .Select(doc =>
                {
                    var juego = doc.ConvertTo<Juego>();
                    juego.Id = doc.Id;
                    return juego;
                })
                .Where(j => j.Estado == "disponible")
                .ToList();

            if (!string.IsNullOrEmpty(genero))
                juegos = juegos.Where(j => j.Genero.ToLower() == genero.ToLower()).ToList();

            if (!string.IsNullOrEmpty(plataforma))
                juegos = juegos.Where(j => j.Plataformas.Contains(plataforma)).ToList();

            if (!string.IsNullOrEmpty(desarrollador))
                juegos = juegos.Where(j => j.Desarrollador.ToLower() == desarrollador.ToLower()).ToList();

            return juegos;
        }

        // ================================
        // Obtener todos los juegos (nuevo)
        // ================================
        public async Task<List<Juego>> ObtenerTodosJuegos()
        {
            var coleccion = _firestoreDb.Collection(Coleccion);
            var snapshot = await coleccion.GetSnapshotAsync();

            var juegos = snapshot.Documents.Select(doc =>
            {
                var juego = doc.ConvertTo<Juego>();
                juego.Id = doc.Id;
                return juego;
            }).ToList();

            return juegos;
        }

        // ================================
        // Actualizar juego
        // ================================
        public async Task<Juego> ActualizarJuego(string id, Juego juegoActualizado)
        {
            var docRef = _firestoreDb.Collection(Coleccion).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new KeyNotFoundException("Juego no encontrado");

            var estadosValidos = new List<string> { "disponible", "mantenimiento", "descontinuado" };
            var updates = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(juegoActualizado.Descripcion))
            {
                if (juegoActualizado.Descripcion.Length < 20)
                    throw new ArgumentException("Descripción muy corta");
                updates["Descripcion"] = juegoActualizado.Descripcion;
            }

            if (juegoActualizado.PuntuacionPromedio > 0)
            {
                if (juegoActualizado.PuntuacionPromedio > 5)
                    throw new ArgumentException("Puntuación inválida (0-5)");
                updates["PuntuacionPromedio"] = juegoActualizado.PuntuacionPromedio;
            }

            if (!string.IsNullOrEmpty(juegoActualizado.Estado))
            {
                if (!estadosValidos.Contains(juegoActualizado.Estado))
                    throw new ArgumentException("Estado inválido");
                updates["Estado"] = juegoActualizado.Estado;
            }

            if (updates.Count == 0)
                throw new ArgumentException("No se enviaron datos para actualizar");

            await docRef.UpdateAsync(updates);

            var updatedSnapshot = await docRef.GetSnapshotAsync();
            var juego = updatedSnapshot.ConvertTo<Juego>();
            juego.Id = updatedSnapshot.Id;

            return juego;
        }

        // ================================
        // Eliminar juego
        // ================================
        public async Task EliminarJuego(string id)
        {
            var docRef = _firestoreDb.Collection(Coleccion).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new KeyNotFoundException("Juego no encontrado");

            var juego = snapshot.ConvertTo<Juego>();

            if (juego.TorneoActivos > 0)
                throw new InvalidOperationException("No se puede eliminar porque tiene torneos activos");

            if (juego.JugadoresActivos > 0)
                throw new InvalidOperationException("No se puede eliminar porque tiene jugadores activos");

            await docRef.DeleteAsync();
        }

        // ================================
        // Obtener estadísticas por ID
        // ================================
        public async Task<Juego> ObtenerEstadisticas(string id)
        {
            var juego = await ObtenerJuegoPorId(id);

            if (juego == null)
                throw new KeyNotFoundException("Juego no encontrado");

            return juego;
        }

        // ================================
        // Obtener juego por ID
        // ================================
        public async Task<Juego> ObtenerJuegoPorId(string id)
        {
            var docRef = _firestoreDb.Collection(Coleccion).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var juego = snapshot.ConvertTo<Juego>();
            juego.Id = snapshot.Id;

            return juego;
        }
    }
}