namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class profileicon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Summoner", "ProfileIcon", c => c.Short());
            Sql("UPDATE dbo.Summoner SET ProfileIcon = 0 WHERE ProfileIcon IS NULL");
            AlterColumn("dbo.Summoner", "ProfileIcon", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Summoner", "ProfileIcon");
        }
    }
}
