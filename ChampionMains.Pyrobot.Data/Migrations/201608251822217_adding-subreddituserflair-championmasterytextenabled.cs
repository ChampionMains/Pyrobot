namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addingsubreddituserflairchampionmasterytextenabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubredditUserFlair", "ChampionMasteryTextEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubredditUserFlair", "ChampionMasteryTextEnabled");
        }
    }
}
