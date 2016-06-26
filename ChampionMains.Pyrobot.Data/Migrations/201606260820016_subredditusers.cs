namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditusers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SubRedditUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RankEnabled = c.Boolean(nullable: false),
                        ChampionMasteryEnabled = c.Boolean(nullable: false),
                        SubRedditId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubReddit", t => t.SubRedditId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.SubRedditId, t.UserId }, unique: true, name: "IX_SubReddit_User");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubRedditUser", "UserId", "dbo.Users");
            DropForeignKey("dbo.SubRedditUser", "SubRedditId", "dbo.SubReddit");
            DropIndex("dbo.SubRedditUser", "IX_SubReddit_User");
            DropTable("dbo.SubRedditUser");
        }
    }
}
