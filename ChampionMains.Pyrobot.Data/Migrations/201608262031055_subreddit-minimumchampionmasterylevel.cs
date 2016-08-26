namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class subredditminimumchampionmasterylevel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "MinimumChampionMasteryLevel", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "MinimumChampionMasteryLevel");
        }
    }
}
