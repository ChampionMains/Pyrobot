namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// Migration for new tiers, Iron and Grandmaster.
    /// Old tiers multiplied by 10, Iron = 5, Grandmaster = 65.
    /// </summary>
    public partial class updatesummonerrankenum : DbMigration
    {
        public override void Up()
        {
            Sql(@"
UPDATE SummonerRank
SET Tier = 10 * Tier");
        }
        
        public override void Down()
        {
            Sql(@"
UPDATE SummonerRank
SET Tier = (
    CASE Tier
		WHEN  5 THEN 1 -- Iron -> Bronze
		WHEN 65 THEN 6 -- Grandmaster -> Master
		ELSE Tier / 10
	END
);");
        }
    }
}
