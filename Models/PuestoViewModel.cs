using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace promerica_frontend.Models
{
    public class PuestoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El código debe ser mayor que cero.")]
        public int Codigo { get; set; }

        [Required(ErrorMessage = "El nombre del puesto es obligatorio.")]
        public string? Puesto { get; set; }

        [Required(ErrorMessage = "El nombre de la persona es obligatorio.")]
        public string? Nombre { get; set; }

        public int? CodigoJefe { get; set; } // puede ser nulo

        [JsonProperty("subordinados")]
        public List<PuestoViewModel> Subordinados { get; set; } = new();
    }
}
