namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subredditchampionrequired : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.SubReddit SET ChampionId = 1 WHERE ChampionId IS NULL");

            DropForeignKey("dbo.SubReddit", "ChampionId", "dbo.Champion");
            DropIndex("dbo.SubReddit", new[] { "ChampionId" });
            AlterColumn("dbo.SubReddit", "ChampionId", c => c.Short(nullable: false));
            CreateIndex("dbo.SubReddit", "ChampionId");
            AddForeignKey("dbo.SubReddit", "ChampionId", "dbo.Champion", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubReddit", "ChampionId", "dbo.Champion");
            DropIndex("dbo.SubReddit", new[] { "ChampionId" });
            AlterColumn("dbo.SubReddit", "ChampionId", c => c.Short());
            CreateIndex("dbo.SubReddit", "ChampionId");
            AddForeignKey("dbo.SubReddit", "ChampionId", "dbo.Champion", "Id");
        }
    }
}
