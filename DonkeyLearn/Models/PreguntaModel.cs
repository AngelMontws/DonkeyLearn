using System.Collections.Generic;

namespace DonkeyLearn.Models
{
    public class PreguntaModel
    {
        public string Pregunta { get; set; }
        public List<RespuestaModel> Respuestas { get; set; }
        public string RespuestaSeleccionada { get; set; } // Cambia esto a string
    }


}
