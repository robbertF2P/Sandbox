namespace Datalayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Modules : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Contracts", "Name", c => c.String());
            AddColumn("dbo.Contracts", "SomeProperty", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Contracts", "SomeProperty");
            DropColumn("dbo.Contracts", "Name");
            DropTable("dbo.Currencies");
        }
    }
}
