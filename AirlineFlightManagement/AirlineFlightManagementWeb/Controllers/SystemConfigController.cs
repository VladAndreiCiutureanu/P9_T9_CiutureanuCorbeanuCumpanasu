using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagementWeb.Models.ViewModels.SystemConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineFlightManagementWeb.Controllers
{
    /// <summary>
    /// REQ-27 — UI admin pentru editarea credentialelor API (SerpAPI).
    /// Strict pentru Administrator (Staff NU vede cheia).
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class SystemConfigController : Controller
    {
        private readonly ISystemConfigService _configService;

        public SystemConfigController(ISystemConfigService configService)
        {
            _configService = configService;
        }

        // GET /SystemConfig — afiseaza pagina cu cheia curenta mascata
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await BuildViewModelAsync();
            return View(vm);
        }

        // POST /SystemConfig — admin actualizeaza cheia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemConfigViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var refreshed = await BuildViewModelAsync();
                refreshed.NewApiKey = vm.NewApiKey;  // pastreaza input-ul
                return View(refreshed);
            }

            try
            {
                await _configService.UpdateSerpApiKeyAsync(vm.NewApiKey.Trim());
                TempData["Success"] = "Cheia SerpAPI a fost actualizata cu succes.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Eroare la salvarea cheii: {ex.Message}");
                var refreshed = await BuildViewModelAsync();
                refreshed.NewApiKey = vm.NewApiKey;
                return View(refreshed);
            }
        }

        // ─── Helper privat ─────────────────────────────────────────────────
        private async Task<SystemConfigViewModel> BuildViewModelAsync()
        {
            var metadata = await _configService.GetMetadataAsync(
                ISystemConfigService.SerpApiKeyName);

            var vm = new SystemConfigViewModel();

            if (metadata != null && !string.IsNullOrEmpty(metadata.SettingValue))
            {
                var key = metadata.SettingValue;
                vm.CurrentKeyLength = key.Length;
                vm.CurrentKeyPreview = key.Length >= 12
                    ? $"{key.Substring(0, 8)}...{key.Substring(key.Length - 4)}"
                    : "***";
                vm.LastUpdated = metadata.LastUpdated;
            }

            return vm;
        }
    }
}
