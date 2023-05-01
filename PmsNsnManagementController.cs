using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.ViewModels;
using NavesPortalforWebWithCoreMvc.Common;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NavesPortalforWebWithCoreMvc.RfSystemModels;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using Syncfusion.EJ2.Base;
using System.Collections;
//using static NavesPortalforWebWithCoreMvc.ViewModels.ShowLogViewModel;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsNsnManagementController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _systemContext;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;
        private readonly INavesPortalCommonService _common;
        private readonly IRfSystemCommonService _rfSystem;

        public PmsNsnManagementController(BM_NAVES_PortalContext db,
            IBM_NAVES_PortalContextProcedures procedures,
            INavesPortalCommonService common,
            IRfSystemCommonService rfSystem,
            RfSystemContext systemContext)
        {
            _repository = db;
            _procedures = procedures;
            _common = common;
            _rfSystem = rfSystem;
            _systemContext = systemContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.StartDate = new DateTime(1984, 1, 1);
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

                List<PNAV_PMS_GET_NSN_LISTResult> resultList = await _procedures.PNAV_PMS_GET_NSN_LISTAsync(SearchString.Trim(), StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_PMS_GET_NSN_LISTResult>().Count();

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

        /// <summary>
        /// NSN 등록 화면
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            // 페이지 초기화
            NsnPageInitial();
            return View();
        }

        /// <summary>
        /// Create, Detail 페이지 초기화
        /// </summary>
        private void NsnPageInitial()
        {
            // Cascading Category
            ViewBag.FirstCate = _repository.VNAV_SELECT_PMS_VESSEL_CATEGORY_LISTs.Where(m => m.LEVEL == 1).OrderBy(m => m.CODE);
            ViewBag.SecondCate = _repository.VNAV_SELECT_PMS_VESSEL_CATEGORY_LISTs.Where(m => m.LEVEL == 2).OrderBy(m => m.CODE);
            ViewBag.ThirdCate = _repository.VNAV_SELECT_PMS_VESSEL_CATEGORY_LISTs.Where(m => m.LEVEL == 3).OrderBy(m => m.CODE);
            ViewBag.ForthCate = _repository.VNAV_SELECT_PMS_VESSEL_CATEGORY_LISTs.Where(m => m.LEVEL == 4).OrderBy(m => m.CODE);

            // DropdownList Height
            ViewBag.popupHeight = "auto";

            // Yard/Maker
            ViewBag.YardMaker = _rfSystem.GetRfvYardMaker()
                                .Where(m => m.CLINAME != null)
                                .OrderBy(m => m.CLINAME);
        }

        /// <summary>
        /// 공통 코드를 사용하는 DropdownList 바인딩
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult CommonDdlValue(string Id)
        {
            var _client = _common.GetCommonCodeList(Id);

            List<dropdownViewModel> ddlVm = new List<dropdownViewModel>();

            foreach (TNAV_COMMON_CODE item in _client)
            {
                ddlVm.Add(new dropdownViewModel
                {
                    Name = item.CODE_NAME,
                    Value = item.IDX.ToString(),
                    Castcading = item.CASCADING_IDX.ToString()
                });
            }

            return Json(ddlVm);
        }

        public IActionResult RfSystemCommonDdlValue()
        {
            var _client = _rfSystem.GetCrmBasicInfo();

            List<dropdownViewModel> ddlVm = new List<dropdownViewModel>();

            foreach (RFV_CRM_BASIC_INFO item in _client)
            {
                ddlVm.Add(new dropdownViewModel
                {
                    Name = item.CLINAME,
                    Value = item.CLIID
                });
            }

            return Json(ddlVm);
        }

        /// <summary>
        /// NSN ID 생성
        /// </summary>
        /// <param name="tNAV_NSN_MANAGEMENT"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TNAV_NSN_MANAGEMENT tNAV_NSN_MANAGEMENT)
        {
            try
            {
                tNAV_NSN_MANAGEMENT.NSN_IDX = Guid.NewGuid();
                tNAV_NSN_MANAGEMENT.NSN_ID = await CreateNsnId(tNAV_NSN_MANAGEMENT.CONTRACT_DATE);
                tNAV_NSN_MANAGEMENT.CREATE_USER_NAME = HttpContext.Session.GetString("UserName");
                tNAV_NSN_MANAGEMENT.REG_DATE = DateTime.Now;

                //if (ModelState.IsValid)
                //{

                // 상태값 저장
                TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                {
                    REG_DATE = DateTime.Now,
                    USER_NAME = HttpContext.Session.GetString("UserName"),
                    PLATFORM = "PMS",
                    MENU_NAME = "NSN_ID",
                    TARGET_IDX = tNAV_NSN_MANAGEMENT.NSN_IDX,
                    STATUS = CommonSettingData.LogStatus.CREATE.ToString()
                };

                _repository.Add(LogViewModel);
                // 상태값 저장

                _repository.Add(tNAV_NSN_MANAGEMENT);
                await _repository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                //}
                //else
                //{
                //    return View("Create", tNAV_NSN_MANAGEMENT);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid NSN_IDX)
        {
            string result = String.Empty;

            try
            {
                var tNAV_NSN_MANAGEMENT = await _repository.TNAV_NSN_MANAGEMENTs.FindAsync(NSN_IDX);

                if (tNAV_NSN_MANAGEMENT != null)
                {
                    int jobCouont = _repository.TNAV_JOB_MANAGEMENTs.Where(m => m.NSN_IDX == NSN_IDX).ToList().Count();
                    if (jobCouont != 0)
                    {
                        var tNAV_JOB_MANAGEMENTs = _repository.TNAV_JOB_MANAGEMENTs.Where(m => m.NSN_IDX == NSN_IDX).ToList();
                        _repository.TNAV_JOB_MANAGEMENTs.RemoveRange(tNAV_JOB_MANAGEMENTs);
                    }

                    // 상태값 저장
                    TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                    {
                        REG_DATE = DateTime.Now,
                        USER_NAME = HttpContext.Session.GetString("UserName"),
                        PLATFORM = "PMS",
                        MENU_NAME = "NSN_ID",
                        TARGET_IDX = tNAV_NSN_MANAGEMENT.NSN_IDX,
                        STATUS = CommonSettingData.LogStatus.DELETE.ToString()
                    };
                    _repository.Add(LogViewModel);

                    _repository.TNAV_NSN_MANAGEMENTs.Remove(tNAV_NSN_MANAGEMENT);
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

        /// <summary>
        /// 수정 저장
        /// </summary>
        /// <param name="_NsnViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TNAV_NSN_MANAGEMENT _NsnViewModel)
        {
            string result = String.Empty;
            TNAV_NSN_MANAGEMENT Model = new TNAV_NSN_MANAGEMENT();

            if (ModelState.IsValid)
            {
                try
                {
                    TNAV_NSN_MANAGEMENT tNAV_NSN_MANAGEMENT = _repository.TNAV_NSN_MANAGEMENTs.AsNoTracking().Where(m => m.NSN_IDX == _NsnViewModel.NSN_IDX).FirstOrDefault();
                    Model = new TNAV_NSN_MANAGEMENT()
                    {
                        NSN_IDX = _NsnViewModel.NSN_IDX,
                        NSN_ID = _NsnViewModel.NSN_ID,
                        CLIENT = _NsnViewModel.CLIENT,
                        VESSEL_TYPE = _NsnViewModel.VESSEL_TYPE,
                        VESSEL_CATEGORY_1 = _NsnViewModel.VESSEL_CATEGORY_1,
                        VESSEL_CATEGORY_2 = _NsnViewModel.VESSEL_CATEGORY_2,
                        VESSEL_CATEGORY_3 = _NsnViewModel.VESSEL_CATEGORY_3,
                        VESSEL_CATEGORY_4 = _NsnViewModel.VESSEL_CATEGORY_4,
                        VESSEL_NAME_KR = _NsnViewModel.VESSEL_NAME_KR,
                        VESSEL_NAME_EN = _NsnViewModel.VESSEL_NAME_EN,
                        HULL_NO = _NsnViewModel.HULL_NO,
                        SHIP_NO = _NsnViewModel.SHIP_NO,
                        YARD_MAKER = _NsnViewModel.YARD_MAKER,
                        YARD_MAKER_NAME = _NsnViewModel.YARD_MAKER_NAME,
                        FULL_LOAD_DISPLACEMENT = _NsnViewModel.FULL_LOAD_DISPLACEMENT,
                        DISPLACEMENT = _NsnViewModel.DISPLACEMENT,
                        LIGHT = _NsnViewModel.LIGHT,
                        BREADTH = _NsnViewModel.BREADTH,
                        DEPTH = _NsnViewModel.DEPTH,
                        OVERALL_LENGTH = _NsnViewModel.OVERALL_LENGTH,
                        DRAFT = _NsnViewModel.DRAFT,
                        HULL_MATERIAL = _NsnViewModel.HULL_MATERIAL,
                        SPEED = _NsnViewModel.SPEED,
                        REMARK = _NsnViewModel.REMARK,
                        DELIVERY_DATE = Convert.ToDateTime(_NsnViewModel.DELIVERY_DATE),
                        REG_DATE = tNAV_NSN_MANAGEMENT.REG_DATE,
                        CREATE_USER_NAME = _NsnViewModel.CREATE_USER_NAME,
                        CONTRACT_DATE = Convert.ToDateTime(_NsnViewModel.CONTRACT_DATE),
                        MODIFY_DATE = DateTime.Now,
                        MODIFY_USER_NAME = HttpContext.Session.GetString("UserName"),
                        IS_DELETED = tNAV_NSN_MANAGEMENT.IS_DELETED
                    };

                    // 상태값 저장
                    TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                    {
                        REG_DATE = DateTime.Now,
                        USER_NAME = HttpContext.Session.GetString("UserName"),
                        PLATFORM = "PMS",
                        MENU_NAME = "NSN_ID",
                        TARGET_IDX = _NsnViewModel.NSN_IDX,
                        STATUS = CommonSettingData.LogStatus.MODIFY.ToString()
                    };
                    _repository.Add(LogViewModel);

                    _repository.Update(Model);
                    await _repository.SaveChangesAsync();
                    result = "OK";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TNAV_NSN_MANAGEMENTExists(Model.NSN_IDX))
                    {
                        result = "대상이 삭제되었거나 존재하지 않습니다. \n다시한번 확인해 주세요";
                    }
                    else
                    {
                        result = "오류가 발생했습니다. 관리자에게 문의하세요";
                    }
                }

                catch (Exception EX)
                {
                    result = EX.InnerException.Message;
                }
            }

            return Json(result);
        }

        /// <summary>
        /// 상세 페이지
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(Guid? id)
        {
            if (id == null || _repository.TNAV_NSN_MANAGEMENTs == null)
            {
                return NotFound();
            }

            // 페이지 초기화
            NsnPageInitial();

            // NSN 상세 내용 불러오기
            var tNAV_NSN_MANAGEMENT = await _repository.TNAV_NSN_MANAGEMENTs.FirstOrDefaultAsync(m => m.NSN_IDX == id);

            return View(tNAV_NSN_MANAGEMENT);
        }

        /// <summary>
        /// NSN에 할당되어 있는 Job ID List 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetJobIdListInNsn(Guid? id)
        {
            if (id == null || _repository.TNAV_JOB_MANAGEMENTs == null)
            {
                return NotFound();
            }

            var tNAV_JOB_MANAGEMENTs = _repository.TNAV_JOB_MANAGEMENTs
                                        .Where(m => m.NSN_IDX == id)
                                        .OrderBy(m => m.REG_DATE)
                                        .AsEnumerable()
                                        .Select((m, i) => new { no = i + 1, job_id = m.JOB_ID, job_idx = m.JOB_IDX })
                                        .ToList()
                                        .OrderByDescending(m => m.no);
            if (tNAV_JOB_MANAGEMENTs == null)
            {
                return NotFound();
            }

            return Json(tNAV_JOB_MANAGEMENTs);

        }

        /// <summary>
        /// Delivery Date yyyy를 기준으로 순번을 부여함.
        /// </summary>
        /// <param name="deliveryDate"></param>
        /// <returns></returns>
        public async Task<string> CreateNsnId(DateTime contractDate)
        {
            string result = string.Empty;

            string _year = contractDate.ToString("yyyy");
            string _code = "NVS";

            OutputParameter<int?> MaxVal = new OutputParameter<int?>();
            await _procedures.PNAV_PMS_GET_MAX_NSN_IDAsync(_year, RESULT: MaxVal);

            result = $"{_year}{_code}{(MaxVal.Value ?? 0).ToString("D2")}";

            return result;
        }

        private bool TNAV_NSN_MANAGEMENTExists(Guid id)
        {
            return (_repository.TNAV_NSN_MANAGEMENTs?.Any(e => e.NSN_IDX == id)).GetValueOrDefault();
        }
    }
}
