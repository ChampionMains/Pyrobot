namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditrenamemissingadmintomissingmod : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Subreddit", "MissingAdmin", "MissingMod");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Subreddit", "MissingMod", "MissingAdmin");
        }
    }
}
