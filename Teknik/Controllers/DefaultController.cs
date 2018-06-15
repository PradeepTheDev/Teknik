using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Teknik.Areas.Error.Controllers;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Security;
using Teknik.Utilities;

namespace Teknik.Controllers
{
    [AllowAnonymous]
    [CORSActionFilter]
    [Area("Default")]
    public class DefaultController : Controller
    {
        protected string Subdomain
        {
            get { return (string)this.ControllerContext.RouteData.Values["sub"]; }
        }

        protected readonly ILogger<Logger> _logger;
        protected readonly Config _config;
        protected readonly TeknikEntities _dbContext;

        //protected virtual new TeknikPrincipal User
        //{
        //    get { return HttpContext.User as TeknikPrincipal; }
        //}

        public DefaultController(ILogger<Logger> logger, Config config, TeknikEntities dbContext)
        {
            _logger = logger;
            _config = config;
            _dbContext = dbContext;

            ViewBag.Title = _config.Title;
            ViewBag.Description = _config.Description;
        }

        // Get the Favicon
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public IActionResult Favicon([FromServices] IHostingEnvironment env)
        {
            //string imageFile = FileHelper.MapPath(env, Constants.FAVICON_PATH);
            return File(Constants.FAVICON_PATH, "image/x-icon");
        }

        // Get the Logo
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public IActionResult Logo([FromServices] IHostingEnvironment env)
        {
            //string imageFile = FileHelper.MapPath(env, Constants.LOGO_PATH);
            return File(Constants.LOGO_PATH, "image/svg+xml");
        }

        // Get the Robots.txt
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Robots([FromServices] IHostingEnvironment env)
        {
            //string file = FileHelper.MapPath(env, Constants.ROBOTS_PATH);
            return File(Constants.ROBOTS_PATH, "text/plain");
        }

        protected IActionResult GenerateActionResult(object json)
        {
            return GenerateActionResult(json, View());
        }

        protected IActionResult GenerateActionResult(object json, IActionResult result)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(json);
            }
            return result;
        }

        protected async Task<string> RenderPartialViewToString(ICompositeViewEngine viewEngine, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult =
                    viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
