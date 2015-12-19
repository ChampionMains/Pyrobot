﻿using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Hallam.RedditRankedFlairs.Data;
using Hallam.RedditRankedFlairs.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Hallam.RedditRankedFlairs.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _users;

        public LoginController(IUserService users)
        {
            _users = users;
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "profile");
            }
            return View();
        }

        public ActionResult Login()
        {
            var returnUrl = Url.Action("login-callback", "login");
            return new OAuthRedirectResult(returnUrl);
        }

        [ActionName("login-callback")]
        public async Task<ActionResult> LoginCallback()
        {
            var owin = HttpContext.GetOwinContext();
            var loginInfo = await owin.Authentication.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("index");
            }

            var user = await _users.FindAsync(loginInfo.Login.ProviderKey)
                       ?? await _users.CreateAsync(loginInfo.Login.ProviderKey);

            await SignInAsync(loginInfo.ExternalIdentity, user);
            return RedirectToAction("index");
        }

        private async Task SignInAsync(ClaimsIdentity externalIdentity, User user)
        {
            var owin = HttpContext.GetOwinContext();
            // kill the reddit cookie
            owin.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity =
                await _users.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie, externalIdentity);
            await Task.Run(() => owin.Authentication.SignIn(new AuthenticationProperties {IsPersistent = true}, identity));
        }

        private class OAuthRedirectResult : ActionResult
        {
            private readonly string _returnUrl;

            public OAuthRedirectResult(string returnUrl)
            {
                _returnUrl = returnUrl;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var owin = context.HttpContext.GetOwinContext();
                owin.Authentication.Challenge(new AuthenticationProperties {RedirectUri = _returnUrl}, "Reddit");
            }
        }
    }
}