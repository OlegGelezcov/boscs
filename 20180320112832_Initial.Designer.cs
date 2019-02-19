﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using PromoCodeService.Models;
using System;

namespace PromoCodeService.Migrations
{
    [DbContext(typeof(PromoCodeContext))]
    [Migration("20180320112832_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PromoCodeService.Models.PromoCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code");

                    b.Property<DateTime>("DateAdded");

                    b.Property<DateTime>("ExpirationDate");

                    b.Property<bool>("IsMultipleRedeemable");

                    b.Property<int>("MaxRedeems");

                    b.Property<double>("Value");

                    b.HasKey("Id");

                    b.ToTable("PromoCodes");
                });

            modelBuilder.Entity("PromoCodeService.Models.RedeemedCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateRedeemed");

                    b.Property<string>("DeviceId");

                    b.Property<int>("PromoCodeId");

                    b.HasKey("Id");

                    b.HasIndex("PromoCodeId");

                    b.ToTable("RedeemedCodes");
                });

            modelBuilder.Entity("PromoCodeService.Models.RedeemedCode", b =>
                {
                    b.HasOne("PromoCodeService.Models.PromoCode", "PromoCode")
                        .WithMany("RedeemedCodes")
                        .HasForeignKey("PromoCodeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
