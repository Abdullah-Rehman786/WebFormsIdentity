namespace WebFormsIdentity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnhashedPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "unhashedPassword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "unhashedPassword");
        }
    }
}
