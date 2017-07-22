namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditaddflaircss3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "FlairCss", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "FlairCss");
        }
    }
}
