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

            Sql("UPDATE dbo.SubReddit SET AdminOnly = false WHERE Identifier IS NULL");
            Sql("UPDATE dbo.SubReddit SET RankEnabled = true WHERE Identifier IS NULL");
            Sql("UPDATE dbo.SubReddit SET ChampionMasteryEnabled = true WHERE Identifier IS NULL");

            AlterColumn("dbo.SubReddit", "AdminOnly", c => c.Boolean(nullable: false));
            AlterColumn("dbo.SubReddit", "RankEnabled", c => c.Boolean(nullable: false));
            AlterColumn("dbo.SubReddit", "ChampionMasteryEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubReddit", "ChampionMasteryEnabled");
            DropColumn("dbo.SubReddit", "RankEnabled");
            DropColumn("dbo.SubReddit", "AdminOnly");
        }
    }
}
