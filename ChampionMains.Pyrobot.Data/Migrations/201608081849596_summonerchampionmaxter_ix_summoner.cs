namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class summonerchampionmaxter_ix_summoner : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.SummonerChampionMastery", "SummonerId", name: "IX_Summoner");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SummonerChampionMastery", "IX_Summoner");
        }
    }
}
