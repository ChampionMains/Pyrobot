namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUrl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SummonerInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Division = c.Byte(nullable: false),
                        Tier = c.Byte(nullable: false),
                        UpdatedTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Summoners", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Summoners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 21),
                        Region = c.String(nullable: false, maxLength: 5),
                        SummonerId = c.Long(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FlairUpdateRequiredTime = c.DateTimeOffset(precision: 7),
                        FlairUpdatedTime = c.DateTimeOffset(precision: 7),
                        IsBanned = c.Boolean(nullable: false),
                        IsAdmin = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 21),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.SummonerChampionMasteries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MasteryPoints = c.Int(nullable: false),
                        MasteryLevel = c.Byte(nullable: false),
                        ChampionId = c.Short(nullable: false),
                        SummonerInfoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Champions", t => t.ChampionId, cascadeDelete: true)
                .ForeignKey("dbo.SummonerInfoes", t => t.SummonerInfoId, cascadeDelete: true)
                .Index(t => t.ChampionId)
                .Index(t => t.SummonerInfoId);
            
            CreateTable(
                "dbo.Champions",
                c => new
                    {
                        Id = c.Short(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SubReddits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 21),
                        ChampionId = c.Short(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Champions", t => t.ChampionId)
                .Index(t => t.ChampionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubReddits", "ChampionId", "dbo.Champions");
            DropForeignKey("dbo.SummonerChampionMasteries", "SummonerInfoId", "dbo.SummonerInfoes");
            DropForeignKey("dbo.SummonerChampionMasteries", "ChampionId", "dbo.Champions");
            DropForeignKey("dbo.SummonerInfoes", "Id", "dbo.Summoners");
            DropForeignKey("dbo.Summoners", "UserId", "dbo.Users");
            DropIndex("dbo.SubReddits", new[] { "ChampionId" });
            DropIndex("dbo.SummonerChampionMasteries", new[] { "SummonerInfoId" });
            DropIndex("dbo.SummonerChampionMasteries", new[] { "ChampionId" });
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.Summoners", new[] { "UserId" });
            DropIndex("dbo.SummonerInfoes", new[] { "Id" });
            DropTable("dbo.SubReddits");
            DropTable("dbo.Champions");
            DropTable("dbo.SummonerChampionMasteries");
            DropTable("dbo.Users");
            DropTable("dbo.Summoners");
            DropTable("dbo.SummonerInfoes");
        }
    }
}
