using Microsoft.AspNetCore.Mvc;

namespace SharedCircle.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
