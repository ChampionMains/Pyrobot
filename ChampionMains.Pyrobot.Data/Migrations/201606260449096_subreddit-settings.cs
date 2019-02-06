namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class subredditsettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubReddit", "AdminOnly", c => c.Boolean());
            AddColumn("dbo.SubReddit", "RankEnabled", c => c.Boolean());
            AddColumn("dbo.SubReddit", "ChampionMasteryEnabled", c => c.Boolean());

            Sql(@"
IF EXISTS(SELECT 1 FROM SYS.COLUMNS
	WHERE Name='Identifier'
	AND OBJECT_ID = OBJECT_ID('dbo.SubReddit'))
BEGIN
	EXEC('UPDATE dbo.SubReddit SET AdminOnly = 0, RankEnabled = 1, ChampionMasteryEnabled = 1
    WHERE Identifier IS NULL')
END");

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
