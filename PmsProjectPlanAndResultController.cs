using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsProjectPlanAndResultController : Controller
    {
        private readonly BM_NAVES_PortalContext _db;

        public PmsProjectPlanAndResultController(BM_NAVES_PortalContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var ProjectList = _db.TNAV_PROJECTs.ToList().OrderByDescending(m => m.PROJECT_ID);
            ViewBag.dataSource = ProjectList;
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Modify()
        {
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }
    }
}
