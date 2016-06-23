using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using ChampionMains.Pyrobot.Data;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace ChampionMains.Pyrobot
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Dictionary<Type, Action<Exception, StringWriter>> ExceptionFormatterDictionary = new Dictionary<Type, Action<Exception, StringWriter>>();
        private static List<Func<Exception, bool>> ExceptionFilters = new List<Func<Exception, bool>>(); 

        static MvcApplication()
        {
            ExceptionFilters.Add(exception =>
            {
                if (exception is HttpException)
                {
                    return (exception as HttpException).GetHttpCode() == 404;
                }
                return false;
            });

            ExceptionFormatterDictionary[typeof (HttpException)] = (exception, writer) =>
            {
                var e = exception as HttpException;
                writer.WriteLine("HTTP Exception -------");
                writer.WriteLine("Status Code: " + e.GetHttpCode());
                writer.WriteLine("HTML Error:  " + e.GetHtmlErrorMessage());
                writer.WriteLine("----------------------");
            };

            ExceptionFormatterDictionary[typeof (DbEntityValidationException)] = (exception, writer) =>
            {
                var e = exception as DbEntityValidationException;
                writer.WriteLine("DbEntityValidationException ------------");
                foreach (var entity in e.EntityValidationErrors)
                {
                    writer.WriteLine("  " + entity.Entry.Entity.ToString() + ":");
                    foreach (var error in entity.ValidationErrors)
                    {
                        writer.WriteLine("    " + error.PropertyName + ": " + error.ErrorMessage);
                    }
                }
                writer.WriteLine("----------------------------------------");
            };
        }

        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UnitOfWork, ChampionMains.Pyrobot.Data.Migrations.Configuration>());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AutofacConfig.Register(new ContainerBuilder());
        }

        protected void Application_Error()
        {
            try
            {
                var error = Server.GetLastError();

                foreach (var filter in ExceptionFilters)
                {
                    if (filter(error))
                    {
                        return;
                    }
                }

                var body = new StringBuilder();
                var writer = new StringWriter(body);

                if (ExceptionFormatterDictionary.ContainsKey(error.GetType()))
                {
                    ExceptionFormatterDictionary[error.GetType()](error, writer);
                    writer.WriteLine();
                }

                var context = HttpContext.Current;

                writer.WriteLine("HTTP Context -------------");
                writer.WriteLine("HTTP Method: " + context.Request.HttpMethod);
                writer.WriteLine("Url: " + context.Request.Url);
                writer.WriteLine("Is Authenticated: " + context.User.Identity.IsAuthenticated);
                if (context.User.Identity.IsAuthenticated)
                {
                    writer.WriteLine("User: " + context.User.Identity.Name);
                }
                writer.WriteLine("--------------------------");

                writer.WriteLine(error.ToString());
                writer.Flush();

                var mailClient = new SmtpClient();
                var mailMessage = new MailMessage();
                mailMessage.To.Add("flairbugs@gmail.com");
                mailMessage.From = new MailAddress("flairsite@jhallam.ca");
                mailMessage.Subject = error.GetType().Name;
                mailMessage.Body = body.ToString();
                mailClient.Send(mailMessage);
            }
            catch { }
        }
    }
}
