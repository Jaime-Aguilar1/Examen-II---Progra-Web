using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
namespace Proyecto_Grupo4.API.Models
{
    [FirestoreData] 
    public class Juego
    {
        [FirestoreDocumentId]
        public string Id { get; set; } 
        [FirestoreProperty]
        public string Titulo { get; set; }

        [FirestoreProperty]
        public string Desarrollador { get; set; }

        [FirestoreProperty]
        public string Genero { get; set; }

        [FirestoreProperty]
        public List<string> Plataformas { get; set; }

        [FirestoreProperty]
        public DateTime FechaLanzamiento { get; set; }

        [FirestoreProperty]
        public string Descripcion { get; set; }

        [FirestoreProperty]
        public int JugadoresActivos { get; set; } = 0;

        [FirestoreProperty]
        public int TorneoActivos { get; set; } = 0;

        [FirestoreProperty]
        public string Estado { get; set; } = "disponible"; 

        [FirestoreProperty]
        public double PuntuacionPromedio { get; set; } = 0.0;

        [FirestoreProperty]
        public DateTime FechaAgreg { get; set; } = DateTime.UtcNow;
    }
}