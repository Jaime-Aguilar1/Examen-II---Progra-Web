using Google.Cloud.Firestore;
using System;

namespace ExamenII_Web.api.Models
{
    [FirestoreData]
    public class Torneo
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("juego")]
        public string Juego { get; set; }

        [FirestoreProperty("organizador")]
        public string Organizador { get; set; }

        [FirestoreProperty("descripcion")]
        public string Descripcion { get; set; }

        [FirestoreProperty("estado")]
        public string Estado { get; set; }

        [FirestoreProperty("formato")]
        public string Formato { get; set; }

        [FirestoreProperty("maxParticipantes")]
        public int MaxParticipantes { get; set; }

        [FirestoreProperty("participantesActuales")]
        public int ParticipantesActuales { get; set; }

        [FirestoreProperty("precioInscripcion")]
        public double PrecioInscripcion { get; set; }

        [FirestoreProperty("premioTotal")]
        public double PremioTotal { get; set; }

        [FirestoreProperty("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [FirestoreProperty("fechaFin")]
        public DateTime FechaFin { get; set; }

        [FirestoreProperty("fechaLimiteInscripcion")]
        public DateTime FechaLimiteInscripcion { get; set; }

        [FirestoreProperty("minNivel")]
        public int MinNivel { get; set; }

        [FirestoreProperty("maxNivel")]
        public int MaxNivel { get; set; }

        [FirestoreProperty("requiereEquipo")]
        public bool RequiereEquipo { get; set; }

        [FirestoreProperty("tamanioEquipo")]
        public int TamanioEquipo { get; set; }

        [FirestoreProperty("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [FirestoreProperty("reglasModificadas")]
        public bool ReglasModificadas { get; set; }
    }
}