namespace BookAdvisorWebApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class First : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Author",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Book",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    F_Id = c.String(nullable: false, maxLength: 200, unicode: false),
                    Title = c.String(nullable: false, maxLength: 200),
                    URL = c.String(),
                    Decription = c.String(),
                    PublisherId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Publisher", t => t.PublisherId)
                .Index(t => t.PublisherId);

            CreateTable(
                "dbo.BookUserLike",
                c => new
                {
                    BookId = c.Int(nullable: false),
                    ProfileId = c.Int(nullable: false),
                    Value = c.Int(),
                    Scale = c.Int(),
                })
                .PrimaryKey(t => new { t.BookId, t.ProfileId })
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Profile", t => t.ProfileId)
                .Index(t => t.BookId)
                .Index(t => t.ProfileId);

            CreateTable(
                "dbo.Profile",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    F_Id = c.String(nullable: false, maxLength: 200, unicode: false),
                    Name = c.String(nullable: false, maxLength: 200),
                    Email = c.String(nullable: false, maxLength: 200, unicode: false),
                    LastName = c.String(maxLength: 200),
                    Gender = c.String(maxLength: 80, unicode: false),
                    URL = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Category",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.IndustryIdentifier",
                c => new
                {
                    Identifier = c.String(nullable: false, maxLength: 128, unicode: false),
                    Type = c.String(nullable: false),
                    BookId = c.Int(),
                })
                .PrimaryKey(t => t.Identifier)
                .ForeignKey("dbo.Book", t => t.BookId)
                .Index(t => t.BookId);

            CreateTable(
                "dbo.Publisher",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 200),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.BookCategory",
                c => new
                {
                    BookId = c.Int(nullable: false),
                    CategoryId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.BookId, t.CategoryId })
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.CategoryId);

            CreateTable(
                "dbo.BookAuthor",
                c => new
                {
                    AuthorId = c.Int(nullable: false),
                    BookId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.AuthorId, t.BookId })
                .ForeignKey("dbo.Author", t => t.AuthorId, cascadeDelete: true)
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .Index(t => t.AuthorId)
                .Index(t => t.BookId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.BookAuthor", "BookId", "dbo.Book");
            DropForeignKey("dbo.BookAuthor", "AuthorId", "dbo.Author");
            DropForeignKey("dbo.Book", "PublisherId", "dbo.Publisher");
            DropForeignKey("dbo.IndustryIdentifier", "BookId", "dbo.Book");
            DropForeignKey("dbo.BookCategory", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.BookCategory", "BookId", "dbo.Book");
            DropForeignKey("dbo.BookUserLike", "ProfileId", "dbo.Profile");
            DropForeignKey("dbo.BookUserLike", "BookId", "dbo.Book");
            DropIndex("dbo.BookAuthor", new[] { "BookId" });
            DropIndex("dbo.BookAuthor", new[] { "AuthorId" });
            DropIndex("dbo.BookCategory", new[] { "CategoryId" });
            DropIndex("dbo.BookCategory", new[] { "BookId" });
            DropIndex("dbo.IndustryIdentifier", new[] { "BookId" });
            DropIndex("dbo.BookUserLike", new[] { "ProfileId" });
            DropIndex("dbo.BookUserLike", new[] { "BookId" });
            DropIndex("dbo.Book", new[] { "PublisherId" });
            DropTable("dbo.BookAuthor");
            DropTable("dbo.BookCategory");
            DropTable("dbo.Publisher");
            DropTable("dbo.IndustryIdentifier");
            DropTable("dbo.Category");
            DropTable("dbo.Profile");
            DropTable("dbo.BookUserLike");
            DropTable("dbo.Book");
            DropTable("dbo.Author");
            
        }
    }
}
