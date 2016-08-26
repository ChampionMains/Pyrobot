using System.Configuration;
using ChampionMains.Pyrobot;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Reddit;

[assembly: OwinStartup(typeof (OwinStartup))]
namespace ChampionMains.Pyrobot
{
    public static class OwinStartup
    {

        public static void Configuration(IAppBuilder app)
        {
            //Enable the application to use a cookie to store information for the signed in user.
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