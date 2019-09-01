namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class subredditaddlastbulkupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "LastBulkUpdate", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "LastBulkUpdate");
        }
    }
}
