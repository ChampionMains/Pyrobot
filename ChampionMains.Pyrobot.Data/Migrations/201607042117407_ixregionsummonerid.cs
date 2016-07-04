namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ixregionsummonerid : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Summoner", new[] { "Region", "SummonerId" }, unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Summoner", new[] { "Region", "SummonerId" });
        }
    }
}
