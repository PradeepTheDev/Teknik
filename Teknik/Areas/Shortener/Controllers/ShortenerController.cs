using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Shortener.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Shortener.Controllers
{
    [TeknikAuthorize]
    public class ShortenerController : DefaultController
    {
        [TrackPageView]
        public ActionResult Index()
        {
            ViewBag.Title = "Url Shortener - " + Config.Title;
            ShortenViewModel model = new ShortenViewModel();
            return View(model);
        }

        [TrackLink]
        [AllowAnonymous]
        public ActionResult RedirectToUrl(string url)
        {
            using (TeknikEntities db = new TeknikEntities())
            {
                ShortenedUrl shortUrl = db.ShortenedUrls.Where(s => s.ShortUrl == url).FirstOrDefault();
                if (shortUrl != null)
                {
                    shortUrl.Views += 1;
                    db.Entry(shortUrl).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Redirect(shortUrl.OriginalUrl);
                }
                return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
            }
        }

        [HttpPost]
        public ActionResult ShortenUrl(string url)
        {
            if (url.IsValidUrl())
            {
                using (TeknikEntities db = new TeknikEntities())
                {
                    ShortenedUrl newUrl = Shortener.ShortenUrl(db, url, Config.ShortenerConfig.UrlLength);

                    if (User.Identity.IsAuthenticated)
                    {
                        Users.Models.User foundUser = UserHelper.GetUser(db, User.Identity.Name);
                        if (foundUser != null)
                        {
                            newUrl.UserId = foundUser.UserId;
                        }
                    }

                    db.ShortenedUrls.Add(newUrl);
                    db.SaveChanges();

                    string shortUrl = string.Format("{0}://{1}/{2}", HttpContext.Request.Url.Scheme, Config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                    if (Config.DevEnvironment)
                    {
                        shortUrl = Url.SubRouteUrl("shortened", "Shortener.View", new { url = newUrl.ShortUrl });
                    }

                    return Json(new { result = new { shortUrl = shortUrl, originalUrl = url } });
                }
            }
            return Json(new { error = "Must be a valid Url" });
        }

        [AllowAnonymous]
        public ActionResult Verify()
        {
            ViewBag.Title = "Url Shortener Verification - " + Config.Title;
            return View();
        }
    }
}
