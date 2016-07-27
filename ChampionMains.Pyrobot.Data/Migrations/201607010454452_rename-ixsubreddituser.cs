namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class renameixsubreddituser : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.SubredditUserFlair", name: "IX_SubReddit_User", newName: "IX_Subreddit_User");
        }

        public override void Down()
        {
            RenameIndex(table: "dbo.SubredditUserFlair", name: "IX_Subreddit_User", newName: "IX_SubReddit_User");
        }
    }
}
