namespace DXWebApplication4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DonHangs",
                c => new
                    {
                        IDDH = c.Int(nullable: false, identity: true),
                        NgayDat = c.DateTime(nullable: false),
                        KhachHang = c.String(),
                    })
                .PrimaryKey(t => t.IDDH);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DonHangs");
        }
    }
}
