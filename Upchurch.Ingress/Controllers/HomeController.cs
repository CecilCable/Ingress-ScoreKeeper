using System.Web.Mvc;

namespace Upchurch.Ingress.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Angular()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult OverallScore()
        {
            return PartialView();
        }

        [HttpGet]
        public PartialViewResult Update()
        {
            return PartialView();
        }

                [HttpGet]
        public PartialViewResult MissingCheckpoint()
        {
            return PartialView();
        }

                [HttpGet]
        public PartialViewResult Scores()
        {
            return PartialView();
        }

                [HttpGet]
        public PartialViewResult Summary()
        {
            return PartialView();
        }



    }
}