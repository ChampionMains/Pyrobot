namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class useraddbackgroundskinid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "BackgroundSkinId", c => c.Int(nullable: false, defaultValue: 266000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "BackgroundSkinId");
        }
    }
}
