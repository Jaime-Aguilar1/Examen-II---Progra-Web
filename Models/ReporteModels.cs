using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace ExamenII_Web.api.Models
{
    [FirestoreData]
    public class Clasificacion
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("jugadorId")]
        public string JugadorId { get; set; }

        [FirestoreProperty("juegoId")]
        public string JuegoId { get; set; }

        [FirestoreProperty("posicion")]
        public int Posicion { get; set; }

        [FirestoreProperty("puntosJuego")]
        public double PuntosJuego { get; set; }

        [FirestoreProperty("nivelJuego")]
        public int NivelJuego { get; set; }

        [FirestoreProperty("torneoGanados")]
        public int TorneoGanados { get; set; }

        [FirestoreProperty("ratioVictoria")]
        public double RatioVictoria { get; set; }

        [FirestoreProperty("totalPartidas")]
        public int TotalPartidas { get; set; }

        [FirestoreProperty("racha")]
        public int Racha { get; set; }

        [FirestoreProperty("rachaMaxima")]
        public int RachaMaxima { get; set; }

        [FirestoreProperty("medallasOro")]
        public int MedallasOro { get; set; }

        [FirestoreProperty("medallaPlata")]
        public int MedallaPlata { get; set; }

        [FirestoreProperty("medallaBronce")]
        public int MedallaBronce { get; set; }

        [FirestoreProperty("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; }

        [FirestoreProperty("estiloJuego")]
        public string EstiloJuego { get; set; }

        [FirestoreProperty("logros")]
        public List<string> Logros { get; set; }
    }
}