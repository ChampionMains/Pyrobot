namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class championidentifierreset : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Champion SET Identifier = '' WHERE Identifier IS NULL");
            AlterColumn("dbo.Champion", "Identifier", c => c.String(nullable: false, maxLength: 21));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Champion", "Identifier", c => c.String(maxLength: 21));
        }
    }
}
