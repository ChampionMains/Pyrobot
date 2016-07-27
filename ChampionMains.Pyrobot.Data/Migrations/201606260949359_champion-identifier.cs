namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class championidentifier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Champion", "Identifier", c => c.String(maxLength: 21));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Champion", "Identifier");
        }
    }
}
