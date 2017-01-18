﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Podcast.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Podcast.Controllers
{
    public class PodcastController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            MainViewModel model = new MainViewModel();
            model.Title = Config.PodcastConfig.Title;
            model.Description = Config.PodcastConfig.Description;
            try
            {
                ViewBag.Title = Config.PodcastConfig.Title + " - " + Config.Title;
                ViewBag.Description = Config.PodcastConfig.Description;
                bool editor = User.IsInRole("Podcast");
                var foundPodcasts = db.Podcasts.Where(p => (p.Published || editor)).FirstOrDefault();
                if (foundPodcasts != null)
                {
                    model.HasPodcasts = (foundPodcasts != null);
                }
                else
                {
                    model.Error = true;
                    model.ErrorMessage = "No Podcasts Available";
                }

                return View("~/Areas/Podcast/Views/Podcast/Main.cshtml", model);
            }
            catch (Exception ex)
            {
                model.Error = true;
                model.ErrorMessage = ex.Message;

                return View("~/Areas/Podcast/Views/Podcast/Main.cshtml", model);
            }
        }

        #region Podcasts
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult View(int episode)
        {
            PodcastViewModel model = new PodcastViewModel();
            // find the podcast specified
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                model = new PodcastViewModel(foundPodcast);

                ViewBag.Title = model.Title + " - Teknikast - " + Config.Title;
                return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "No Podcasts Available";
            return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Download(int episode, string fileName)
        {
            // find the podcast specified
            var foundPodcast = db.Podcasts.Where(p => (p.Published && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                PodcastFile file = foundPodcast.Files.Where(f => f.FileName == fileName).FirstOrDefault();
                if (file != null)
                {
                    if (System.IO.File.Exists(file.Path))
                    {
                        // Read in the file
                        byte[] data = System.IO.File.ReadAllBytes(file.Path);

                        // Create File
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = file.FileName,
                            Inline = true
                        };

                        Response.AppendHeader("Content-Disposition", cd.ToString());

                        return File(data, file.ContentType);
                    }
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcasts(int startPodcastID, int count)
        {
            bool editor = User.IsInRole("Podcast");
            var podcasts = db.Podcasts.Where(p => p.Published || editor).OrderByDescending(p => p.DatePosted).Skip(startPodcastID).Take(count).ToList();
            List<PodcastViewModel> podcastViews = new List<PodcastViewModel>();
            if (podcasts != null)
            {
                foreach (Models.Podcast podcast in podcasts)
                {
                    podcastViews.Add(new PodcastViewModel(podcast));
                }
            }
            return PartialView("~/Areas/Podcast/Views/Podcast/Podcasts.cshtml", podcastViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastEpisode(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Episode });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastTitle(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Title });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastDescription(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Description });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastFiles(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                List<object> files = new List<object>();
                foreach (PodcastFile file in foundPodcast.Files)
                {
                    object fileObj = new
                    {
                        name = file.FileName,
                        id = file.PodcastFileId
                    };
                    files.Add(fileObj);
                }
                return Json(new { result = new { files = files } });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        public ActionResult CreatePodcast(int episode, string title, string description)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    // Grab the next episode number
                    Models.Podcast lastPod = db.Podcasts.Where(p => p.Episode == episode).FirstOrDefault();
                    if (lastPod == null)
                    {
                        // Create the podcast object
                        Models.Podcast podcast = db.Podcasts.Create();
                        podcast.Episode = episode;
                        podcast.Title = title;
                        podcast.Description = description;
                        podcast.DatePosted = DateTime.Now;
                        podcast.DatePublished = DateTime.Now;
                        podcast.DateEdited = DateTime.Now;
                        podcast.Files = SaveFiles(Request.Files, episode);

                        db.Podcasts.Add(podcast);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "That episode already exists" });
                }
                return Json(new { error = "You don't have permission to create a podcast" });
            }
            return Json(new { error = "No podcast created" });
        }

        [HttpPost]
        public ActionResult EditPodcast(int podcastId, int episode, string title, string description, string fileIds)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault();
                    if (podcast != null)
                    {
                        if (db.Podcasts.Where(p => p.Episode != episode).FirstOrDefault() == null || podcast.Episode == episode)
                        {
                            podcast.Episode = episode;
                            podcast.Title = title;
                            podcast.Description = description;
                            podcast.DateEdited = DateTime.Now;
                            // Remove any files not in fileIds
                            List<string> fileIdList = new List<string>();
                            if (!string.IsNullOrEmpty(fileIds))
                            {
                                fileIdList = fileIds.Split(',').ToList();
                            }
                            for (int i = 0; i < podcast.Files.Count; i++)
                            {
                                PodcastFile curFile = podcast.Files.ElementAt(i);
                                if (!fileIdList.Exists(id => id == curFile.PodcastFileId.ToString()))
                                {
                                    if (System.IO.File.Exists(curFile.Path))
                                    {
                                        System.IO.File.Delete(curFile.Path);
                                    }
                                    db.PodcastFiles.Remove(curFile);
                                    podcast.Files.Remove(curFile);
                                }
                            }
                            // Add any new files
                            List<PodcastFile> newFiles = SaveFiles(Request.Files, episode);
                            foreach (PodcastFile file in newFiles)
                            {
                                podcast.Files.Add(file);
                            }

                            // Save podcast
                            db.Entry(podcast).State = EntityState.Modified;
                            db.SaveChanges();
                            return Json(new { result = true });
                        }
                        return Json(new { error = "That episode already exists" });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to edit this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }

        [HttpPost]
        public ActionResult PublishPodcast(int podcastId, bool publish)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = db.Podcasts.Find(podcastId);
                    if (podcast != null)
                    {
                        podcast.Published = publish;
                        if (publish)
                            podcast.DatePublished = DateTime.Now;
                        db.Entry(podcast).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to publish this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }

        [HttpPost]
        public ActionResult DeletePodcast(int podcastId)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault();
                    if (podcast != null)
                    {
                        foreach (PodcastFile file in podcast.Files)
                        {
                            System.IO.File.Delete(file.Path);
                        }
                        db.Podcasts.Remove(podcast);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to delete this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }
        #endregion

        #region Comments
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComments(int podcastId, int startCommentID, int count)
        {
            var comments = db.PodcastComments.Where(p => (p.PodcastId == podcastId)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
            List<CommentViewModel> commentViews = new List<CommentViewModel>();
            if (comments != null)
            {
                foreach (PodcastComment comment in comments)
                {
                    commentViews.Add(new CommentViewModel(comment));
                }
            }
            return PartialView("~/Areas/Podcast/Views/Podcast/Comments.cshtml", commentViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetCommentArticle(int commentID)
        {
            PodcastComment comment = db.PodcastComments.Where(p => (p.PodcastCommentId == commentID)).FirstOrDefault();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        public ActionResult CreateComment(int podcastId, string article)
        {
            if (ModelState.IsValid)
            {
                if (db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault() != null)
                {
                    PodcastComment comment = db.PodcastComments.Create();
                    comment.PodcastId = podcastId;
                    comment.UserId = UserHelper.GetUser(db, User.Identity.Name).UserId;
                    comment.Article = article;
                    comment.DatePosted = DateTime.Now;
                    comment.DateEdited = DateTime.Now;

                    db.PodcastComments.Add(comment);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "That podcast does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                PodcastComment comment = db.PodcastComments.Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        comment.Article = article;
                        comment.DateEdited = DateTime.Now;
                        db.Entry(comment).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to edit this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult DeleteComment(int commentID)
        {
            if (ModelState.IsValid)
            {
                PodcastComment comment = db.PodcastComments.Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        db.PodcastComments.Remove(comment);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to delete this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }
        #endregion

        public List<PodcastFile> SaveFiles(HttpFileCollectionBase files, int episode)
        {
            List<PodcastFile> podFiles = new List<PodcastFile>();

            if (files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i]; 
                                                               
                    int fileSize = file.ContentLength;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (!Directory.Exists(Config.PodcastConfig.PodcastDirectory))
                    {
                        Directory.CreateDirectory(Config.PodcastConfig.PodcastDirectory);
                    }
                    string newName = string.Format("Teknikast_Episode_{0}{1}", episode, fileExt);
                    int index = 1;
                    while (System.IO.File.Exists(Path.Combine(Config.PodcastConfig.PodcastDirectory, newName)))
                    {
                        newName = string.Format("Teknikast_Episode_{0} ({1}){2}", episode, index, fileExt);
                        index++;
                    }
                    string fullPath = Path.Combine(Config.PodcastConfig.PodcastDirectory, newName);
                    PodcastFile podFile = new PodcastFile();
                    podFile.Path = fullPath;
                    podFile.FileName = newName;
                    podFile.ContentType = file.ContentType;
                    podFile.ContentLength = file.ContentLength;
                    podFiles.Add(podFile);

                    file.SaveAs(fullPath);
                }
            }

            return podFiles;
        }
    }
}