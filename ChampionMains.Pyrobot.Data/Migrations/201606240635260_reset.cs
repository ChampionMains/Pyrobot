namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Champion",
                c => new
                    {
                        Id = c.Short(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SummonerInfo",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Division = c.Byte(nullable: false),
                        Tier = c.Byte(nullable: false),
                        UpdatedTime = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Summoner", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Summoner",
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
                "dbo.SummonerChampionMastery",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MasteryPoints = c.Int(nullable: false),
                        MasteryLevel = c.Byte(nullable: false),
                        ChampionId = c.Short(nullable: false),
                        SummonerInfoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Champion", t => t.ChampionId, cascadeDelete: true)
                .ForeignKey("dbo.SummonerInfo", t => t.SummonerInfoId, cascadeDelete: true)
                .Index(t => t.ChampionId)
                .Index(t => t.SummonerInfoId);
            
            CreateTable(
                "dbo.SubReddit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 21),
                        ChampionId = c.Short(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Champion", t => t.ChampionId)
                .Index(t => t.ChampionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubReddit", "ChampionId", "dbo.Champion");
            DropForeignKey("dbo.SummonerChampionMastery", "SummonerInfoId", "dbo.SummonerInfo");
            DropForeignKey("dbo.SummonerChampionMastery", "ChampionId", "dbo.Champion");
            DropForeignKey("dbo.SummonerInfo", "Id", "dbo.Summoner");
            DropForeignKey("dbo.Summoner", "UserId", "dbo.Users");
            DropIndex("dbo.SubReddit", new[] { "ChampionId" });
            DropIndex("dbo.SummonerChampionMastery", new[] { "SummonerInfoId" });
            DropIndex("dbo.SummonerChampionMastery", new[] { "ChampionId" });
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.Summoner", new[] { "UserId" });
            DropIndex("dbo.SummonerInfo", new[] { "Id" });
            DropTable("dbo.SubReddit");
            DropTable("dbo.SummonerChampionMastery");
            DropTable("dbo.Users");
            DropTable("dbo.Summoner");
            DropTable("dbo.SummonerInfo");
            DropTable("dbo.Champion");
        }
    }
}
