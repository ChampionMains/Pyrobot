namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class flairupdatetime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubredditUserFlair", "LastUpdate", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubredditUserFlair", "LastUpdate");
        }
    }
}
