namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removesummonerchampionmasteryupdatedtime : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SummonerChampionMastery", "UpdatedTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SummonerChampionMastery", "UpdatedTime", c => c.DateTimeOffset(precision: 7));
        }
    }
}
