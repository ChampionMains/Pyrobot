namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class renamesubredditusersubreddituserflair : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SubRedditUser", newName: "SubredditUserFlair");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.SubredditUserFlair", newName: "SubRedditUser");
        }
    }
}
