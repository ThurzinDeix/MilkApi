using Microsoft.AspNetCore.Mvc;

namespace MilkApi.Controllers
{
    public class TipoVacinaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
