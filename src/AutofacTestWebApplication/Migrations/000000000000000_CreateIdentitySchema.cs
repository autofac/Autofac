using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;

namespace AutofacTestWebApplication.Migrations
{
    public partial class CreateIdentitySchema : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("AspNetRoles",
                c => new
                    {
                        Id = c.String(),
                        Name = c.String()
                    })
                .PrimaryKey("PK_AspNetRoles", t => t.Id);
            
            migrationBuilder.CreateTable("AspNetRoleClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        RoleId = c.String()
                    })
                .PrimaryKey("PK_AspNetRoleClaims", t => t.Id);
            
            migrationBuilder.CreateTable("AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        UserId = c.String()
                    })
                .PrimaryKey("PK_AspNetUserClaims", t => t.Id);
            
            migrationBuilder.CreateTable("AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(),
                        ProviderKey = c.String(),
                        ProviderDisplayName = c.String(),
                        UserId = c.String()
                    })
                .PrimaryKey("PK_AspNetUserLogins", t => new { t.LoginProvider, t.ProviderKey });
            
            migrationBuilder.CreateTable("AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(),
                        RoleId = c.String()
                    })
                .PrimaryKey("PK_AspNetUserRoles", t => new { t.UserId, t.RoleId });
            
            migrationBuilder.CreateTable("AspNetUsers",
                c => new
                    {
                        Id = c.String(),
                        AccessFailedCount = c.Int(nullable: false),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        LockoutEnabled = c.Boolean(nullable: false),
                        LockoutEnd = c.DateTimeOffset(),
                        NormalizedUserName = c.String(),
                        PasswordHash = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        SecurityStamp = c.String(),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        UserName = c.String()
                    })
                .PrimaryKey("PK_AspNetUsers", t => t.Id);
            
            migrationBuilder.AddForeignKey(
                "AspNetRoleClaims",
                "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                 new[] { "RoleId" },
                 "AspNetRoles",
                 new[] { "Id" },
                 cascadeDelete: false);
            
            migrationBuilder.AddForeignKey(
                "AspNetUserClaims",
                "FK_AspNetUserClaims_AspNetUsers_UserId",
                 new[] { "UserId" }, "AspNetUsers",
                 new[] { "Id" },
                 cascadeDelete: false);
            
            migrationBuilder.AddForeignKey(
                "AspNetUserLogins",
                "FK_AspNetUserLogins_AspNetUsers_UserId",
                 new[] { "UserId" },
                 "AspNetUsers",
                 new[] { "Id" },
                 cascadeDelete: false);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("AspNetRoleClaims", "FK_AspNetRoleClaims_AspNetRoles_RoleId");
            
            migrationBuilder.DropForeignKey("AspNetUserClaims", "FK_AspNetUserClaims_AspNetUsers_UserId");
            
            migrationBuilder.DropForeignKey("AspNetUserLogins", "FK_AspNetUserLogins_AspNetUsers_UserId");
            
            migrationBuilder.DropTable("AspNetRoles");
            
            migrationBuilder.DropTable("AspNetRoleClaims");
            
            migrationBuilder.DropTable("AspNetUserClaims");
            
            migrationBuilder.DropTable("AspNetUserLogins");
            
            migrationBuilder.DropTable("AspNetUserRoles");
            
            migrationBuilder.DropTable("AspNetUsers");
        }
    }

    [ContextType(typeof(Models.ApplicationDbContext))]
    public partial class CreateIdentitySchema : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return "000000000000000_CreateIdentitySchema";
            }
        }

        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return "7.0.0-beta2";
            }
        }

        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();

                builder.Entity("Microsoft.AspNet.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id");
                    b.Property<string>("Name");
                    b.Key("Id");
                    b.ForRelational().Table("AspNetRoles");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityRoleClaim`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.Property<string>("ClaimType");
                    b.Property<string>("ClaimValue");
                    b.Property<int>("Id")
                        .GenerateValueOnAdd();
                    b.Property<string>("RoleId");
                    b.Key("Id");
                    b.ForRelational().Table("AspNetRoleClaims");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityUserClaim`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.Property<string>("ClaimType");
                    b.Property<string>("ClaimValue");
                    b.Property<int>("Id")
                        .GenerateValueOnAdd();
                    b.Property<string>("UserId");
                    b.Key("Id");
                    b.ForRelational().Table("AspNetUserClaims");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityUserLogin`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.Property<string>("LoginProvider");
                    b.Property<string>("ProviderDisplayName");
                    b.Property<string>("ProviderKey");
                    b.Property<string>("UserId");
                    b.Key("LoginProvider", "ProviderKey");
                    b.ForRelational().Table("AspNetUserLogins");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityUserRole`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.Property<string>("RoleId");
                    b.Property<string>("UserId");
                    b.Key("UserId", "RoleId");
                    b.ForRelational().Table("AspNetUserRoles");
                });

                builder.Entity("AutofacTestWebApplication.Models.ApplicationUser", b =>
                {
                    b.Property<int>("AccessFailedCount");
                    b.Property<string>("Email");
                    b.Property<bool>("EmailConfirmed");
                    b.Property<string>("Id");
                    b.Property<bool>("LockoutEnabled");
                    b.Property<DateTimeOffset?>("LockoutEnd");
                    b.Property<string>("NormalizedUserName");
                    b.Property<string>("PasswordHash");
                    b.Property<string>("PhoneNumber");
                    b.Property<bool>("PhoneNumberConfirmed");
                    b.Property<string>("SecurityStamp");
                    b.Property<bool>("TwoFactorEnabled");
                    b.Property<string>("UserName");
                    b.Key("Id");
                    b.ForRelational().Table("AspNetUsers");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityRoleClaim`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.ForeignKey("Microsoft.AspNet.Identity.IdentityRole", "RoleId");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityUserClaim`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.ForeignKey("AutofacTestWebApplication.Models.ApplicationUser", "UserId");
                });

                builder.Entity("Microsoft.AspNet.Identity.IdentityUserLogin`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", b =>
                {
                    b.ForeignKey("AutofacTestWebApplication.Models.ApplicationUser", "UserId");
                });

                return builder.Model;
            }
        }
    }
}