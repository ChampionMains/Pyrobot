namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditaddmissingadmin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "MissingAdmin", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "MissingAdmin");
        }
    }
}
