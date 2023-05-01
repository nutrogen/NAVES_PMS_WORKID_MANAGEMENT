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
using System.Configuration;
using Syncfusion.EJ2.Inputs;
using Syncfusion.EJ2.Base;
using System.Collections;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsProjectIdManagementController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="rfSystemRepository"></param>
        public PmsProjectIdManagementController(BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = repository;
            _rfSystemRepository = rfSystemRepository;
            _procedures = procedures;
        }

        /// <summary>
        /// 목록 화면 반환
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View());
        }

        /// <summary>
        /// PRoject Id List
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>json</returns>
        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null)
                {
                    SearchString = "";
                }

                List<PNAV_PMS_GET_PROJECT_LISTResult> resultList = await _procedures.PNAV_PMS_GET_PROJECT_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_PMS_GET_PROJECT_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "PmsProjectIdManagement", returnView = "Index" });
            }
        }

        /// <summary>
        /// New 화면 반환
        /// </summary>
        /// <returns></returns>
        public IActionResult Create(ProjectViewModel? vProjectViewModel)
        {
            ViewBag.UserList = _rfSystemRepository.RFV_USER_DEPTs.ToList();

            // 플랫폼 종류
            ViewBag.ProjectType = CommonSettingData.GetPlatformType();

            // 화폐 단위 
            ViewBag.CurrnecyType = CommonSettingData.GetCurrencyType();

            // Create new IDX for Project IDX
            ViewBag.CurrentIdx = Guid.NewGuid();

            // History Idx
            ViewBag.ReleatedIdx = Guid.NewGuid();

            // User Name
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        /// <summary>
        /// Project ID 생성
        /// </summary>\\
        /// <param name="vProjectViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(ProjectViewModel vProjectViewModel)
        {
            try
            {
                if (vProjectViewModel is not null)
                {
                    string _ProjectId = await CreateProjectId(vProjectViewModel.CONTRACT_DATE);

                    List<TNAV_PROJECT_PIC> tNAV_PROJECT_PIC = new List<TNAV_PROJECT_PIC>();

                    TNAV_PROJECT tNAV_PROJECT = new TNAV_PROJECT()
                    {
                        PROJECT_IDX = vProjectViewModel.PROJECT_IDX,
                        PROJECT_ID = _ProjectId,
                        PROJECT_TITLE = vProjectViewModel.PROJECT_TITLE ?? "",
                        CONTRACT_DATE = vProjectViewModel.CONTRACT_DATE,
                        CONTRACT_NO = vProjectViewModel.CONTRACT_NO ?? "",
                        TERM_OF_PROJECT_START_DATE = vProjectViewModel.TERM_OF_PROJECT_START_DATE,
                        TERM_OF_PROJECT_END_DATE = vProjectViewModel.TERM_OF_PROJECT_END_DATE,
                        CURRENCY_SYMBOL = vProjectViewModel.CURRENCY_SYMPOL ?? "",
                        PROJECT_TOTAL_AMOUNT = vProjectViewModel.PROJECT_TOTAL_AMOUNT,
                        INVOICE_INITIAL_DATE = vProjectViewModel.INVOICE_INITIAL_DATE,
                        INVOICE_INITIAL_AMOUNT = vProjectViewModel.INVOICE_INITIAL_AMOUNT,
                        INVOICE_INTER_DATE = vProjectViewModel.INVOICE_INTER_DATE,
                        INVOICE_INTER_AMOUNT = vProjectViewModel.INVOICE_INTER_AMOUNT,
                        INVOICE_FINAL_DATE = vProjectViewModel.INVOICE_FINAL_DATE,
                        INVOICE_FINAL_AMOUNT = vProjectViewModel.INVOICE_FINAL_AMOUNT,
                        INVOICE_IS_INITIAL = vProjectViewModel.INVOICE_IS_INITIAL,
                        INVOICE_IS_INTERMEDIATE = vProjectViewModel.INVOICE_IS_INTERMEDIATE,
                        INVOICE_IS_FINAL = vProjectViewModel.INVOICE_IS_FINAL,
                        QUANTITY = vProjectViewModel.QUANTITY,
                        WORK_ID_QUANTITY = vProjectViewModel.WORK_ID_QUANTITY,
                        PROJECT_TYPE = vProjectViewModel.PROJECT_TYPE ?? "",
                        CLIENT_CODE = vProjectViewModel.CLIENT_CODE ?? "",
                        CLIENT_NAME = vProjectViewModel.CLIENT_NAME ?? "",
                        CLIENT_SUPERVISOR_NAME = vProjectViewModel.CLIENT_SUPERVISOR_NAME ?? "",
                        CLIENT_SUPERVISOR_MOBILE = vProjectViewModel.CLIENT_SUPERVISOR_MOBILE ?? "",
                        CLIENT_SUPERVISOR_EMAIL = vProjectViewModel.CLIENT_SUPERVISOR_EMAIL ?? "",
                        CLIENT_CONTRACTOR_NAME = vProjectViewModel.CLIENT_CONTRACTOR_NAME ?? "",
                        CLIENT_CONTRACTOR_MOBILE = vProjectViewModel.CLIENT_CONTRACTOR_MOBILE ?? "",
                        CLIENT_CONTRACTOR_EMAIL = vProjectViewModel.CLIENT_CONTRACTOR_EMAIL ?? "",
                        REG_DATE = DateTime.Now,
                        CREATE_USER_NAME = HttpContext.Session.GetString("UserName") ?? "",
                        CONFIRM_STATUS = vProjectViewModel.CONFIRM_STATUS ?? "",
                        CONFIRM_DATE = null,
                        CONFIRM_USER_NAME = String.Empty,
                        REJECT_COMMENT = null ?? "",
                        REJECT_DATE = null,
                        STATUS = CommonSettingData.ProjectID_Status.DRAFT.ToString(),
                        PROJECT_PROGRESS = "0"
                    };

                    // 기존 정보를 TNAV_PROJECT_HISTORY에 저장하여 이력으로 관리
                    TNAV_PROJECT_HISTORY _PROJECT_HISTORY = new TNAV_PROJECT_HISTORY()
                    {
                        IDX = vProjectViewModel.RELEATED_IDX.HasValue ? vProjectViewModel.RELEATED_IDX.Value : Guid.NewGuid(),
                        PROJECT_IDX = vProjectViewModel.PROJECT_IDX,
                        PROJECT_TITLE = vProjectViewModel.PROJECT_TITLE ?? "",
                        CONTRACT_DATE = vProjectViewModel.CONTRACT_DATE,
                        CONTRACT_NO = vProjectViewModel.CONTRACT_NO ?? "",
                        MODIFY_CONTRACT_COUNT = 0,
                        TERM_OF_PROJECT_START_DATE = vProjectViewModel.TERM_OF_PROJECT_START_DATE,
                        TERM_OF_PROJECT_END_DATE = vProjectViewModel.TERM_OF_PROJECT_END_DATE,
                        CURRENCY_SYMBOL = vProjectViewModel.CURRENCY_SYMPOL ?? "",
                        PROJECT_TOTAL_AMOUNT = vProjectViewModel.PROJECT_TOTAL_AMOUNT,
                        INVOICE_INITIAL_DATE = vProjectViewModel.INVOICE_INITIAL_DATE,
                        INVOICE_INITIAL_AMOUNT = vProjectViewModel.INVOICE_INITIAL_AMOUNT ?? 0,
                        INVOICE_INTER_DATE = vProjectViewModel.INVOICE_INTER_DATE,
                        INVOICE_INTER_AMOUNT = vProjectViewModel.INVOICE_INTER_AMOUNT ?? 0,
                        INVOICE_FINAL_DATE = vProjectViewModel.INVOICE_FINAL_DATE,
                        INVOICE_FINAL_AMOUNT = vProjectViewModel.INVOICE_FINAL_AMOUNT ?? 0,
                        INVOICE_IS_INITIAL = vProjectViewModel.INVOICE_IS_INITIAL,
                        INVOICE_IS_INTERMEDIATE = vProjectViewModel.INVOICE_IS_INTERMEDIATE,
                        INVOICE_IS_FINAL = vProjectViewModel.INVOICE_IS_FINAL,
                        CONTRACT_QUANTITY = vProjectViewModel.QUANTITY,
                        REG_DATE = DateTime.Now,
                        CREATE_USER_NAME = HttpContext.Session.GetString("UserName")
                    };
                    _repository.Add(_PROJECT_HISTORY);

                    // 할당된 PM 목록
                    if (vProjectViewModel.PROJECT_PM is not null)
                    {
                        foreach (string pm in vProjectViewModel.PROJECT_PM)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PmUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pm).First();

                            TNAV_PROJECT_PIC PM = new TNAV_PROJECT_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                PROJECT_IDX = vProjectViewModel.PROJECT_IDX,
                                PROJECT_ID = _ProjectId,
                                PROJECT_POSTION = "PM",
                                USER_ID = PmUser.USER_ID,
                                USER_NAME_KR = PmUser.USER_NAME,
                                USRE_NAME_EN = PmUser.USER_NAME_E,
                                DEPT_ID = PmUser.DEPT_ID,
                                DEPT_NAME_KR = PmUser.DEPT_NAME,
                                DEPT_NAME_EN = PmUser.DEPT_NAME_E,
                                DEGREE_KR = PmUser.DEGREE,
                                DEGREE_EN = PmUser.DEGREE_E,
                                EMP_ID = PmUser.EMP_ID,
                                SUR_NO = PmUser.SUR_NO,
                                POSITION_KR = PmUser.POSITION_K,
                                POSITION_EN = PmUser.POSITION_E,
                                REG_DATE = DateTime.Now
                            };

                            tNAV_PROJECT_PIC.Add(PM);
                        }
                    }

                    // 할당된 PIC 목록
                    if (vProjectViewModel.PROJECT_PIC is not null)
                    {
                        foreach (string pic in vProjectViewModel.PROJECT_PIC)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PicUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pic).First();

                            TNAV_PROJECT_PIC PIC = new TNAV_PROJECT_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                PROJECT_IDX = vProjectViewModel.PROJECT_IDX,
                                PROJECT_ID = _ProjectId,
                                PROJECT_POSTION = "PIC",
                                USER_ID = PicUser.USER_ID,
                                USER_NAME_KR = PicUser.USER_NAME,
                                USRE_NAME_EN = PicUser.USER_NAME_E,
                                DEPT_ID = PicUser.DEPT_ID,
                                DEPT_NAME_KR = PicUser.DEPT_NAME,
                                DEPT_NAME_EN = PicUser.DEPT_NAME_E,
                                DEGREE_KR = PicUser.DEGREE,
                                DEGREE_EN = PicUser.DEGREE_E,
                                EMP_ID = PicUser.EMP_ID,
                                SUR_NO = PicUser.SUR_NO,
                                POSITION_KR = PicUser.POSITION_K,
                                POSITION_EN = PicUser.POSITION_E,
                                REG_DATE = DateTime.Now
                            };

                            tNAV_PROJECT_PIC.Add(PIC);
                        }
                    }

                    ModelState.Clear();
                    ModelState.ClearValidationState(nameof(tNAV_PROJECT));
                    if (!TryValidateModel(tNAV_PROJECT, nameof(tNAV_PROJECT)))
                    {
                        return View(nameof(Create), vProjectViewModel);
                    }

                    if (ModelState.IsValid)
                    {
                        // 상태값 저장
                        TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                        {
                            REG_DATE = DateTime.Now,
                            USER_NAME = HttpContext.Session.GetString("UserName"),
                            PLATFORM = "PMS",
                            MENU_NAME = "PROJECT ID",
                            TARGET_IDX = vProjectViewModel.PROJECT_IDX,
                            STATUS = CommonSettingData.LogStatus.CREATE.ToString()
                        };
                        _repository.Add(LogViewModel);

                        _repository.Add(tNAV_PROJECT);
                        _repository.AddRange(tNAV_PROJECT_PIC);
                        await _repository.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Detail), new { id = vProjectViewModel.PROJECT_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Project ID 생성
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateProjectId(DateTime ContractDate)
        {
            string result = string.Empty;
            string _year = ContractDate.ToString("yyyy");

            OutputParameter<int?> MaxVal = new OutputParameter<int?>();
            await _procedures.PNAV_PMS_GET_MAX_PROJECT_IDAsync(_year, RESULT: MaxVal);

            string _sn = (MaxVal.Value ?? 0).ToString("0000");
            result = ContractDate.ToString("yy") + "-" + _sn;

            return result;
        }

        /// <summary>
        /// PM, PIC 선택
        /// </summary>
        /// <param name="USER_NAME_E"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SelectUser(string USER_NAME_E)
        {
            try
            {
                if (!string.IsNullOrEmpty(USER_NAME_E) && USER_NAME_E.Length > 2)
                {
                    // 2022-11-24
                    //SUR NO가 없는 사람도 검색, 할당 가능할 수 있도록 변경
                    var result = await _rfSystemRepository.RFV_USER_DEPTs
                        .Where(m => m.USER_NAME_E == USER_NAME_E)
                        .FirstOrDefaultAsync();
                    return Json(result);
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception)
            {
                throw;
                //return Json(string.Empty);
            }

        }

        public IActionResult Delete()
        {
            return View();
        }

        /// <summary>
        /// 상세 화면, 수정 화면 반환
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Detail(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                ViewBag.AmountFormat = "###,###,###,###";
                ViewBag.ProjectType = CommonSettingData.GetPlatformType();
                ViewBag.CurrnecyType = CommonSettingData.GetCurrencyType();
                ViewBag.UserList = _rfSystemRepository.RFV_USER_DEPTs.ToList();
                ViewBag.CurrentIdx = id;
                // History Idx
                ViewBag.ReleatedIdx = Guid.NewGuid();

                // 첨부파일 목록
                ViewBag.FileList = await _repository.TNAV_COM_FILEs.Where(m => m.DOCUMENT_IDX == id && m.KIND_OF_FILES == "ContractDocument").ToListAsync();

                // Project ID 상세 정보
                TNAV_PROJECT tNAV_PROJECT = await _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == id).FirstAsync();

                // PM으로 할당된 PM 사용자 정보
                List<TNAV_PROJECT_PIC> tNAV_PROJECT_PM = _repository.TNAV_PROJECT_PICs.Where(m => m.PROJECT_IDX == id && m.PROJECT_POSTION == "PM").ToList();

                // PIC로 할당된 PIC 사용자 정보
                List<TNAV_PROJECT_PIC> tNAV_PROJECT_PIC = _repository.TNAV_PROJECT_PICs.Where(m => m.PROJECT_IDX == id && m.PROJECT_POSTION == "PIC").ToList();

                ViewBag.Pm = tNAV_PROJECT_PM;
                ViewBag.Pic = tNAV_PROJECT_PIC;
                ViewBag.ProjectId = tNAV_PROJECT.PROJECT_ID;

                ProjectViewModel projectViewModel = new ProjectViewModel()
                {
                    PROJECT_IDX = tNAV_PROJECT.PROJECT_IDX,
                    PROJECT_ID = tNAV_PROJECT.PROJECT_ID,
                    PROJECT_TITLE = tNAV_PROJECT.PROJECT_TITLE,
                    CONTRACT_DATE = tNAV_PROJECT.CONTRACT_DATE,
                    CONTRACT_NO = tNAV_PROJECT.CONTRACT_NO,
                    TERM_OF_PROJECT_START_DATE = tNAV_PROJECT.TERM_OF_PROJECT_START_DATE,
                    TERM_OF_PROJECT_END_DATE = tNAV_PROJECT.TERM_OF_PROJECT_END_DATE,
                    CURRENCY_SYMPOL = tNAV_PROJECT.CURRENCY_SYMBOL,
                    PROJECT_TOTAL_AMOUNT = tNAV_PROJECT.PROJECT_TOTAL_AMOUNT ?? 0,
                    INVOICE_INITIAL_DATE = tNAV_PROJECT.INVOICE_INITIAL_DATE,
                    INVOICE_INITIAL_AMOUNT = tNAV_PROJECT.INVOICE_INITIAL_AMOUNT ?? 0,
                    INVOICE_INTER_DATE = tNAV_PROJECT.INVOICE_INTER_DATE,
                    INVOICE_INTER_AMOUNT = tNAV_PROJECT.INVOICE_INTER_AMOUNT ?? 0,
                    INVOICE_FINAL_DATE = tNAV_PROJECT.INVOICE_FINAL_DATE,
                    INVOICE_FINAL_AMOUNT = tNAV_PROJECT.INVOICE_FINAL_AMOUNT ?? 0,
                    INVOICE_IS_INITIAL = tNAV_PROJECT.INVOICE_IS_INITIAL,
                    INVOICE_IS_INTERMEDIATE = tNAV_PROJECT.INVOICE_IS_INTERMEDIATE,
                    INVOICE_IS_FINAL = tNAV_PROJECT.INVOICE_IS_FINAL,
                    IS_MODIFY_CONTRACT = tNAV_PROJECT.IS_MODIFY_CONTRACT,
                    QUANTITY = tNAV_PROJECT.QUANTITY,
                    WORK_ID_QUANTITY = tNAV_PROJECT.WORK_ID_QUANTITY,
                    PROJECT_TYPE = tNAV_PROJECT.PROJECT_TYPE,
                    CLIENT_CODE = tNAV_PROJECT.CLIENT_CODE,
                    CLIENT_NAME = tNAV_PROJECT.CLIENT_NAME,
                    CLIENT_SUPERVISOR_NAME = tNAV_PROJECT.CLIENT_SUPERVISOR_NAME,
                    CLIENT_SUPERVISOR_MOBILE = tNAV_PROJECT.CLIENT_SUPERVISOR_MOBILE,
                    CLIENT_SUPERVISOR_EMAIL = tNAV_PROJECT.CLIENT_SUPERVISOR_EMAIL,
                    CLIENT_CONTRACTOR_NAME = tNAV_PROJECT.CLIENT_CONTRACTOR_NAME,
                    CLIENT_CONTRACTOR_MOBILE = tNAV_PROJECT.CLIENT_CONTRACTOR_MOBILE,
                    CLIENT_CONTRACTOR_EMAIL = tNAV_PROJECT.CLIENT_CONTRACTOR_EMAIL,
                    CREATE_USER_NAME = tNAV_PROJECT.CREATE_USER_NAME,
                    REG_DATE = tNAV_PROJECT.REG_DATE,
                    CONFIRM_STATUS = tNAV_PROJECT.CONFIRM_STATUS,
                    CONFIRM_DATE = tNAV_PROJECT.CONFIRM_DATE,
                    CONFIRM_USER_NAME = tNAV_PROJECT.CONFIRM_USER_NAME,
                    REJECT_COMMENT = tNAV_PROJECT.REJECT_COMMENT,
                    REJECT_DATE = tNAV_PROJECT.REJECT_DATE,
                    REJECT_USER_NAME = tNAV_PROJECT.REJECT_USER_NAME,
                    STATUS = tNAV_PROJECT.STATUS,
                    RELEATED_IDX = _repository.TNAV_PROJECT_HISTORies.Where(m => m.PROJECT_IDX == id).OrderByDescending(m => m.MODIFY_CONTRACT_COUNT).Select(m => m.IDX).FirstOrDefault()
                };

                // 로그인 사용자와 할당된 PM이 같으면
                foreach (TNAV_PROJECT_PIC pm in tNAV_PROJECT_PM)
                {
                    if (pm.DEPT_NAME_EN == HttpContext.Session.GetString("UserName"))
                    {
                        ViewBag.IsPM = 1; // true
                        break;
                    }
                    else
                    {
                        ViewBag.IsPM = 0; // false
                    }
                }

                // Work Scope
                ViewBag.WorkScope = await _repository.VNAV_SELECT_PMS_WORK_SCOPE_LISTs.Where(m => m.PROJECT_IDX == id).ToListAsync();

                int worikIdCount = _repository.TNAV_WORKs.Where(m => m.PROJECT_IDX == id).Count();

                // Work ID Count 
                ViewBag.WorkIdCount = worikIdCount;

                // int wrkQuantity = 0;
                if (worikIdCount > projectViewModel.WORK_ID_QUANTITY)
                {
                    ViewBag.Msg = "생성된 WORK ID가 Work ID 계약 수량보다 많습니다.";
                    ViewBag.IsShowModal = true;
                }
                else if (worikIdCount < projectViewModel.WORK_ID_QUANTITY)
                {
                    ViewBag.Msg = "생성된 WORK ID 가 Work ID 계약 수량보다 작습니다. Work ID를 생성하세요...";
                    ViewBag.IsShowModal = true;
                }
                else
                {
                    ViewBag.IsShowModal = false;
                }

                // 마지막 변경 이력 횟수
                int maxContract_count = _repository.TNAV_PROJECT_HISTORies
                    .Where(m => m.PROJECT_IDX == id)
                    .OrderByDescending(m => m.MODIFY_CONTRACT_COUNT)
                    .Select(m => m.MODIFY_CONTRACT_COUNT)
                    .FirstOrDefault();

                // Project ID 변경 계약 
                List<TNAV_PROJECT_HISTORY> historyQuery = _repository.TNAV_PROJECT_HISTORies
                    .Where(m => m.PROJECT_IDX == id && m.MODIFY_CONTRACT_COUNT != maxContract_count)
                    .OrderByDescending(m => m.MODIFY_CONTRACT_COUNT)
                    .ToList();

                List<ContractHistoryViewModel> ContractList = new List<ContractHistoryViewModel>();
                foreach (TNAV_PROJECT_HISTORY projectHistory in historyQuery)
                {
                    ContractHistoryViewModel history = new ContractHistoryViewModel()
                    {
                        PROJECT_HISTORY = projectHistory,
                        // 첨부 파일
                        FILES = _repository.TNAV_COM_FILEs
                                .Where(m => m.DOCUMENT_IDX.Equals(projectHistory.PROJECT_IDX) &&
                                       m.KIND_OF_FILES == "ModifyContractDocument" &&
                                       m.RELATED_INFO == projectHistory.IDX.ToString().ToUpper()).ToList()
                    };

                    ContractList.Add(history);
                }

                // Project ID 변경 계약 
                int HistoryCount = _repository.TNAV_PROJECT_HISTORies
                    .Where(m => m.PROJECT_IDX == id && m.MODIFY_CONTRACT_COUNT != 0)
                    .OrderByDescending(m => m.MODIFY_CONTRACT_COUNT)
                    .Count();

                ViewBag.ProjectHistoryList = ContractList;
                ViewBag.ProjectHistoryCount = HistoryCount;

                return View(projectViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 수정
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projectViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectViewModel projectViewModel)
        {
            ModelState.Clear();
            ModelState.ClearValidationState(nameof(projectViewModel));

            if (ModelState.IsValid)
            {
                try
                {
                    TNAV_PROJECT _PROJECT = _repository.TNAV_PROJECTs.AsNoTracking().Where(m => m.PROJECT_IDX == projectViewModel.PROJECT_IDX).First();

                    // Project의 변경 계약이면
                    if (projectViewModel.IS_MODIFY_CONTRACT)
                    {
                        // 수정된 횟수
                        int modifyCount = _repository.TNAV_PROJECT_HISTORies.Where(m => m.PROJECT_IDX == projectViewModel.PROJECT_IDX).Count();

                        // 기존 정보를 TNAV_PROJECT_HISTORY에 저장하여 이력으로 관리
                        TNAV_PROJECT_HISTORY _PROJECT_HISTORY = new TNAV_PROJECT_HISTORY()
                        {
                            IDX = projectViewModel.RELEATED_IDX.HasValue ? projectViewModel.RELEATED_IDX.Value : Guid.NewGuid(),
                            PROJECT_IDX = _PROJECT.PROJECT_IDX,
                            PROJECT_TITLE = _PROJECT.PROJECT_TITLE,
                            CONTRACT_DATE = _PROJECT.CONTRACT_DATE,
                            CONTRACT_NO = _PROJECT.CONTRACT_NO,
                            MODIFY_CONTRACT_COUNT = modifyCount == 1 ? 1 : modifyCount,
                            TERM_OF_PROJECT_START_DATE = _PROJECT.TERM_OF_PROJECT_START_DATE,
                            TERM_OF_PROJECT_END_DATE = _PROJECT.TERM_OF_PROJECT_END_DATE,
                            CURRENCY_SYMBOL = _PROJECT.CURRENCY_SYMBOL,
                            PROJECT_TOTAL_AMOUNT = _PROJECT.PROJECT_TOTAL_AMOUNT,
                            INVOICE_INITIAL_DATE = _PROJECT.INVOICE_INITIAL_DATE,
                            INVOICE_INITIAL_AMOUNT = _PROJECT.INVOICE_INITIAL_AMOUNT ?? 0,
                            INVOICE_INTER_DATE = _PROJECT.INVOICE_INTER_DATE,
                            INVOICE_INTER_AMOUNT = _PROJECT.INVOICE_INTER_AMOUNT ?? 0,
                            INVOICE_FINAL_DATE = _PROJECT.INVOICE_FINAL_DATE,
                            INVOICE_FINAL_AMOUNT = _PROJECT.INVOICE_FINAL_AMOUNT ?? 0,
                            INVOICE_IS_INITIAL = _PROJECT.INVOICE_IS_INITIAL,
                            INVOICE_IS_INTERMEDIATE = _PROJECT.INVOICE_IS_INTERMEDIATE,
                            INVOICE_IS_FINAL = _PROJECT.INVOICE_IS_FINAL,
                            CONTRACT_QUANTITY = _PROJECT.QUANTITY,
                            REG_DATE = DateTime.Now,
                            CREATE_USER_NAME = HttpContext.Session.GetString("UserName")
                        };

                        _repository.Add(_PROJECT_HISTORY);
                    }

                    // Project 수정
                    TNAV_PROJECT tNAV_PROJECT = new TNAV_PROJECT()
                    {
                        PROJECT_IDX = projectViewModel.PROJECT_IDX,
                        PROJECT_ID = projectViewModel.PROJECT_ID,
                        PROJECT_TITLE = projectViewModel.PROJECT_TITLE,
                        CONTRACT_DATE = projectViewModel.CONTRACT_DATE,
                        CONTRACT_NO = projectViewModel.CONTRACT_NO,
                        TERM_OF_PROJECT_START_DATE = projectViewModel.TERM_OF_PROJECT_START_DATE,
                        TERM_OF_PROJECT_END_DATE = projectViewModel.TERM_OF_PROJECT_END_DATE,
                        CURRENCY_SYMBOL = projectViewModel.CURRENCY_SYMPOL,
                        PROJECT_TOTAL_AMOUNT = projectViewModel.PROJECT_TOTAL_AMOUNT ?? 0,
                        INVOICE_INITIAL_DATE = projectViewModel.INVOICE_INITIAL_DATE,
                        INVOICE_INITIAL_AMOUNT = projectViewModel.INVOICE_INITIAL_AMOUNT ?? 0,
                        INVOICE_INTER_DATE = projectViewModel.INVOICE_INTER_DATE,
                        INVOICE_INTER_AMOUNT = projectViewModel.INVOICE_INTER_AMOUNT ?? 0,
                        INVOICE_FINAL_DATE = projectViewModel.INVOICE_FINAL_DATE,
                        INVOICE_FINAL_AMOUNT = projectViewModel.INVOICE_FINAL_AMOUNT ?? 0,
                        INVOICE_IS_INITIAL = projectViewModel.INVOICE_IS_INITIAL,
                        INVOICE_IS_INTERMEDIATE = projectViewModel.INVOICE_IS_INTERMEDIATE,
                        INVOICE_IS_FINAL = projectViewModel.INVOICE_IS_FINAL,
                        IS_MODIFY_CONTRACT = false,
                        QUANTITY = projectViewModel.QUANTITY,
                        WORK_ID_QUANTITY = projectViewModel.WORK_ID_QUANTITY,
                        PROJECT_TYPE = projectViewModel.PROJECT_TYPE,
                        CLIENT_CODE = projectViewModel.CLIENT_CODE,
                        CLIENT_NAME = projectViewModel.CLIENT_NAME,
                        CLIENT_SUPERVISOR_NAME = projectViewModel.CLIENT_SUPERVISOR_NAME,
                        CLIENT_SUPERVISOR_MOBILE = projectViewModel.CLIENT_SUPERVISOR_MOBILE,
                        CLIENT_SUPERVISOR_EMAIL = projectViewModel.CLIENT_SUPERVISOR_EMAIL,
                        CLIENT_CONTRACTOR_NAME = projectViewModel.CLIENT_CONTRACTOR_NAME,
                        CLIENT_CONTRACTOR_MOBILE = projectViewModel.CLIENT_CONTRACTOR_MOBILE,
                        CLIENT_CONTRACTOR_EMAIL = projectViewModel.CLIENT_CONTRACTOR_EMAIL,
                        REG_DATE = _PROJECT.REG_DATE,
                        CONFIRM_STATUS = projectViewModel.CONFIRM_STATUS,
                        CONFIRM_DATE = _PROJECT.CONFIRM_DATE,
                        CONFIRM_USER_NAME = _PROJECT.CONFIRM_USER_NAME,
                        REJECT_COMMENT = _PROJECT.REJECT_COMMENT,
                        REJECT_DATE = _PROJECT.REJECT_DATE,
                        REJECT_USER_NAME = _PROJECT.REJECT_USER_NAME,
                        STATUS = _PROJECT.STATUS,
                        PROJECT_PROGRESS = _PROJECT.PROJECT_PROGRESS,
                        CREATE_USER_NAME = _PROJECT.CREATE_USER_NAME

                    };

                    List<TNAV_PROJECT_PIC> tNAV_PROJECT_PIC_Add = new List<TNAV_PROJECT_PIC>();
                    List<TNAV_PROJECT_PIC> tNAV_PROJECT_PIC_Remove = _repository.TNAV_PROJECT_PICs.Where(m => m.PROJECT_IDX == projectViewModel.PROJECT_IDX).ToList();

                    // PM 수정
                    TNAV_PROJECT_PIC PM;
                    if (projectViewModel.PROJECT_PM is not null)
                    {
                        // 기존에 할당된 PM이 없고 새롭게 PM이 추가 된다면
                        if (tNAV_PROJECT_PIC_Remove.Where(m => m.PROJECT_POSTION == "PM").Count() == 0 && projectViewModel.PROJECT_PM.Count > 0)
                        {
                            tNAV_PROJECT.STATUS = CommonSettingData.ProjectID_Status.SUBMIT.ToString();
                        }

                        foreach (string pm in projectViewModel.PROJECT_PM)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PmUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pm).First();

                            PM = new TNAV_PROJECT_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                PROJECT_IDX = projectViewModel.PROJECT_IDX,
                                PROJECT_ID = projectViewModel.PROJECT_ID,
                                PROJECT_POSTION = "PM",
                                USER_ID = PmUser.USER_ID,
                                USER_NAME_KR = PmUser.USER_NAME,
                                USRE_NAME_EN = PmUser.USER_NAME_E,
                                DEPT_ID = PmUser.DEPT_ID,
                                DEPT_NAME_KR = PmUser.DEPT_NAME,
                                DEPT_NAME_EN = PmUser.DEPT_NAME_E,
                                DEGREE_KR = PmUser.DEGREE,
                                DEGREE_EN = PmUser.DEGREE_E,
                                EMP_ID = PmUser.EMP_ID,
                                SUR_NO = PmUser.SUR_NO,
                                POSITION_KR = PmUser.POSITION_K,
                                POSITION_EN = PmUser.POSITION_E,
                                REG_DATE = DateTime.Now
                            };

                            tNAV_PROJECT_PIC_Add.Add(PM);



                            // 수정되는 PM목록에서 기존과 다른 PM이 있으면 컨범을 활성화 하기 위해 Project의 Confirm_status 값 변경
                            foreach (TNAV_PROJECT_PIC existPm in tNAV_PROJECT_PIC_Remove.Where(m => m.PROJECT_POSTION == "PM"))
                            {
                                if (existPm.USER_ID != PM.USER_ID)
                                {
                                    tNAV_PROJECT.CONFIRM_STATUS = null;
                                }
                            }
                        }

                        // 로그인 사용자와 할당된 PM이 같으면
                        foreach (string pm in projectViewModel.PROJECT_PM)
                        {
                            if (pm == HttpContext.Session.GetString("UserName"))
                            {
                                ViewBag.IsPM = 1; // true
                                break;
                            }
                            else
                            {
                                ViewBag.IsPM = 0; // false
                            }
                        }
                    }

                    // PIC 수정
                    TNAV_PROJECT_PIC PIC;
                    if (projectViewModel.PROJECT_PIC is not null)
                    {
                        foreach (string pic in projectViewModel.PROJECT_PIC)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PicUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pic).First();

                            PIC = new TNAV_PROJECT_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                PROJECT_IDX = projectViewModel.PROJECT_IDX,
                                PROJECT_ID = projectViewModel.PROJECT_ID,
                                PROJECT_POSTION = "PIC",
                                USER_ID = PicUser.USER_ID,
                                USER_NAME_KR = PicUser.USER_NAME,
                                USRE_NAME_EN = PicUser.USER_NAME_E,
                                DEPT_ID = PicUser.DEPT_ID,
                                DEPT_NAME_KR = PicUser.DEPT_NAME,
                                DEPT_NAME_EN = PicUser.DEPT_NAME_E,
                                DEGREE_KR = PicUser.DEGREE,
                                DEGREE_EN = PicUser.DEGREE_E,
                                EMP_ID = PicUser.EMP_ID,
                                SUR_NO = PicUser.SUR_NO,
                                POSITION_KR = PicUser.POSITION_K,
                                POSITION_EN = PicUser.POSITION_E,
                                REG_DATE = DateTime.Now
                            };

                            tNAV_PROJECT_PIC_Add.Add(PIC);
                        }
                    }

                    // PM으로 할당된 PM 사용자 정보
                    List<TNAV_PROJECT_PIC> tNAV_PROJECT_PM = _repository.TNAV_PROJECT_PICs.Where(m => m.PROJECT_IDX == projectViewModel.PROJECT_IDX && m.PROJECT_POSTION == "PM").ToList();

                    // PIC로 할당된 PIC 사용자 정보
                    List<TNAV_PROJECT_PIC> tNAV_PROJECT_PIC = _repository.TNAV_PROJECT_PICs.Where(m => m.PROJECT_IDX == projectViewModel.PROJECT_IDX && m.PROJECT_POSTION == "PIC").ToList();

                    ViewBag.Pm = tNAV_PROJECT_PM;
                    ViewBag.Pic = tNAV_PROJECT_PIC;

                    // 상태값 저장
                    TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                    {
                        USER_NAME = HttpContext.Session.GetString("UserName"),
                        PLATFORM = "PMS",
                        MENU_NAME = "PROJECT ID",
                        TARGET_IDX = projectViewModel.PROJECT_IDX,
                        STATUS = CommonSettingData.LogStatus.MODIFY.ToString(),
                        REG_DATE = DateTime.Now
                    };
                    _repository.Add(LogViewModel);

                    _repository.Update(tNAV_PROJECT);
                    _repository.RemoveRange(tNAV_PROJECT_PIC_Remove);
                    _repository.AddRange(tNAV_PROJECT_PIC_Add);
                    await _repository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (!TnavProjectExists(projectViewModel.PROJECT_IDX))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "PmsProjectIdManagement", returnView = "Detail" });
                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "PmsProjectIdManagement", returnView = "Detail" });
                }

                return RedirectToAction(nameof(Detail), new { id = projectViewModel.PROJECT_IDX });
            }

            return RedirectToAction(nameof(Detail), new { id = projectViewModel.PROJECT_IDX });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool TnavProjectExists(Guid id)
        {
            return (_repository.TNAV_PROJECTs?.Any(e => e.PROJECT_IDX == id)).GetValueOrDefault();
        }

        /// <summary>
        /// PM으로서 프로젝트를 승인 함
        /// </summary>
        /// <param name="Project_Idx"></param>
        /// <returns></returns>
        public async Task<IActionResult> ConfrimProject(Guid Project_Idx)
        {
            try
            {
                string result = String.Empty;
                if (TnavProjectExists(Project_Idx))
                {
                    TNAV_PROJECT _PROJECT = await _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == Project_Idx).FirstAsync();

                    _PROJECT.CONFIRM_STATUS = CommonSettingData.ProjectConfirmFromPM.CONFIRM.ToString();
                    _PROJECT.STATUS = CommonSettingData.ProjectID_Status.CONFIRM.ToString();
                    _PROJECT.CONFIRM_DATE = DateTime.Now;

                    _repository.Update(_PROJECT);
                    await _repository.SaveChangesAsync(); ;

                    result = "OK";
                    return Json(result);
                }
                else
                {
                    result = "Failure";
                    return Json(result);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// PM으로서 프로젝트를 반려 함.
        /// </summary>
        /// <param name="Project_Idx"></param>
        /// <param name="Comment"></param>
        /// <returns></returns>
        public async Task<IActionResult> RejectProject(Guid Project_Idx, string Comment)
        {
            try
            {
                string result = String.Empty;
                if (TnavProjectExists(Project_Idx))
                {
                    TNAV_PROJECT _PROJECT = await _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == Project_Idx).FirstAsync();

                    _PROJECT.CONFIRM_STATUS = CommonSettingData.ProjectConfirmFromPM.REJECT.ToString();
                    _PROJECT.STATUS = CommonSettingData.ProjectID_Status.REJECTED.ToString();
                    _PROJECT.REJECT_DATE = DateTime.Now;
                    _PROJECT.REJECT_COMMENT = Comment;

                    _repository.Update(_PROJECT);
                    await _repository.SaveChangesAsync(); ;

                    result = "OK";
                    return Json(result);
                }
                else
                {
                    result = "Failure";
                    return Json(result);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
