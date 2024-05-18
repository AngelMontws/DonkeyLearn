using System.Collections.Generic;

namespace DonkeyLearn.Models
{
    public class CuestionarioModel
    {
        public string NombreCuestionario { get; set; }
        public List<PreguntaModel> Preguntas { get; set; }

    }
}
