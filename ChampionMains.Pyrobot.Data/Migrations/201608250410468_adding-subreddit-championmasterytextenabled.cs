namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addingsubredditchampionmasterytextenabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "ChampionMasteryTextEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "ChampionMasteryTextEnabled");
        }
    }
}
