using Microsoft.AspNetCore.Mvc;
using ShopAPI.Singleton;

namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IAppConfigService _config;

        public ConfigController(IAppConfigService config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            var config = _config.GetConfiguration();
            return Ok(config);
        }
    }
}