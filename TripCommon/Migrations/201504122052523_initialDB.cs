namespace TripCommon.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TripAdvices",
                c => new
                    {
                        TripAdviceID = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        AdviceType = c.String(),
                        PlaceName = c.String(),
                        GeoLocation = c.String(),
                        Country = c.String(),
                        City = c.String(),
                        AdviceText = c.String(),
                        RankPlace = c.String(),
                        ImageURL = c.String(),
                        ThumbnailURL = c.String(),
                        AdviceDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TripAdviceID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TripAdvices");
        }
    }
}
