namespace Web.Controllers
{
    using Common;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ImportController : BaseController
    {
        [Authorize(Roles = "1")]
        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            ViewData["UserId"] = this.GetUserId();

            return View();
        }

        public IActionResult Project()
        {
            return View();
        }

        public IActionResult Timesheet()
        {
            return View();
        }
    }
}
