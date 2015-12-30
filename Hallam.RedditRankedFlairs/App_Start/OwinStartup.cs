﻿using System;
using System.Configuration;
using System.Security.Claims;
using System.Web.Http;
using Hallam.RedditRankedFlairs;
using Hallam.RedditRankedFlairs.Jobs;
using Hallam.RedditRankedFlairs.Security;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Reddit;
using GlobalConfiguration = Hangfire.GlobalConfiguration;

[assembly: OwinStartup(typeof (OwinStartup))]

namespace Hallam.RedditRankedFlairs
{
    public static class OwinStartup
    {
        private const string FlairJobId = "$FlairJob";

        public static void Configuration(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/")
            });

            // Enable the application to use a cookie to store information about a user logging
            // in with a third party login provider. 
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enable the Reddit authentication provider.
            app.UseRedditAuthentication(GetRedditOptions());

            GlobalConfiguration.Configuration.UseSqlServerStorage("Hangfire");
            app.UseHangfireDashboard("/Hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] {new HangfireDashboardAuthorizationFilter(),}
            });
            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<BulkFlairUpdateJob>(FlairJobId, job => job.Execute(), Cron.Minutely);
        }

        private static RedditAuthenticationOptions GetRedditOptions()
        {
            var options = new RedditAuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["reddit.auth.clientId"],
                ClientSecret = ConfigurationManager.AppSettings["reddit.auth.clientSecret"]
            };
            options.Scope.Clear();
            options.Scope.Add("identity");
            options.Scope.Add("mysubreddits");
            return options;
        }
    }
}