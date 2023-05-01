using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NavesPortalforWebWithCoreMvc.Models;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Base;
using System.Collections;
using NavesPortalforWebWithCoreMvc.Common;
using NavesPortalforWebWithCoreMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using Syncfusion.EJ2.Linq;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsJobIdManagementController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemContext;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;
        private readonly INavesPortalCommonService _common;
        private readonly IRfSystemCommonService _rfSystem;

        public PmsJobIdManagementController(BM_NAVES_PortalContext repository, INavesPortalCommonService common, RfSystemContext rfSystemContext, IRfSystemCommonService rfSystem, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = repository;
            _common = common;
            _rfSystemContext = rfSystemContext;
            _rfSystem = rfSystem;
            _procedures = procedures;
        }

        //public IActionResult Index()
        //{
        //    ViewBag.dataSource = _repository.VNAV_SELECT_PMS_JOB_LISTs.ToList().OrderByDescending(m => m.NO);
        //    return View();
        //}

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View());
        }

        /// <summary>
        /// 목록
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="dm"></param>
        /// <returns></returns>
        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_PMS_GET_JOB_LISTResult> resultList = await _procedures.PNAV_PMS_GET_JOB_LISTAsync(SearchString, StartDate, EndDate);

                IEnumerable DataSource = resultList.AsEnumerable();
                DataOperations operation = new DataOperations();

                //Search
                if (dm.Search != null && dm.Search.Count > 0)
                {
                    DataSource = operation.PerformSearching(DataSource, dm.Search);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
                {
                    DataSource = operation.PerformSorting(DataSource, dm.Sorted);
                }

                //Filtering
                if (dm.Where != null && dm.Where.Count > 0)
                {
                    DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
                }

                int count = DataSource.Cast<PNAV_PMS_GET_JOB_LISTResult>().Count();

                //Paging
                if (dm.Skip != 0)
                {

                    DataSource = operation.PerformSkip(DataSource, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    DataSource = operation.PerformTake(DataSource, dm.Take);
                }

                return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(new { result = DataSource });
            }
            catch (Exception e)
            {
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "PmsWorkIdManagement", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create()
        {
            // AutoComplate NSN List
            ViewBag.DataSourece = await _repository.TNAV_NSN_MANAGEMENTs.OrderByDescending(m => m.NSN_ID).ToListAsync();
            ViewBag.sort = "Ascending";

            return View();
        }

        /// <summary>
        /// NSN ID로 검색하기
        /// </summary>
        /// <param name="nsnId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTNAV_NSN(string nsnId)
        {
            try
            {
                // 선택한 NSN ID를 조회
                VNAV_SELECT_PMS_NSN_LIST result = _repository.VNAV_SELECT_PMS_NSN_LISTs.Where(m => m.NSN_ID == nsnId).First();

                // 조회된 NSN으로 생성된 Job Category를 조회해 중복된 Category를 생성할 수 없도록 한다.
                var existCategory = _repository.TNAV_JOB_MANAGEMENTs
                    .Where(m => m.NSN_IDX == result.NSN_IDX)
                    .GroupBy(m => m.JOB_CATEGORY)
                    .Select(m => m.Key).ToList();

                return Json(new
                {
                    data = result,
                    category = existCategory,
                    result = "OK"
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///  저장
        /// </summary>
        /// <param name="tNAV_JOB_MANAGEMENT"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TNAV_JOB_MANAGEMENT tNAV_JOB_MANAGEMENT)
        {
            try
            {
                // JOb ID : DSME + Hull NO + Client                            
                tNAV_JOB_MANAGEMENT.JOB_ID = createJobID(tNAV_JOB_MANAGEMENT.YARD_MAKER, tNAV_JOB_MANAGEMENT.HULL_NO, tNAV_JOB_MANAGEMENT.CLIENT, tNAV_JOB_MANAGEMENT.JOB_CATEGORY);
                tNAV_JOB_MANAGEMENT.JOB_IDX = Guid.NewGuid();
                tNAV_JOB_MANAGEMENT.CREATE_USER = HttpContext.Session.GetString("UserName");
                tNAV_JOB_MANAGEMENT.REG_DATE = DateTime.Now;
                //tNAV_JOB_MANAGEMENT.NSN_IDXNavigation = tNAV_JOB_MANAGEMENT.NSN_IDX;

                //if (ModelState.IsValid)
                //{

                // 상태값 저장
                TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                {
                    REG_DATE = DateTime.Now,
                    USER_NAME = HttpContext.Session.GetString("UserName"),
                    PLATFORM = "PMS",
                    MENU_NAME = "JOB_ID",
                    TARGET_IDX = tNAV_JOB_MANAGEMENT.JOB_IDX,
                    STATUS = CommonSettingData.LogStatus.CREATE.ToString()
                };
                _repository.Add(LogViewModel);

                _repository.Add(tNAV_JOB_MANAGEMENT);
                await _repository.SaveChangesAsync();
                return RedirectToAction(nameof(Detail), new { id = tNAV_JOB_MANAGEMENT.JOB_IDX });
            }
            catch (Exception e)
            {
                //throw;
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "PmsJobIdManagement", returnView= "Create" });
            }
        }

        /// <summary>
        /// Job Id 생성
        /// </summary>
        /// <param name="ShipBuilder"></param>
        /// <param name="HullNo"></param>
        /// <param name="Client"></param>
        /// <returns></returns>
        private string createJobID(string ShipBuilder, string HullNo, Guid Client, string JobCategory)
        {
            string _client = string.Empty;

            //string sn = (getCountJobId(JobCategory) + 1).ToString("000");
            switch (JobCategory)
            {
                case "1": // Design
                    HullNo = "D" + HullNo;
                    break;
                case "2": // Vessel - New Building
                    HullNo = "H" + HullNo;
                    break;
                case "3": // Vessel - Existience
                    HullNo = "E" + HullNo;
                    break;
                case "4": // Facility
                    // 뒤 3자리는 전체 FA의 일련번호
                    HullNo = "FA" + int.Parse(getCountJobId(JobCategory).ToString()).ToString("000");
                    break;
                case "5": // Equipment
                    // 뒤 3자리는 전체 EQ의 일련번호
                    HullNo = "EQ" + int.Parse(getCountJobId(JobCategory).ToString()).ToString("000");
                    break;
                case "6": // Engineering
                    // 뒤 3자리는 전체 EN의 일련번호
                    HullNo = "EN" + int.Parse(getCountJobId(JobCategory).ToString()).ToString("000");
                    break;
            }


            switch (findCommCode(Client))
            {
                case "GovernmentVessel": // 관공선
                    _client = "GV";
                    break;
                case "CoastGuard": // 해경
                    _client = "NP";
                    break;
                case "Navy":
                    _client = "NS"; // 해군
                    break;
                case "Army":
                    _client = "AR"; // 육군
                    break;
                case "AirForce": // 공군
                    _client = "AF";
                    break;
            }


            return findShipBuilderId(ShipBuilder) + HullNo + "-" + _client;
        }

        private string findCommCode(Guid Idx)
        {
            return _repository.TNAV_COMMON_CODEs.Where(m => m.IDX.Equals(Idx)).Select(m => m.CODE).Single();
        }

        private async Task<int> getCountJobId(string JobCategory)
        {
            OutputParameter<int?> MaxVal = new OutputParameter<int?>();
            await _procedures.PNAV_PMS_GET_MAX_JOB_IDAsync(int.Parse(JobCategory), RESULT: MaxVal);

            return MaxVal.Value ?? 0;
        }

        /// <summary>
        /// Job ID를 생성하기 위한 ShipBuilder ID를 조회
        /// </summary>
        /// <param name="_CliId"></param>
        /// <returns></returns>
        private string findShipBuilderId(string _CliId)
        {
            //return _rfSystemContext.TPRO_JOB_ID_MASTERs
            //    .Where(m => m.Jejo_Code == _CliId)
            //    .Select(m => m.ShipBuilderID).Single();

            return _rfSystem.GetRfvYardMaker().Where(m => m.Jejo_Code == _CliId).Select(m => m.ShipBuilderID).Single();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(TNAV_JOB_MANAGEMENT _tNAV_JOB_MANAGEMENT)
        {
            string result = String.Empty;

            try
            {
                var tNAV_JOB_MANAGEMENT = await _repository.TNAV_JOB_MANAGEMENTs.FindAsync(_tNAV_JOB_MANAGEMENT.JOB_IDX);

                if (tNAV_JOB_MANAGEMENT != null)
                {
                    // 상태값 저장
                    TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                    {
                        REG_DATE = DateTime.Now,
                        USER_NAME = HttpContext.Session.GetString("UserName"),
                        PLATFORM = "PMS",
                        MENU_NAME = "JOB_ID",
                        TARGET_IDX = tNAV_JOB_MANAGEMENT.JOB_IDX,
                        STATUS = CommonSettingData.LogStatus.DELETE.ToString()
                    };
                    _repository.Add(LogViewModel);

                    _repository.TNAV_JOB_MANAGEMENTs.Remove(tNAV_JOB_MANAGEMENT);
                    await _repository.SaveChangesAsync();
                    result = "OK";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }

        public async Task<IActionResult> Detail(Guid? id)
        {
            if (id == null || _repository.TNAV_JOB_MANAGEMENTs == null)
            {
                return NotFound();
            }

            var tNAV_JOB_MANAGEMENT = await _repository.TNAV_JOB_MANAGEMENTs
                                            .Include(m => m.NSN_IDXNavigation)
                                            .FirstOrDefaultAsync(m => m.JOB_IDX == id);

            if (tNAV_JOB_MANAGEMENT == null)
            {
                return NotFound();
            }
            else
            {
                ViewBag.ClientName = _repository.TNAV_COMMON_CODEs.Where(m => m.IDX == tNAV_JOB_MANAGEMENT.CLIENT).Select(m => m.CODE_NAME).First();
                ViewBag.VesselType = _repository.TNAV_COMMON_CODEs.Where(m => m.IDX == tNAV_JOB_MANAGEMENT.VESSEL_TYPE).Select(m => m.CODE_NAME).First();

                ViewBag.DataSourece = _repository.TNAV_NSN_MANAGEMENTs.AsNoTracking().ToList().OrderByDescending(m => m.NSN_ID);
                ViewBag.sort = "Ascending";
            }

            return View(tNAV_JOB_MANAGEMENT);
        }
    }
}
