﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Vault
{
    public class VaultAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Vault";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Vault.Index",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Vault", action = "NewVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.NewVault",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Create",
                 new { controller = "Vault", action = "NewVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.NewVaultFromService",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Create/{type}",
                 new { controller = "Vault", action = "NewVaultFromService" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.EditVault",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Edit/{url}",
                 new { controller = "Vault", action = "EditVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.DeleteVault",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Delete",
                 new { controller = "Vault", action = "DeleteVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.ViewVault",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "v/{id}",
                 new { controller = "Vault", action = "ViewVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.Action",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",
                 new { controller = "Vault", action = "NewVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            // Register style bundles
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/vault", config.CdnHost).Include(
                      "~/Content/mdd_styles.css",
                      "~/Areas/Vault/Content/Vault.css"));

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/vault", config.CdnHost).Include(
                      "~/Scripts/jquery.blockUI.js",
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/MarkdownDeepLib.min.js",
                      "~/Areas/Vault/Scripts/Vault.js"));
        }
    }
}