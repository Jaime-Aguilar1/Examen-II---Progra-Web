using Google.Cloud.Firestore;

namespace ExamenII_Web.api.DTOs;

public class MisTorneosDTO
{
    public string torneoId { get; set; }
    public string estado { get; set; }
    public int posicion { get; set; }
    public int puntos { get; set; }
    public int victorias { get; set; }
    public int derrotas { get; set; }
    public Timestamp fechaInscripcion { get; set; }
}
