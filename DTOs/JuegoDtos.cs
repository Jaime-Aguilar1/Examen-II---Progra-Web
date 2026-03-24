namespace ExamenII_Web.api.DTOs;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class JuegoDto
{
    [Required] public string Titulo { get; set; }
    [Required] public string Desarrollador { get; set; }
    [Required] public string Genero { get; set; }
    [Required] public List<string> Plataformas { get; set; }
    [Required] public DateTime FechaLanzamiento { get; set; }
    [Required, MinLength(20)] public string Descripcion { get; set; }

}