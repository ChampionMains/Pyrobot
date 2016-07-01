namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamesubreddituserflairfksubredditid : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.SubredditUserFlair", name: "SubRedditId", newName: "SubredditId");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.SubredditUserFlair", name: "SubredditId", newName: "SubRedditId");
        }
    }
}
