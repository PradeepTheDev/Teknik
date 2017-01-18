﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Piwik;

namespace Teknik.Filters
{
    public class TrackPageView : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string title = string.Empty;
            if (filterContext.Controller != null && filterContext.Controller.ViewBag != null && filterContext.Controller.ViewBag.Title != null)
            {
                title = filterContext.Controller.ViewBag.Title;
            }
            HttpRequestBase request = filterContext.HttpContext.Request;
            // Fire and forget.  Don't need to wait for it.
            Task.Run(() => AsyncTrackPageView(request, title));

            base.OnActionExecuted(filterContext);
        }

        private void AsyncTrackPageView(HttpRequestBase request, string title)
        {
            Config config = Config.Load();
            Tracking.TrackPageView(request, config, title);
        }
    }
}
