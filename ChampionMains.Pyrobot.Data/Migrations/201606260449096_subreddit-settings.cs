namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditsettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubReddit", "AdminOnly", c => c.Boolean());
            AddColumn("dbo.SubReddit", "RankEnabled", c => c.Boolean());
            AddColumn("dbo.SubReddit", "ChampionMasteryEnabled", c => c.Boolean());

            AlterColumn("dbo.SubReddit", "AdminOnly", c => c.Boolean(nullable: false, defaultValue: false));
            AlterColumn("dbo.SubReddit", "RankEnabled", c => c.Boolean(nullable: false, defaultValue: true));
            AlterColumn("dbo.SubReddit", "ChampionMasteryEnabled", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubReddit", "ChampionMasteryEnabled");
            DropColumn("dbo.SubReddit", "RankEnabled");
            DropColumn("dbo.SubReddit", "AdminOnly");
        }
    }
}
