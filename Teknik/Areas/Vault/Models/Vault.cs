﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Vault.Models
{
    public class Vault
    {
        public int VaultId { get; set; }

        public int? UserId { get; set; }

        public virtual Users.Models.User User { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateEdited { get; set; }

        public int Views { get; set; }

        public virtual ICollection<VaultItem> VaultItems { get; set; }

        public Vault()
        {
            Title = string.Empty;
            Description = string.Empty;
            DateCreated = DateTime.Now;
            DateEdited = DateTime.Now;
            Views = 0;
            VaultItems = new List<VaultItem>();
        }
    }
}