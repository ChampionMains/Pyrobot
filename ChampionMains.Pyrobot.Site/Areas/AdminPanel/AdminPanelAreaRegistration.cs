using System.Web.Mvc;

namespace ChampionMains.Pyrobot.Areas.AdminPanel
{
    public class AdminPanelAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "AdminPanel";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //context.MapRoute("AdminPanel_Default", "adminPanel/{action}",
            //    new {controller = "AdminDefault", action = "index"});
        }
    }
}