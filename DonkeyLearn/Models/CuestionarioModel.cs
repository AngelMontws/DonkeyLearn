using System.Collections.Generic;

namespace DonkeyLearn.Models
{
    public class CuestionarioModel
    {
        public string ID { get; set; }
        public string NombreCuestionario { get; set; }
        public List<PreguntaModel> Preguntas { get; set; }
        public int Puntaje { get; set; } // Añade esta línea
    }


}
