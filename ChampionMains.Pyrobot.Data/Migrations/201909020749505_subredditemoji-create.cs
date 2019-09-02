namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditemojicreate : DbMigration
    {
        // Add unique nullable index 
        private const string TableNameSubredditEmoji = "dbo.SubredditEmoji";
        private const string IndexName = "IX_UQ_Subreddit_Tier";

        public override void Up()
        {
            CreateTable(
                    TableNameSubredditEmoji,
                    c => new
                    {
                        SubredditId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 24),
                        Tier = c.Byte(nullable: true),
                    })
                .PrimaryKey(t => new {t.SubredditId, t.Name})
                .ForeignKey("dbo.Subreddit", t => t.SubredditId, cascadeDelete: true)
                .Index(t => t.SubredditId);

            Sql(string.Format(@"
                    CREATE UNIQUE NONCLUSTERED INDEX {0}
                    ON {1}({2},{3}) 
                    WHERE {3} IS NOT NULL;",
                    IndexName, TableNameSubredditEmoji, "SubredditId", "Tier"));
        }
        
        public override void Down()
        {
            DropForeignKey(TableNameSubredditEmoji, "SubredditId", "dbo.Subreddit");
            DropIndex(TableNameSubredditEmoji, IndexName);
            DropIndex(TableNameSubredditEmoji, new[] { "SubredditId" });
            DropTable(TableNameSubredditEmoji);
        }
    }
}
