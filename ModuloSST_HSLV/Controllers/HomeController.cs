// Controllers/HomeController.cs
using System.Web.Mvc;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador de la página de inicio del Módulo SST — HSLV.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>Página de bienvenida del sistema.</summary>
        public ActionResult Index()
        {
            return View();
        }
    }
}