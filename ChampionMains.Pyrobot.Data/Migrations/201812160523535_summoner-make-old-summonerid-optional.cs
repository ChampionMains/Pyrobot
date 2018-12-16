namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class summonermakeoldsummoneridoptional : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Summoner", new[] { "Region", "SummonerId" });
            AlterColumn("dbo.Summoner", "SummonerId", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Summoner", "SummonerId", c => c.Long(nullable: false));
            CreateIndex("dbo.Summoner", new[] { "Region", "SummonerId" }, unique: true);
        }
    }
}
