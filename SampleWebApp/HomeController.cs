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
                {"value",  _options.Value ?? "undefined" }
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

        [HttpGet]
        public ActionResult<Dictionary<string, string>> Get()
        {
            var result = new Dictionary<string, string>
            {
                {"value: ",  _options.Value ?? "undefined" }
            };

            if (_options.Metadata != null)
            {
                foreach (var (key, value) in _options.Metadata)
                {
                    result.Add($"metadata/{key}:", value);
                }
            }

            return result;
        }
    }
}
