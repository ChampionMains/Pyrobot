namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamesubredditusersubredditfk : DbMigration
    {
        public override void Up()
        {
            //FK_dbo.SubRedditUser_dbo.SubReddit_SubRedditId
            //RenameColumn(table: "dbo.SubredditUserFlair", name: "FK_dbo.SubRedditUser_dbo.SubReddit_SubRedditId", newName: "FK_dbo.SubredditUserFlair_dbo.Subreddit_SubredditId");
            Sql("EXECUTE sp_rename '[FK_dbo.SubRedditUser_dbo.SubReddit_SubRedditId]', 'FK_dbo.SubredditUser_dbo.Subreddit_SubredditId', 'OBJECT'");
        }

        public override void Down()
        {
            Sql("EXECUTE sp_rename '[FK_dbo.SubredditUser_dbo.Subreddit_SubredditId]', 'FK_dbo.SubRedditUser_dbo.SubReddit_SubRedditId', 'OBJECT'");
        }
    }
}
