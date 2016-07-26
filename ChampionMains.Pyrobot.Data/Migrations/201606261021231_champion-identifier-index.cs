namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class championidentifierindex : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Champion SET Identifier = Name");
            CreateIndex("dbo.Champion", "Identifier", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Champion", new[] { "Identifier" });
        }
    }
}
