﻿using Microsoft.Win32;
using MimeDetective;
using MimeDetective.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public static class FileHelper
    {
        public static string GenerateRandomFileName(string path, string extension, int length)
        {
            if (Directory.Exists(path))
            {
                string filename = StringHelper.RandomString(length);
                string subDir = filename[0].ToString();
                path = Path.Combine(path, subDir);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                while (File.Exists(Path.Combine(path, string.Format("{0}.{1}", filename, extension))))
                {
                    filename = StringHelper.RandomString(length);
                    subDir = filename[0].ToString();
                    path = Path.Combine(path, subDir);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                return Path.Combine(path, string.Format("{0}.{1}", filename, extension));
            }

            return string.Empty;
        }

        /// <summary>
        /// Append a number to the end of a file name to make it unique, incrementing the number if needed
        /// </summary>
        /// <param name="file">the name of the file, including extension</param>
        /// <param name="destinationFolder">the destination folder in which to check for uniqueness</param>
        /// <returns>the file name, without folder, with extension, with a unique index as the suffix, if necessary</returns>
        public static string MakeUniqueFilename(string file, string destinationFolder)
        {
            string fileName = string.Empty;
            try
            {
                string fileBase = Path.GetFileNameWithoutExtension(file);
                string fileExt = Path.GetExtension(file);
                fileName = string.Format("{0}{1}", fileBase, fileExt);
                string filePath = Path.Combine(destinationFolder, fileName);

                int iteration = 1;
                while (File.Exists(filePath))
                {
                    fileName = string.Format("{0} ({1}){2}", fileBase, iteration, fileExt);
                    filePath = Path.Combine(destinationFolder, fileName);
                    iteration++;
                }
            }
            catch (Exception ex)
            {
                // Throw up exception to parent
                throw new Exception(string.Format("Exception occured in FileHelper.MakeUniqueFilename({0}, {1})", file, destinationFolder), ex);
            }

            return fileName;
        }

        public static string GetDefaultExtension(string mimeType)
        {
            return GetDefaultExtension(mimeType, string.Empty);
        }

        public static string GetDefaultExtension(string mimeType, string defaultExtension)
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : defaultExtension;

            return result;
        }

        public static string GetDefaultExtension(Stream fileStream, string defaultExtension)
        {
            FileType fileType = fileStream.GetFileType();
            if (!string.IsNullOrEmpty(fileType.Extension))
                return fileType.Extension;
            return defaultExtension;
        }
    }
}
