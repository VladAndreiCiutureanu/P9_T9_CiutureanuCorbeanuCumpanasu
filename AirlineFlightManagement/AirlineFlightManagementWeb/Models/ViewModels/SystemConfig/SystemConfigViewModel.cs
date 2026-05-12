using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagementWeb.Models.ViewModels.SystemConfig
{
    /// <summary>
    /// REQ-27 — pagina /SystemConfig pentru editarea credentialelor API.
    /// Doar Admin (verificat in controller).
    /// </summary>
    public class SystemConfigViewModel
    {
        // ─── Afisare cheie curenta (mascata pentru securitate) ──────────────
        public string CurrentKeyPreview { get; set; } = "<not set>";
        public int CurrentKeyLength { get; set; }
        public DateTime? LastUpdated { get; set; }

        // ─── Input pentru actualizare ───────────────────────────────────────
        // Lasata goala la load — admin scrie noua cheie aici.
        [Required(ErrorMessage = "Introduceti noua cheie API.")]
        [MinLength(10, ErrorMessage = "Cheia pare prea scurta — verificati.")]
        [Display(Name = "Noua cheie SerpAPI")]
        public string NewApiKey { get; set; } = string.Empty;
    }
}
