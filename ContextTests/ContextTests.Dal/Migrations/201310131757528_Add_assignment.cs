namespace ContextTests.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_assignment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assignments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Assignments");
        }
    }
}
