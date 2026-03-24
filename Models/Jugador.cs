
using Google.Cloud.Firestore;

namespace ExamenII_Web.api.Models;

[FirestoreData]
public class Jugador
{        [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty]
    public string UserName { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string UserLastName { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Pass { get; set; } = string.Empty;

    
    [FirestoreProperty]
    public string NickName { get; set; } = string.Empty;

    [FirestoreProperty]
    public Timestamp BirthDate { get; set; } 
    
    [FirestoreProperty]
    public string Contry { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Role { get; set; } = "jugador";

    [FirestoreProperty]
    public bool IsActive { get; set; } = true;
    
    [FirestoreProperty]
    public int GlobalPoints { get; set; } = 0;
    
    [FirestoreProperty]
    public int TournamentWon { get; set; } = 0;

    [FirestoreProperty]
    public Timestamp RegistrationDate { get; set; }
    
    [FirestoreProperty("IsOnline")]
    public bool IsOnline { get; set; } 

    [FirestoreProperty("LastConect")]
    public Timestamp LastConect { get; set; }
    
}

