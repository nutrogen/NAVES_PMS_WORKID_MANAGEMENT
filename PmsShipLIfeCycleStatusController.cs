using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsShipLIfeCycleStatusController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;

        public PmsShipLIfeCycleStatusController(BM_NAVES_PortalContext repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            ViewBag.dataSource = _repository.VNAV_SELECT_PMS_SHIPLIFECYCLE_LISTs.ToList().OrderByDescending(m => m.NO);
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

        public IActionResult Detail(Guid? id)
        {
            ViewBag.dataSource = _repository.VNAV_SELECT_PMS_SHIPLIFECYCLE_LISTs.Where(m => m.NSN_IDX == id).ToList();
            return View();
        }
    }
}
