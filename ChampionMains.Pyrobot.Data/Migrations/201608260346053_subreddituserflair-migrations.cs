namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class subreddituserflairmigrations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subreddit", "TextDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subreddit", "TextDisabled");
        }
    }
}
