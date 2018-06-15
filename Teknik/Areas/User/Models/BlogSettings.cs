﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class BlogSettings
    {
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual SecuritySettings SecuritySettings { get; set; }

        public virtual UserSettings UserSettings { get; set; }

        public virtual UploadSettings UploadSettings { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public BlogSettings()
        {
            Title = string.Empty;
            Description = string.Empty;
        }
    }
}
