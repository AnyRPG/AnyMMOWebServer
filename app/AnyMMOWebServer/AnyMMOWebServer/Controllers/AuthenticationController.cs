using Microsoft.AspNetCore.Mvc;

namespace AnyMMOWebServer.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
