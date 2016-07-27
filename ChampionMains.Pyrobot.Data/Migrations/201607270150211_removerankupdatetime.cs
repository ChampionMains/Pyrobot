namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removerankupdatetime : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SummonerRank", "UpdatedTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SummonerRank", "UpdatedTime", c => c.DateTimeOffset(precision: 7));
        }
    }
}
