﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class ResetPasswordVerification
    {
        public int ResetPasswordVerificationId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        [CaseSensitive]
        public string Code { get; set; }

        public DateTime DateCreated { get; set; }
    }
}