using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Admin.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;
using Teknik.ViewModels;
using Teknik.Logging;

namespace Teknik.Areas.Admin.Controllers
{
    [TeknikAuthorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminController : DefaultController
    {
        public AdminController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base (logger, config, dbContext) { }

        [HttpGet]
        [ServiceFilter(typeof(TrackPageView))]
        public ActionResult Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(TrackPageView))]
        public ActionResult UserSearch()
        {
            UserSearchViewModel model = new UserSearchViewModel();
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(TrackPageView))]
        public ActionResult UserInfo(string username)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                User user = UserHelper.GetUser(_dbContext, username);
                UserInfoViewModel model = new UserInfoViewModel();
                model.Username = user.Username;
                model.AccountType = user.AccountType;
                model.AccountStatus = user.AccountStatus;
                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpGet]
        public ActionResult UploadSearch()
        {
            UploadSearchViewModel model = new UploadSearchViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> GetUserSearchResults(string query, [FromServices] ICompositeViewEngine viewEngine)
        {
            List<UserResultViewModel> models = new List<UserResultViewModel>();

            var results = _dbContext.Users.Where(u => u.Username.Contains(query)).ToList();
            if (results != null)
            {
                foreach (User user in results)
                {
                    try
                    {
                        UserResultViewModel model = new UserResultViewModel();
                        model.Username = user.Username;
                        if (_config.EmailConfig.Enabled)
                        {
                            model.Email = string.Format("{0}@{1}", user.Username, _config.EmailConfig.Domain);
                        }
                        model.JoinDate = user.JoinDate;
                        model.LastSeen = UserHelper.GetLastAccountActivity(_dbContext, _config, user);
                        models.Add(model);
                    }
                    catch (Exception)
                    {
                        // Skip this result
                    }
                }
            }

            string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/UserResults.cshtml", models);

            return Json(new { result = new { html = renderedView } });
        }

        [HttpPost]
        public async Task<ActionResult> GetUploadSearchResults(string url, [FromServices] ICompositeViewEngine viewEngine)
        {
            Upload.Models.Upload foundUpload = _dbContext.Uploads.Where(u => u.Url == url).FirstOrDefault();
            if (foundUpload != null)
            {
                UploadResultViewModel model = new UploadResultViewModel();

                model.Url = foundUpload.Url;
                model.ContentType = foundUpload.ContentType;
                model.ContentLength = foundUpload.ContentLength;
                model.DateUploaded = foundUpload.DateUploaded;
                model.Downloads = foundUpload.Downloads;
                model.DeleteKey = foundUpload.DeleteKey;

                string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/UploadResult.cshtml", model);

                return Json(new { result = new { html = renderedView } });
            }
            return Json(new { error = new { message = "Upload does not exist" } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccountType(string username, AccountType accountType)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                // Edit the user's account type
                UserHelper.EditAccountType(_dbContext, _config, username, accountType);
                return Json(new { result = new { success = true } });
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccountStatus(string username, AccountStatus accountStatus)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                // Edit the user's account type
                UserHelper.EditAccountStatus(_dbContext, _config, username, accountStatus);
                return Json(new { result = new { success = true } });
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInviteCode(string username)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                User user = UserHelper.GetUser(_dbContext, username);
                InviteCode inviteCode = new InviteCode();
                inviteCode.Active = true;
                inviteCode.Code = Guid.NewGuid().ToString();
                inviteCode.Owner = user;
                _dbContext.InviteCodes.Add(inviteCode);
                _dbContext.SaveChanges();

                return Json(new { result = new { code = inviteCode.Code } });
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAccount(string username)
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, username);
                if (user != null)
                {
                    UserHelper.DeleteAccount(_dbContext, _config, user);
                    return Json(new { result = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
            return Json(new { error = "Unable to delete user" });
        }
    }
}
