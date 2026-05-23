namespace ContextTests.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Student_extended : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "EnrolledOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Students", "Graduated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "Graduated");
            DropColumn("dbo.Students", "EnrolledOn");
        }
    }
}
