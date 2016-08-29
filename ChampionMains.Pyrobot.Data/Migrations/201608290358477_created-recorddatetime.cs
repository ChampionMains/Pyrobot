namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class createdrecorddatetime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Summoner", "Created", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.User", "Created", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.SubredditUserFlair", "Created", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.Subreddit", "Created", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "GETUTCDATE()"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "Created");
            DropColumn("dbo.SubredditUserFlair", "Created");
            DropColumn("dbo.User", "Created");
            DropColumn("dbo.Summoner", "Created");
        }
    }
}
