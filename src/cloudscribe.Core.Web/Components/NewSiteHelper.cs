﻿// Author:					Joe Audette
// Created:				    2014-11-24
// Last Modified:		    2014-12-04


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using cloudscribe.Core.Models;
using cloudscribe.Configuration;
using cloudscribe.AspNet.Identity;
using Microsoft.AspNet.Identity;

namespace cloudscribe.Core.Web.Components
{
    public class NewSiteHelper
    {
        public static SiteSettings CreateNewSite(ISiteRepository siteRepo)
        {
            //string templateFolderPath = GetMessageTemplateFolder();
            //string templateFolder = templateFolderPath;

            SiteSettings newSite = new SiteSettings();
            newSite.SiteName = "Sample Site";

            CreateNewSite(siteRepo, newSite);

            return newSite;


        }

        public static void CreateNewSite(ISiteRepository siteRepo, ISiteSettings newSite)
        {
            if (siteRepo == null) { throw new ArgumentNullException("you must pass in an instance of ISiteRepository"); }
            if (newSite == null) { throw new ArgumentNullException("you must pass in an instance of ISiteSettings"); }
            if (newSite.SiteGuid != Guid.Empty) { throw new ArgumentException("newSite should not already have a site guid"); }

            //string templateFolderPath = GetMessageTemplateFolder();
            //string templateFolder = templateFolderPath;

            //SiteSettings newSite = new SiteSettings();

            
            newSite.Skin = AppSettings.DefaultInitialSkin;

            //newSite.Logo = GetMessageTemplate(templateFolder, "InitialSiteLogoContent.config");

            newSite.AllowHideMenuOnPages = false;
            newSite.AllowNewRegistration = true;
            newSite.AllowPageSkins = false;
            newSite.AllowUserFullNameChange = false;
            newSite.AllowUserSkins = false;
            newSite.AutoCreateLdapUserOnFirstLogin = true;
            //newSite.DefaultFriendlyUrlPattern = SiteSettings.FriendlyUrlPattern.PageNameWithDotASPX;
            //newSite.EditorSkin = SiteSettings.ContentEditorSkin.normal;
            //newSite.EncryptPasswords = false;
            newSite.Icon = String.Empty;
            newSite.IsServerAdminSite = true;
            newSite.ReallyDeleteUsers = true;
            newSite.SiteLdapSettings.Port = 389;
            newSite.SiteLdapSettings.RootDN = String.Empty;
            newSite.SiteLdapSettings.Server = String.Empty;
            newSite.UseEmailForLogin = true;
            newSite.UseLdapAuth = false;
            newSite.UseSecureRegistration = false;
            newSite.UseSslOnAllPages = AppSettings.SslIsRequiredByWebServer;
            //newSite.CreateInitialDataOnCreate = false;

            newSite.AllowPasswordReset = true;
            newSite.AllowPasswordRetrieval = true;
            //0 = clear, 1= hashed, 2= encrypted
            newSite.PasswordFormat = 1;

            newSite.RequiresQuestionAndAnswer = true;
            newSite.MaxInvalidPasswordAttempts = 10;
            newSite.PasswordAttemptWindowMinutes = 5;
            //newSite.RequiresUniqueEmail = true;
            newSite.MinRequiredNonAlphanumericCharacters = 0;
            newSite.MinRequiredPasswordLength = 7;
            newSite.PasswordStrengthRegularExpression = String.Empty;
            //newSite.DefaultEmailFromAddress = GetMessageTemplate(templateFolder, "InitialEmailFromContent.config");

            siteRepo.Save(newSite);





        }

        public static void CreateRequiredRolesAndAdminUser(
            SiteSettings site,
            ISiteRepository siteRepository,
            IUserRepository userRepository)
        {

            SiteRole adminRole = new SiteRole();
            adminRole.DisplayName = "Admins";
            //adminRole.DisplayName = "Administrators";
            adminRole.SiteId = site.SiteId;
            adminRole.SiteGuid = site.SiteGuid;
            userRepository.SaveRole(adminRole);
            adminRole.DisplayName = "Administrators";
            userRepository.SaveRole(adminRole);

            SiteRole roleAdminRole = new SiteRole();
            roleAdminRole.RoleName = "Role Admins";
            roleAdminRole.SiteId = site.SiteId;
            roleAdminRole.SiteGuid = site.SiteGuid;
            userRepository.SaveRole(roleAdminRole);

            roleAdminRole.DisplayName = "Role Administrators";
            userRepository.SaveRole(roleAdminRole);

            SiteRole contentAdminRole = new SiteRole();
            contentAdminRole.DisplayName = "Content Administrators";
            contentAdminRole.SiteId = site.SiteId;
            contentAdminRole.SiteGuid = site.SiteGuid;
            userRepository.SaveRole(contentAdminRole);

            SiteRole authenticatedUserRole = new SiteRole();
            authenticatedUserRole.DisplayName = "Authenticated Users";
            authenticatedUserRole.SiteId = site.SiteId;
            authenticatedUserRole.SiteGuid = site.SiteGuid;
            userRepository.SaveRole(authenticatedUserRole);

            //SiteRole newsletterAdminRole = new SiteRole();
            //newsletterAdminRole.DisplayName = "Newsletter Administrators";
            //newsletterAdminRole.SiteId = site.SiteId;
            //newsletterAdminRole.SiteGuid = site.SiteGuid;
            //userRepository.SaveRole(newsletterAdminRole);

            // if using related sites mode there is a problem if we already have user admin@admin.com
            // and we create another one in the child site with the same email and login so we need to make it different
            // we could just skip creating this user since in related sites mode all users come from the first site
            // but then if the config were changed to not related sites mode there would be no admin user
            // so in related sites mode we create one only as a backup in case settings are changed later
            int countOfSites = siteRepository.GetCount();
            string siteDifferentiator = string.Empty;
            if (
                (countOfSites >= 1)
                && (AppSettings.UseRelatedSiteMode)
                )
            {
                if (site.SiteId > 1)
                {
                    siteDifferentiator = site.SiteId.ToInvariantString();
                }
            }

            //mojoMembershipProvider membership = Membership.Provider as mojoMembershipProvider;
            //bool overridRelatedSiteMode = true;
            SiteUser adminUser = new SiteUser();
            adminUser.SiteId = site.SiteId;
            adminUser.SiteGuid = site.SiteGuid;
            adminUser.Email = "admin" + siteDifferentiator + "@admin.com";
            adminUser.DisplayName = "Admin";
            adminUser.UserName = "admin" + siteDifferentiator;

            adminUser.EmailConfirmed = true;
            adminUser.ApprovedForLogin = true;
            adminUser.Password = "admin";
            adminUser.PasswordFormat = 0;

            //if (membership != null)
            //{
            //    adminUser.Password = membership.EncodePassword(site, adminUser, "admin");
            //}

            adminUser.PasswordQuestion = "What is your user name?";
            adminUser.PasswordAnswer = "admin";

            userRepository.Save(adminUser);

            //siteUserManager.AddPassword(adminUser.UserGuid.ToString(), "admin");

            //siteUserManager.Create(adminUser, "admin");
            //var result = siteUserManager.CreateAsync(adminUser, "admin");
            //if (result.Succeeded)
            //{
            //}
            userRepository.AddUserToRole(adminRole.RoleId, adminRole.RoleGuid, adminUser.UserId, adminUser.UserGuid);



        }

    }
}