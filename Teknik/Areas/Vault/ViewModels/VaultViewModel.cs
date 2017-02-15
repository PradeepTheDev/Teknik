﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Vault.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class VaultViewModel : ViewModelBase
    {
        public string Url { get; set; }
        public int? UserId { get; set; }
        public Users.Models.User User { get; set; }
        public string CurrentSub { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateEdited { get; set; }
        public List<VaultItemViewModel> Items { get; set; }

        public VaultViewModel()
        {
            CurrentSub = "vault";
            Title = string.Empty;
            Description = string.Empty;
            DateCreated = DateTime.Now;
            DateEdited = DateTime.Now;
            Items = new List<VaultItemViewModel>();
        }
    }
}