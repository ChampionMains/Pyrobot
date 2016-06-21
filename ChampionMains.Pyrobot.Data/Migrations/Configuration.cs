using System.Data.Entity.Infrastructure;

namespace Hallam.RedditRankedFlairs.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Hallam.RedditRankedFlairs.Data.UnitOfWork>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            TargetDatabase = new DbConnectionInfo(
                "Data Source=grass.arvixe.com; Initial Catalog=RedditRankedFlairs; User Id=RedditRankedFlairs; Password=qJwt*BhoWuX@7b@0", "System.Data.SqlClient");
        }

        protected override void Seed(Hallam.RedditRankedFlairs.Data.UnitOfWork context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
