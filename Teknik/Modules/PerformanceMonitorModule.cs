using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.Utilities;

namespace Teknik.Modules
{
    public class PerformanceMonitorModule : IHttpModule
    {
        public void Dispose() { /* Nothing to do */ }
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += delegate (object sender, EventArgs e)
            {
                HttpContext requestContext = ((HttpApplication)sender).Context;
                Stopwatch timer = new Stopwatch();
                requestContext.Items["Timer"] = timer;
                timer.Start();
            };
            context.PostRequestHandlerExecute += delegate (object sender, EventArgs e)
            {
                HttpContext requestContext = ((HttpApplication)sender).Context;

                Stopwatch timer = (Stopwatch)requestContext.Items["Timer"];
                timer.Stop();
                // Don't interfere with non-HTML responses 

                if (requestContext.Response.ContentType == "text/html" && requestContext.Response.StatusCode == 200 && !new HttpRequestWrapper(requestContext.Request).IsAjaxRequest())
                {
                    double ms = (double)timer.ElapsedMilliseconds;
                    string result = string.Format("{0:F0}", ms);

                    requestContext.Response.Write(
                            "<script nonce=\"" + requestContext.Items[Constants.NONCE_KEY] + "\">" +
                                "var pageGenerationTime = '" + result + "';" +
                                "pageloadStopTimer();" +
                            "</script >");
                }
            };
        }
    }
}
