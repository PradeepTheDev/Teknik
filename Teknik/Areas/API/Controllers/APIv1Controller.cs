﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload;
using Teknik.Areas.Paste;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using System.Text;
using Teknik.Areas.Shortener.Models;
using nClam;
using Teknik.Filters;
using Teknik.Areas.API.Models;

namespace Teknik.Areas.API.Controllers
{
    public class APIv1Controller : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.Topic", new { topic = "API" }));
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Upload(APIv1UploadModel model)
        {
            try
            {
                if (model.file != null)
                {
                    if (model.file.ContentLength <= Config.UploadConfig.MaxUploadSize)
                    {
                        // convert file to bytes
                        byte[] fileData = null;
                        string fileExt = Path.GetExtension(model.file.FileName);
                        int contentLength = model.file.ContentLength;
                        using (var binaryReader = new BinaryReader(model.file.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(model.file.ContentLength);
                        }

                        // Scan the file to detect a virus
                        if (Config.UploadConfig.VirusScanEnable)
                        {
                            byte[] scanData = fileData;
                            // If it was encrypted client side, decrypt it
                            if (!model.encrypt && model.key != null)
                            {
                                // If the IV is set, and Key is set, then decrypt it
                                if (!string.IsNullOrEmpty(model.key) && !string.IsNullOrEmpty(model.iv))
                                {
                                    // Decrypt the data
                                    scanData = AES.Decrypt(scanData, model.key, model.iv);
                                }
                            }
                            ClamClient clam = new ClamClient(Config.UploadConfig.ClamServer, Config.UploadConfig.ClamPort);
                            clam.MaxStreamSize = Config.UploadConfig.MaxUploadSize;
                            ClamScanResult scanResult = clam.SendAndScanFile(scanData);

                            switch (scanResult.Result)
                            {
                                case ClamScanResults.Clean:
                                    break;
                                case ClamScanResults.VirusDetected:
                                    return Json(new { error = new { message = string.Format("Virus Detected: {0}. As per our <a href=\"{1}\">Terms of Service</a>, Viruses are not permited.", scanResult.InfectedFiles.First().VirusName, Url.SubRouteUrl("tos", "TOS.Index")) } });
                                case ClamScanResults.Error:
                                    break;
                                case ClamScanResults.Unknown:
                                    break;
                            }
                        }

                        // Need to grab the contentType if it's empty
                        if (string.IsNullOrEmpty(model.contentType))
                        {
                            model.contentType = (string.IsNullOrEmpty(model.file.ContentType)) ? "application/octet-stream" : model.file.ContentType;
                        }

                        // Initialize the key size and block size if empty
                        if (model.keySize <= 0)
                            model.keySize = Config.UploadConfig.KeySize;
                        if (model.blockSize <= 0)
                            model.blockSize = Config.UploadConfig.BlockSize;

                        byte[] data = null;
                        // If they want us to encrypt the file first, do that here
                        if (model.encrypt)
                        {
                            // Generate key and iv if empty
                            if (string.IsNullOrEmpty(model.key))
                            {
                                model.key = StringHelper.RandomString(model.keySize / 8);
                            }
                            if (string.IsNullOrEmpty(model.iv))
                            {
                                model.iv = StringHelper.RandomString(model.blockSize / 8);
                            }

                            data = AES.Encrypt(fileData, model.key, model.iv);
                            if (data == null || data.Length <= 0)
                            {
                                return Json(new { error = new { message = "Unable to encrypt file" } });
                            }
                        }

                        // Save the file data
                        Upload.Models.Upload upload = Uploader.SaveFile(db, Config, (model.encrypt) ? data : fileData, model.contentType, contentLength, fileExt, model.iv, (model.saveKey) ? model.key : null, model.keySize, model.blockSize);

                        if (upload != null)
                        {
                            // Generate delete key if asked to
                            if (model.genDeletionKey)
                            {
                                string delKey = StringHelper.RandomString(Config.UploadConfig.DeleteKeyLength);
                                upload.DeleteKey = delKey;
                                db.Entry(upload).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            // Pull all the information together 
                            string fullUrl = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url });
                            var returnData = new
                            {
                                url = (model.saveKey || string.IsNullOrEmpty(model.key)) ? fullUrl : fullUrl + "#" + model.key,
                                fileName = upload.Url,
                                contentType = model.contentType,
                                contentLength = contentLength,
                                key = model.key,
                                keySize = model.keySize,
                                iv = model.iv,
                                blockSize = model.blockSize,
                                deletionKey = upload.DeleteKey

                            };
                            return Json(new { result = returnData });
                        }
                        return Json(new { error = new { message = "Unable to save file" } });
                    }
                    else
                    {
                        return Json(new { error = new { message = "File Too Large" } });
                    }
                }
                return Json(new { error = new { message = "Invalid Upload Request" } });
            }
            catch(Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Paste(APIv1PasteModel model)
        {
            try
            {
                if (model != null && model.code != null)
                {
                    Paste.Models.Paste paste = PasteHelper.CreatePaste(model.code, model.title, model.syntax, model.expireUnit, model.expireLength, model.password, model.hide);

                    db.Pastes.Add(paste);
                    db.SaveChanges();

                    return Json(new
                    {
                        result = new
                        {
                            id = paste.Url,
                            url = Url.SubRouteUrl("paste", "Paste.View", new { type = "Full", url = paste.Url, password = model.password }),
                            title = paste.Title,
                            syntax = paste.Syntax,
                            expiration = paste.ExpireDate,
                            password = model.password
                        }
                    });
                }
                return Json(new { error = new { message = "Invalid Paste Request" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Shorten(APIv1ShortenModel model)
        {
            try
            {
                if (model.url.IsValidUrl())
                {
                    ShortenedUrl newUrl = Shortener.Shortener.ShortenUrl(model.url, Config.ShortenerConfig.UrlLength);

                    db.ShortenedUrls.Add(newUrl);
                    db.SaveChanges();

                    string shortUrl = string.Format("{0}://{1}/{2}", HttpContext.Request.Url.Scheme, Config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                    if (Config.DevEnvironment)
                    {
                        shortUrl = Url.SubRouteUrl("shortened", "Shortener.View", new { url = newUrl.ShortUrl });
                    }

                    return Json(new
                    {
                        result = new
                        {
                            shortUrl = shortUrl,
                            originalUrl = model.url
                        }
                    });
                }
                return Json(new { error = new { message = "Must be a valid Url" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }
    }
}