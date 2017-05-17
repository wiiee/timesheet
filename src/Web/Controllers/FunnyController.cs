namespace Web.Controllers
{
    using Common;
    using Microsoft.AspNetCore.Mvc;

    public class FunnyController : BaseController
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return PartialView();
        }

        public IActionResult Murmur()
        {
            return View();
        }
    }
}
