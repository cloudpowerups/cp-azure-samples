using System.Web.Mvc;

namespace CP.Azure.Samples.MultiComponentRole.Api.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}