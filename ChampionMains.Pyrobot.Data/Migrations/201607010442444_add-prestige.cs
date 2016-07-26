namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addprestige : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubredditUserFlair", "PrestigeEnabled", c => c.Boolean());
            AddColumn("dbo.Subreddit", "PrestigeEnabled", c => c.Boolean());

            Sql("UPDATE dbo.SubredditUserFlair SET PrestigeEnabled = 0 WHERE PrestigeEnabled IS NULL");
            Sql("UPDATE dbo.Subreddit SET PrestigeEnabled = 1 WHERE PrestigeEnabled IS NULL");

            AlterColumn("dbo.SubredditUserFlair", "PrestigeEnabled", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Subreddit", "PrestigeEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "PrestigeEnabled");
            DropColumn("dbo.SubredditUserFlair", "PrestigeEnabled");
        }
    }
}
