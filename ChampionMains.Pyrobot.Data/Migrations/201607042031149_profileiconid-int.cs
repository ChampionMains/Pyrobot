namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class profileiconidint : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Summoner", "ProfileIconId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Summoner", "ProfileIconId", c => c.Short(nullable: false));
        }
    }
}
