using System.Collections.Generic;

namespace DonkeyLearn.Models
{
    public class PreguntaModel
    {
        public string Pregunta { get; set; }
        public List<RespuestaModel> Respuestas { get; set; }
        public RespuestaModel RespuestaSeleccionada { get; set; } // Añade esta línea
    }

}
