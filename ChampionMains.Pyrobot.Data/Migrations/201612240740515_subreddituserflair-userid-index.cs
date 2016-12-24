namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subreddituserflairuseridindex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.SubredditUserFlair", "UserId", name: "IX_User");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SubredditUserFlair", "IX_User");
        }
    }
}
