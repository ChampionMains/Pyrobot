namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatetime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Summoner", "LastUpdate", c => c.DateTimeOffset(precision: 7));
            DropColumn("dbo.User", "FlairUpdateRequiredTime");
            DropColumn("dbo.User", "FlairUpdatedTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "FlairUpdatedTime", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.User", "FlairUpdateRequiredTime", c => c.DateTimeOffset(precision: 7));
            DropColumn("dbo.Summoner", "LastUpdate");
        }
    }
}
