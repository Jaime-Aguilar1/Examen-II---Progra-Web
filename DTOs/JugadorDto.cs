


using Google.Cloud.Firestore;

namespace ExamenII_Web.api.DTOs
{
    public class JugadorDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; } 
        public string NickName { get; set; } = string.Empty; 
        public int GlobalPoints { get; set; }
        public int TournamentWon { get; set; } 
        public string Role { get; set; } = "jugador"; 
       
        public bool IsOnline { get; set; }
        public string Email { get; set; }
    }

    public class RegisterDto
    {
        public string UserName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string Pass { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } 
        public string Contry { get; set; } = string.Empty;
    }


    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
    }


    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; 
        public JugadorDto Jugador { get; set; } = new JugadorDto();
    }
    
    public class ActualizarPerfilDto
    {
        public string UserName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } 
        public string Contry { get; set; } = string.Empty;
    }
    
    
}

