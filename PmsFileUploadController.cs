using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.ViewModels;
using NuGet.Protocol.Core.Types;
using Syncfusion.EJ2.Inputs;
using System.Net.Http.Headers;

namespace NavesPortalforWebWithCoreMvc.Controllers.PMS
{
    [Authorize]
    [CheckSession]
    public class PmsFileUploadController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;

        private string _uploadFolderName = "UploadFiles";
        private IWebHostEnvironment hostingEnv;

        public PmsFileUploadController(IWebHostEnvironment env, BM_NAVES_PortalContext repository)
        {
            this.hostingEnv = env;
            _repository = repository;
        }

        /// <summary>
        /// 첫 계약서 파일 수정 저장 및 업로드
        /// </summary>
        /// <param name="UploadFilesContractDoc"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult SaveUploadFilesContractDoc(IList<IFormFile> UploadFilesContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            Save(UploadFilesContractDoc, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);

            return Content("");
        }

        /// <summary>
        /// 첫 계약서 Uploader에서 파일 삭제
        /// </summary>
        /// <param name="UploadFilesContractDoc"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult RemoveUploadFilesContractDoc(IList<IFormFile> UploadFilesContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {

            Remove(UploadFilesContractDoc, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);
            return Content("");
        }

        /// <summary>
        /// 수정 계약서 파일 저장 및 업로드
        /// </summary>
        /// <param name="UploadFilesModifyContractDoc"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult SaveUploadFilesModifyContractDoc(IList<IFormFile> UploadFilesModifyContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string ReleatedBrforeInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            try
            {
                // 수정된 횟수
                //int relatedInfoCount = _repository.TNAV_PROJECT_HISTORies.Where(m => m.PROJECT_IDX.Equals(CurrentDocIdx)).Count();

                //기존 첨부된 ContractDocument -> ModifyContractDocument로 변경
                List<TNAV_COM_FILE> contractDoc = _repository.TNAV_COM_FILEs.Where(m =>
                                                           m.DOCUMENT_IDX.Equals(CurrentDocIdx) &&
                                                           m.KIND_OF_FILES == "ContractDocument" &&
                                                           m.RELATED_INFO == ReleatedBrforeInfo).AsNoTracking().ToList();

                if (contractDoc.Count > 0)
                {
                    foreach (TNAV_COM_FILE file in contractDoc)
                    {
                        file.KIND_OF_FILES = "ModifyContractDocument";
                    }

                    _repository.UpdateRange(contractDoc);
                    _repository.SaveChanges();
                }

                Save(UploadFilesModifyContractDoc, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);
            }
            catch (Exception)
            {
                throw;
            }
            return Content("");
        }

        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult RemoveUploadFilesModifyContractDoc(IList<IFormFile> UploadFilesModifyContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {

            Remove(UploadFilesModifyContractDoc, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);


            return Content("");
        }


        private void Save(IList<IFormFile> UploadFiles,
                string PlatformName,
                Guid CurrentDocIdx,
                string Type,
                Guid ReleatedInfo,
                string OriginalFileName,
                string SavedFileName,
                Guid? ProjectIdx,
                string? ProjectId,
                Guid? WorkIdx,
                string? WorkId,
                string CreateUserName)
        {
            try
            {
                foreach (var file in UploadFiles)
                {
                    if (UploadFiles != null)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var targetFolder = hostingEnv.WebRootPath + $@"\{_uploadFolderName}\{PlatformName}";


                        // 플랫폼별 대상 경로 확인
                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        var targetProjectFolder = $@"{targetFolder}";
                        if (!string.IsNullOrEmpty(ProjectId) || ProjectId != null)
                        {
                            targetProjectFolder = $@"{targetProjectFolder}\{ProjectId}";

                            // Project 별 대상 경로 확인
                            if (!Directory.Exists(targetProjectFolder))
                            {
                                Directory.CreateDirectory(targetProjectFolder);
                            }
                        }

                        var targetWorkFolder = $@"{targetProjectFolder}";
                        if (!string.IsNullOrEmpty(WorkId) || WorkId != null)
                        {
                            targetWorkFolder = $@"{targetWorkFolder}\{WorkId}";

                            // Work ID 별 대상 경로 확인
                            if (!Directory.Exists(targetWorkFolder))
                            {
                                Directory.CreateDirectory(targetWorkFolder);
                            }
                        }

                        fileName = $@"{targetWorkFolder}\{fileName}";

                        // 파일 유무 확인 후 업로드
                        if (!System.IO.File.Exists(fileName))
                        {
                            using (FileStream fs = System.IO.File.Create(fileName))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }

                            //업로드 파일 정보 저장
                            TNAV_COM_FILE _COM_FILE = new TNAV_COM_FILE()
                            {
                                IDX = Guid.NewGuid(),
                                PLATFORM = PlatformName,
                                DOCUMENT_IDX = CurrentDocIdx,
                                KIND_OF_FILES = Type,
                                RELATED_INFO = ReleatedInfo.ToString().ToUpper(),
                                PROJECT_IDX = ProjectIdx,
                                PROJECT_ID = ProjectId,
                                WORK_IDX = WorkIdx,
                                WORK_ID = WorkId,
                                SAVED_FULL_PATH = fileName,
                                ORIGINAL_FILENAME = OriginalFileName,
                                EXTENSION = Path.GetExtension(fileName).ToLower(),
                                SAVED_FILENAME = SavedFileName,
                                REG_DATE = DateTime.Now,
                                CREATE_USER_NAME = CreateUserName
                            };

                            _repository.Add(_COM_FILE);
                            _repository.SaveChanges();
                        }
                        else
                        {
                            Response.Clear();
                            Response.StatusCode = 204;
                            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File already exists.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Remove(UploadFiles, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);
                
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "No Content";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UploadFilesContractDoc"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        public void Remove(IList<IFormFile> UploadFilesContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            try
            {
                foreach (var file in UploadFilesContractDoc)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var targetFolder = hostingEnv.WebRootPath + $@"\{_uploadFolderName}\{PlatformName}";
                    fileName = $@"{targetFolder}\{fileName}";

                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);

                        //업로드된 파일 삭제
                        if (_repository.TNAV_COM_FILEs.Where(m => m.SAVED_FILENAME == SavedFileName).Any())
                        {
                            var _COM_FILE = _repository.TNAV_COM_FILEs.Where(m => m.SAVED_FILENAME == SavedFileName).First();
                            _repository.Remove(_COM_FILE);
                            _repository.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }
    }
}
