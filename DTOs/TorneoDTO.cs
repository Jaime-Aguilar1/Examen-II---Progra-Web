using System;
using System.ComponentModel.DataAnnotations;

namespace ExamenII_Web.api.DTOs
{
    public class TorneoDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El ID del juego es obligatorio.")]
        public string Juego { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El formato es obligatorio.")]
        [RegularExpression("^(individual|equipos|royale)$", ErrorMessage = "El formato debe ser 'individual', 'equipos' o 'royale'.")]
        public string Formato { get; set; }

        [Required]
        [Range(3, 10000, ErrorMessage = "La cantidad máxima de participantes debe ser mayor a 2.")]
        public int MaxParticipantes { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de inscripción no puede ser negativo.")]
        public double PrecioInscripcion { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El premio total no puede ser negativo.")]
        public double PremioTotal { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public DateTime FechaLimiteInscripcion { get; set; }

        [Range(1, 100, ErrorMessage = "El nivel mínimo debe estar entre 1 y 100.")]
        public int MinNivel { get; set; }

        [Range(0, 100, ErrorMessage = "El nivel máximo debe estar entre 0 y 100 (0 = sin límite).")]
        public int MaxNivel { get; set; }

        public bool RequiereEquipo { get; set; }

        [Range(1, 100, ErrorMessage = "El tamaño del equipo debe ser al menos 1.")]
        public int TamanioEquipo { get; set; }
    }

    public class TorneoUpdateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Required]
        [Range(3, 10000, ErrorMessage = "La cantidad máxima de participantes debe ser mayor a 2.")]
        public int MaxParticipantes { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de inscripción no puede ser negativo.")]
        public double PrecioInscripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaLimiteInscripcion { get; set; }

        [Range(1, 100, ErrorMessage = "El nivel mínimo debe estar entre 1 y 100.")]
        public int MinNivel { get; set; }

        [Range(0, 100, ErrorMessage = "El nivel máximo debe estar entre 0 y 100.")]
        public int MaxNivel { get; set; }
    }
}