namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class summoneraddencryptedsummonerid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Summoner", "SummonerIdEnc", c => c.String(maxLength: 80));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Summoner", "SummonerIdEnc");
        }
    }
}
