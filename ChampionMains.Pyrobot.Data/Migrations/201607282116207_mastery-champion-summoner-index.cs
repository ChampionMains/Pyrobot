namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class masterychampionsummonerindex : DbMigration
    {
        public override void Up()
        {
            Sql(@"
DELETE SummonerChampionMastery
FROM SummonerChampionMastery
LEFT OUTER JOIN (
   SELECT MIN(Id) as Id, ChampionId, SummonerId
   FROM SummonerChampionMastery 
   GROUP BY ChampionId, SummonerId
) as KeepRows ON
   SummonerChampionMastery.Id = KeepRows.Id
WHERE
   KeepRows.Id IS NULL");
            DropIndex("dbo.SummonerChampionMastery", new[] { "ChampionId" });
            DropIndex("dbo.SummonerChampionMastery", new[] { "SummonerId" });
            CreateIndex("dbo.SummonerChampionMastery", new[] { "ChampionId", "SummonerId" }, unique: true, name: "IX_Champion_Summoner");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SummonerChampionMastery", "IX_Champion_Summoner");
            CreateIndex("dbo.SummonerChampionMastery", "SummonerId");
            CreateIndex("dbo.SummonerChampionMastery", "ChampionId");
        }
    }
}
