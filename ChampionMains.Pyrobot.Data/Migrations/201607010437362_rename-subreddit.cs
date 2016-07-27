namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class renamesubreddit : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SubReddit", newName: "Subreddit");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Subreddit", newName: "SubReddit");
        }
    }
}
