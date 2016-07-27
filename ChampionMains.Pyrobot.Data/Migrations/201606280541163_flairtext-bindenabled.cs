namespace ChampionMains.Pyrobot.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class flairtextbindenabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubReddit", "BindEnabled", c => c.Boolean());
            Sql("UPDATE dbo.SubReddit SET BindEnabled = 0 WHERE BindEnabled IS NULL");
            AlterColumn("dbo.SubReddit", "BindEnabled", c => c.Boolean(nullable: false));

            AddColumn("dbo.SubRedditUser", "FlairText", c => c.String(maxLength: 64));
        }

        public override void Down()
        {
            DropColumn("dbo.SubReddit", "BindEnabled");
            DropColumn("dbo.SubRedditUser", "FlairText");
        }
    }
}
