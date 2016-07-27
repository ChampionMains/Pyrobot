namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class profileiconprofileiconid : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Summoner", "ProfileIcon", "ProfileIconId");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Summoner", "ProfileIconId", "ProfileIcon");
        }
    }
}
