namespace ExamenII_Web.api.Models;
using Google.Cloud.Firestore;

[FirestoreData]
public class Participacion
{
    [FirestoreProperty]
    public string torneoId { get; set; }

    [FirestoreProperty]
    public string jugadorId { get; set; }

    [FirestoreProperty]
    public string estado { get; set; }

    [FirestoreProperty]
    public int victorias { get; set; }

    [FirestoreProperty]
    public int derrotas { get; set; }

    [FirestoreProperty]
    public int puntosObtenidos { get; set; }

    [FirestoreProperty]
    public int partidasJugadas { get; set; }

    [FirestoreProperty]
    public int posicionActual { get; set; }

    [FirestoreProperty]
    public Timestamp fechaInscripcion { get; set; }

    [FirestoreProperty]
    public Timestamp fechaEliminacion { get; set; }
}
