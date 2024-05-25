namespace DonkeyLearn.Models
{
    public class RespuestaModel
    {
        public string ID { get; set; } // Añade esta línea
        public string Respuesta { get; set; }
        public bool EsCorrecta { get; set; }

    }
}
