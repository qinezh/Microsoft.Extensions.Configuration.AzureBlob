using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private BlobSetting _options;

        public HomeController(IOptionsSnapshot<BlobSetting> options)
        {
            _options = options.Value;
        }

        public IActionResult Index()
        {
            var config = new Dictionary<string, string>
            {
                {"StorageAccountName",  _options.StorageAccountName ?? "undefined" },
                {"BlobContainerName",  _options.BlobContainerName ?? "undefined" },
                {"ConfigurationFile",  _options.ConfigurationFile ?? "undefined" }
            };

            if (_options.Metadata != null)
            {
                foreach (var (key, value) in _options.Metadata)
                {
                    config.Add($"metadata:{key}", value);
                }
            }

            ViewData["config"] = config;

            return View();
        }
    }
}
