namespace DXWebApplication4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CTDHs",
                c => new
                    {
                        IDCTDH = c.Int(nullable: false, identity: true),
                        IDDH = c.Int(nullable: false),
                        IDVariant = c.Int(nullable: false),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IDCTDH)
                .ForeignKey("dbo.DonHangs", t => t.IDDH, cascadeDelete: true)
                .ForeignKey("dbo.ProductVariants", t => t.IDVariant, cascadeDelete: true)
                .Index(t => t.IDDH)
                .Index(t => t.IDVariant);
            
            CreateTable(
                "dbo.ProductVariants",
                c => new
                    {
                        IDVariant = c.Int(nullable: false, identity: true),
                        IDPro = c.Int(nullable: false),
                        IDSize = c.Int(nullable: false),
                        IDColor = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IDVariant)
                .ForeignKey("dbo.Colors", t => t.IDColor, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.IDPro, cascadeDelete: true)
                .ForeignKey("dbo.Sizes", t => t.IDSize, cascadeDelete: true)
                .Index(t => t.IDPro)
                .Index(t => t.IDSize)
                .Index(t => t.IDColor);
            
            CreateTable(
                "dbo.Colors",
                c => new
                    {
                        IDColor = c.Int(nullable: false, identity: true),
                        TenColor = c.String(),
                    })
                .PrimaryKey(t => t.IDColor);
            
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        IDImg = c.Int(nullable: false, identity: true),
                        URL = c.String(),
                        IDVariant = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IDImg)
                .ForeignKey("dbo.ProductVariants", t => t.IDVariant, cascadeDelete: true)
                .Index(t => t.IDVariant);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        IDPro = c.Int(nullable: false, identity: true),
                        TenPro = c.String(),
                    })
                .PrimaryKey(t => t.IDPro);
            
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        IDNote = c.Int(nullable: false, identity: true),
                        IDPro = c.Int(nullable: false),
                        NoiDung = c.String(),
                    })
                .PrimaryKey(t => t.IDNote)
                .ForeignKey("dbo.Products", t => t.IDPro, cascadeDelete: true)
                .Index(t => t.IDPro);
            
            CreateTable(
                "dbo.Sizes",
                c => new
                    {
                        IDSize = c.Int(nullable: false, identity: true),
                        TenSize = c.String(),
                    })
                .PrimaryKey(t => t.IDSize);
            
            CreateTable(
                "dbo.ListNPLs",
                c => new
                    {
                        IDList = c.Int(nullable: false, identity: true),
                        IDDH = c.Int(nullable: false),
                        IDNPL = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IDList)
                .ForeignKey("dbo.DonHangs", t => t.IDDH, cascadeDelete: true)
                .ForeignKey("dbo.NPLs", t => t.IDNPL, cascadeDelete: true)
                .Index(t => t.IDDH)
                .Index(t => t.IDNPL);
            
            CreateTable(
                "dbo.NPLs",
                c => new
                    {
                        IDNPL = c.Int(nullable: false, identity: true),
                        TenNPL = c.String(),
                    })
                .PrimaryKey(t => t.IDNPL);
            
            CreateTable(
                "dbo.ImageNPLs",
                c => new
                    {
                        IDImgNPL = c.Int(nullable: false, identity: true),
                        URL = c.String(),
                        IDNPL = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IDImgNPL)
                .ForeignKey("dbo.NPLs", t => t.IDNPL, cascadeDelete: true)
                .Index(t => t.IDNPL);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ListNPLs", "IDNPL", "dbo.NPLs");
            DropForeignKey("dbo.ImageNPLs", "IDNPL", "dbo.NPLs");
            DropForeignKey("dbo.ListNPLs", "IDDH", "dbo.DonHangs");
            DropForeignKey("dbo.ProductVariants", "IDSize", "dbo.Sizes");
            DropForeignKey("dbo.ProductVariants", "IDPro", "dbo.Products");
            DropForeignKey("dbo.Notes", "IDPro", "dbo.Products");
            DropForeignKey("dbo.Images", "IDVariant", "dbo.ProductVariants");
            DropForeignKey("dbo.CTDHs", "IDVariant", "dbo.ProductVariants");
            DropForeignKey("dbo.ProductVariants", "IDColor", "dbo.Colors");
            DropForeignKey("dbo.CTDHs", "IDDH", "dbo.DonHangs");
            DropIndex("dbo.ImageNPLs", new[] { "IDNPL" });
            DropIndex("dbo.ListNPLs", new[] { "IDNPL" });
            DropIndex("dbo.ListNPLs", new[] { "IDDH" });
            DropIndex("dbo.Notes", new[] { "IDPro" });
            DropIndex("dbo.Images", new[] { "IDVariant" });
            DropIndex("dbo.ProductVariants", new[] { "IDColor" });
            DropIndex("dbo.ProductVariants", new[] { "IDSize" });
            DropIndex("dbo.ProductVariants", new[] { "IDPro" });
            DropIndex("dbo.CTDHs", new[] { "IDVariant" });
            DropIndex("dbo.CTDHs", new[] { "IDDH" });
            DropTable("dbo.ImageNPLs");
            DropTable("dbo.NPLs");
            DropTable("dbo.ListNPLs");
            DropTable("dbo.Sizes");
            DropTable("dbo.Notes");
            DropTable("dbo.Products");
            DropTable("dbo.Images");
            DropTable("dbo.Colors");
            DropTable("dbo.ProductVariants");
            DropTable("dbo.CTDHs");
        }
    }
}
