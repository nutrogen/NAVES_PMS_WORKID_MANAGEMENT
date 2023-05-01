using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.ViewModels;
using Microsoft.CodeAnalysis;
using NuGet.Protocol.Core.Types;
using Syncfusion.EJ2.Base;
using static NuGet.Packaging.PackagingConstants;
using System.Collections;
using Syncfusion.EJ2.Linq;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsWorkIdManagementController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly INavesPortalCommonService _commonService;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public PmsWorkIdManagementController(BM_NAVES_PortalContext repository, INavesPortalCommonService commonService, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = repository;
            _commonService = commonService;
            _procedures = procedures;
        }

        /// <summary>
        /// 목록 페이지
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View());
        }

        /// <summary>
        /// Work Id List
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>json</returns>
        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_PMS_GET_WORK_LISTResult> resultList = await _procedures.PNAV_PMS_GET_WORK_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_PMS_GET_WORK_LISTResult>().Count();

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
        /// 생성 페이지
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            // Project Id 
            List<PNAV_PMS_GET_SEARCH_PROJECT_ID_FOR_WORK_IDResult> resultProjectList = await _procedures.PNAV_PMS_GET_SEARCH_PROJECT_ID_FOR_WORK_IDAsync(string.Empty);
            ViewBag.ProjectId = resultProjectList;

            // JOb ID
            List<PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDResult> resultJobIdList = await _procedures.PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDAsync(string.Empty);
            ViewBag.JobId = resultJobIdList;

            // Work Category Code List
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync();

            // Department
            Department department = new Department();

            return await Task.Run(() => View());
        }

        public async Task<IActionResult> UrlDatasourceForJobId([FromBody] DataManagerRequest? dm)
        {
            List<PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDResult> resultList = await _procedures.PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDAsync(string.Empty);

            IEnumerable DataSource = resultList.AsEnumerable();
            DataOperations operation = new DataOperations();


            if (dm.Take != 0)
            {
                DataSource = operation.PerformTake(DataSource, dm.Take);
            }

            return Json(DataSource);
        }

        /// <summary>
        /// 상세 페이지
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(Guid? id)
        {
            // Project Id 
            List<PNAV_PMS_GET_SEARCH_PROJECT_ID_FOR_WORK_IDResult> resultProjectList = await _procedures.PNAV_PMS_GET_SEARCH_PROJECT_ID_FOR_WORK_IDAsync(string.Empty);
            ViewBag.ProjectId = resultProjectList;

            // JOb ID
            List<PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDResult> resultJobIdList = await _procedures.PNAV_PMS_GET_SEARCH_JOB_ID_FOR_WORK_IDAsync(string.Empty);
            ViewBag.JobId = resultJobIdList;

            //Work Category Code List
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync();

            try
            {
                TNAV_WORK tNAV_WORK = await _repository.TNAV_WORKs.Where(m => m.WORK_IDX == id).SingleAsync();
                return View(tNAV_WORK);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Project Id 상세 내용
        /// </summary>
        /// <param name="_projectID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProjectId(string _projectID)
        {
            VNAV_SELECT_PMS_PROJECTID_DETAIL result = new VNAV_SELECT_PMS_PROJECTID_DETAIL();
            try
            {
                if (_projectID.Count() > 2)
                {
                    result = _repository.VNAV_SELECT_PMS_PROJECTID_DETAILs.Where(m => m.PROJECT_ID == _projectID).First();
                }
                return Json(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Job Id 상세 내용
        /// </summary>
        /// <param name="_jobId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetJotId(string _jobId)
        {
            try
            {
                if (_jobId.Count() > 2)
                {
                    VNAV_SELECT_PMS_JOB_DETAIL result = _repository.VNAV_SELECT_PMS_JOB_DETAILs.Where(m => m.JOB_ID == _jobId).First();
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
            }
        }

        /// <summary>
        /// Work ID 생성
        /// </summary>
        /// <param name="_part">부서코드</param>
        /// <param name="_projectCagtegory">Project Cagegory.. SIP, MRO...</param>
        /// <param name="_workCategory">Work Cagegory... CRN, SAT...</param>
        /// <returns></returns>
        public async Task<string> CreateWorkId(string _deptCode, string _projectType, string _workCategory, DateTime _contractDate)
        {
            string _PartCode = string.Empty;
            switch (_deptCode.ToUpper())
            {
                case "NVP": // 함정사업기획팀
                    _PartCode = "1";
                    break;
                case "NVS": // 함정검사팀
                    _PartCode = "2";
                    break;
                case "NVT": // 함정기술팀
                    _PartCode = "3";
                    break;
                default:
                    break;
            }

            string _year = _contractDate.ToString("yy");

            OutputParameter<int?> MaxVal = new OutputParameter<int?>();
            await _procedures.PNAV_PMS_GET_MAX_WORK_IDAsync(_year, _projectType, _workCategory, RESULT: MaxVal);

            string _sn = (MaxVal.Value ?? 1).ToString("00");
            string _workId = _PartCode + _projectType + _workCategory + _sn + "-" + _contractDate.ToString("yy");

            return _workId;
        }

        /// <summary>
        /// 생성 저장하기
        /// </summary>
        /// <param name="tNAV_WORK"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TNAV_WORK tNAV_WORK)
        {
            try
            {
                Guid _work_Idx = Guid.Empty;
                if (tNAV_WORK is not null)
                {
                    _work_Idx = Guid.NewGuid();

                    TNAV_PROJECT project = _repository.TNAV_PROJECTs.Where(m => m.PROJECT_ID == tNAV_WORK.PROJECT_ID).First();
                    string _workId = await CreateWorkId(tNAV_WORK.DEPARTMENT, project.PROJECT_TYPE, tNAV_WORK.PROJECT_CATEGORY, project.CONTRACT_DATE);

                    TNAV_WORK _tnavWork = new TNAV_WORK()
                    {
                        WORK_IDX = _work_Idx,
                        WORK_ID = _workId,
                        PROJECT_ID = tNAV_WORK.PROJECT_ID,
                        PROJECT_IDX = tNAV_WORK.PROJECT_IDX,
                        JOB_ID = tNAV_WORK.JOB_ID,
                        JOB_IDX = tNAV_WORK.JOB_IDX,                        
                        DEPARTMENT = tNAV_WORK.DEPARTMENT,
                        PROJECT_CATEGORY = tNAV_WORK.PROJECT_CATEGORY,
                        PARTICULAR = tNAV_WORK.PARTICULAR,
                        DESCRIPTION = tNAV_WORK.DESCRIPTION,
                        CURRENCY = tNAV_WORK.CURRENCY,
                        SUB_AMOUNT = tNAV_WORK.SUB_AMOUNT,
                        UNIT = tNAV_WORK.UNIT,
                        QUANTITY = tNAV_WORK.QUANTITY,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        IS_DELETE = false,
                    };

                    if (ModelState.IsValid)
                    {
                        // 상태값 저장
                        TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                        {
                            REG_DATE = DateTime.Now,
                            USER_NAME = HttpContext.Session.GetString("UserName"),
                            PLATFORM = "PMS",
                            MENU_NAME = "WORK ID",
                            TARGET_IDX = _tnavWork.WORK_IDX,
                            STATUS = CommonSettingData.LogStatus.CREATE.ToString()
                        };
                        _repository.Add(LogViewModel);

                        _repository.Add(_tnavWork);
                        await _repository.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Detail), new { id = _work_Idx });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PopUpCreate(ProjectIdWorkIdViewModel ProjectIdWorkId)
        {
            try
            {
                Guid _work_Idx = Guid.Empty;
                if (ProjectIdWorkId.WORK is not null)
                {
                    _work_Idx = Guid.NewGuid();

                    TNAV_PROJECT _project = _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == ProjectIdWorkId.PROJECTID_DETAIL.PROJECT_IDX).First();
                    string _workId = await CreateWorkId(ProjectIdWorkId.WORK.DEPARTMENT, _project.PROJECT_TYPE, ProjectIdWorkId.WORK.PROJECT_CATEGORY, _project.CONTRACT_DATE);

                    TNAV_WORK _tnavWork = new TNAV_WORK()
                    {
                        WORK_IDX = _work_Idx,
                        WORK_ID = _workId,
                        PROJECT_ID = _project.PROJECT_ID,
                        PROJECT_IDX = _project.PROJECT_IDX,
                        JOB_ID = ProjectIdWorkId.WORK.JOB_ID,
                        JOB_IDX = ProjectIdWorkId.WORK.JOB_IDX,
                        CURRENCY = ProjectIdWorkId.WORK.CURRENCY,
                        UNIT = ProjectIdWorkId.WORK.UNIT,
                        DEPARTMENT = ProjectIdWorkId.WORK.DEPARTMENT,
                        PROJECT_CATEGORY = ProjectIdWorkId.WORK.PROJECT_CATEGORY,
                        PARTICULAR = ProjectIdWorkId.WORK.PARTICULAR,
                        DESCRIPTION = ProjectIdWorkId.WORK.DESCRIPTION,
                        SUB_AMOUNT = ProjectIdWorkId.WORK.SUB_AMOUNT,
                        QUANTITY = ProjectIdWorkId.WORK.QUANTITY,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        IS_DELETE = false,
                    };

                    if (ModelState.IsValid)
                    {
                        // 상태값 저장
                        TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                        {
                            REG_DATE = DateTime.Now,
                            USER_NAME = HttpContext.Session.GetString("UserName"),
                            PLATFORM = "PMS",
                            MENU_NAME = "WORK ID",
                            TARGET_IDX = _work_Idx,
                            STATUS = CommonSettingData.LogStatus.CREATE.ToString()
                        };
                        _repository.Add(LogViewModel);

                        _repository.Add(_tnavWork);
                        await _repository.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(WorkIdCreatePopUp), new { projectIDX = ProjectIdWorkId.PROJECTID_DETAIL.PROJECT_IDX, Success = "OK" });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PopUpModify(ProjectIdWorkIdViewModel ProjectIdWorkId)
        {
            try
            {
                if (ProjectIdWorkId.WORK is not null)
                {
                    var _project = _repository.TNAV_PROJECTs.AsNoTracking().Where(m => m.PROJECT_IDX == ProjectIdWorkId.PROJECTID_DETAIL.PROJECT_IDX).First();
                    TNAV_WORK work = _repository.TNAV_WORKs.AsNoTracking().Where(m => m.WORK_IDX == ProjectIdWorkId.WORK.WORK_IDX).First();

                    TNAV_WORK _tnavWork = new TNAV_WORK()
                    {
                        WORK_IDX = ProjectIdWorkId.WORK.WORK_IDX,
                        WORK_ID = ProjectIdWorkId.WORK.WORK_ID,
                        PROJECT_ID = _project.PROJECT_ID,
                        PROJECT_IDX = _project.PROJECT_IDX,
                        CURRENCY = ProjectIdWorkId.WORK.CURRENCY,
                        UNIT = ProjectIdWorkId.WORK.UNIT,
                        JOB_ID = ProjectIdWorkId.WORK.JOB_ID,
                        JOB_IDX = ProjectIdWorkId.WORK.JOB_IDX,
                        DEPARTMENT = ProjectIdWorkId.WORK.DEPARTMENT,
                        PROJECT_CATEGORY = ProjectIdWorkId.WORK.PROJECT_CATEGORY,
                        PARTICULAR = ProjectIdWorkId.WORK.PARTICULAR,
                        DESCRIPTION = ProjectIdWorkId.WORK.DESCRIPTION,
                        SUB_AMOUNT = ProjectIdWorkId.WORK.SUB_AMOUNT,
                        QUANTITY = ProjectIdWorkId.WORK.QUANTITY,
                        MODIFY_USER = HttpContext.Session.GetString("UserName"),
                        MODIFY_DATE = DateTime.Now,
                        IS_DELETE = false,
                    };

                    if (ModelState.IsValid)
                    {
                        // 상태값 저장
                        TNAV_COMMON_LOG LogViewModel = new TNAV_COMMON_LOG()
                        {
                            REG_DATE = DateTime.Now,
                            USER_NAME = HttpContext.Session.GetString("UserName"),
                            PLATFORM = "PMS",
                            MENU_NAME = "WORK ID",
                            TARGET_IDX = _tnavWork.WORK_IDX,
                            STATUS = CommonSettingData.LogStatus.MODIFY.ToString()
                        };
                        _repository.Add(LogViewModel);

                        _repository.Update(_tnavWork);
                        await _repository.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(WorkIdDetailPopUp), new { WorkIdx = ProjectIdWorkId.WORK.WORK_IDX, Success = "OK" });
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 수정, 삭제 저장하기
        /// </summary>
        /// <param name="tNAV_WORK"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TNAV_WORK tNAV_WORK)
        {
            if (tNAV_WORK.WORK_IDX == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var WorkToUpdate = await _repository.TNAV_WORKs.AsNoTracking().FirstOrDefaultAsync(m => m.WORK_IDX == tNAV_WORK.WORK_IDX);

                if (WorkToUpdate != null)
                {
                    WorkToUpdate.DEPARTMENT = tNAV_WORK.DEPARTMENT;
                    WorkToUpdate.DESCRIPTION = tNAV_WORK.DESCRIPTION;
                    WorkToUpdate.CURRENCY = tNAV_WORK.CURRENCY;
                    WorkToUpdate.SUB_AMOUNT = tNAV_WORK.SUB_AMOUNT;
                    WorkToUpdate.UNIT = tNAV_WORK.UNIT;
                    WorkToUpdate.QUANTITY = tNAV_WORK.QUANTITY;
                    WorkToUpdate.PARTICULAR = tNAV_WORK.PARTICULAR;

                    _repository.TNAV_WORKs.Update(WorkToUpdate);
                    await _repository.SaveChangesAsync();

                    return RedirectToAction(nameof(Detail), new { id = tNAV_WORK.WORK_IDX });
                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Work ID 생성
        /// </summary>
        /// <returns></returns>
        public IActionResult WorkIdCreatePopUp(Guid? projectIDX)
        {
            if (projectIDX == null)
            {
                return View();
            }

            ViewBag.ProjectId = _repository.TNAV_PROJECTs.ToList().OrderByDescending(m => m.REG_DATE);
            ViewBag.JobId = _repository.TNAV_JOB_MANAGEMENTs.ToList().OrderByDescending(m => m.REG_DATE);

            // Work Category Code List
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync();

            ProjectIdWorkIdViewModel projectIdWorkIdViewModel = new ProjectIdWorkIdViewModel()
            {
                PROJECTID_DETAIL = _repository.VNAV_SELECT_PMS_PROJECTID_DETAILs.Where(m => m.PROJECT_IDX == projectIDX).First(),
                WORK = new TNAV_WORK()
            };

            return View(projectIdWorkIdViewModel);
        }

        /// <summary>
        /// Work ID 상세 또는 수정
        /// </summary>
        /// <returns></returns>
        public IActionResult WorkIdDetailPopUp(Guid? WorkIdx)
        {
            ViewBag.ProjectId = _repository.TNAV_PROJECTs.AsNoTracking().ToList().OrderByDescending(m => m.REG_DATE);
            ViewBag.JobId = _repository.TNAV_JOB_MANAGEMENTs.ToList().OrderByDescending(m => m.REG_DATE);
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync();

            try
            {
                TNAV_WORK work = _repository.TNAV_WORKs.Where(m => m.WORK_IDX == WorkIdx).First();
                ProjectIdWorkIdViewModel ProjectIdWorkIdViewModel = new ProjectIdWorkIdViewModel()
                {
                    PROJECTID_DETAIL = _repository.VNAV_SELECT_PMS_PROJECTID_DETAILs.Where(m => m.PROJECT_IDX == work.PROJECT_IDX).First(),
                    WORK = work ?? new TNAV_WORK()
                };
                return View(ProjectIdWorkIdViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}