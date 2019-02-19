﻿// <auto-generated />
using LeaderboardService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace LeaderboardService.Migrations
{
    [DbContext(typeof(LeaderboardContext))]
    partial class LeaderboardContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LeaderboardService.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("LeaderboardService.Models.Leaderboard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<int>("GameId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Leaderboards");
                });

            modelBuilder.Entity("LeaderboardService.Models.LeaderboardEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceID");

                    b.Property<int?>("LeaderboardId");

                    b.Property<string>("PlayerName");

                    b.Property<double>("ScoreValue");

                    b.HasKey("Id");

                    b.HasIndex("LeaderboardId");

                    b.ToTable("LeaderboardEntries");
                });

            modelBuilder.Entity("LeaderboardService.Models.Leaderboard", b =>
                {
                    b.HasOne("LeaderboardService.Models.Game", "Game")
                        .WithMany("Leaderboards")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LeaderboardService.Models.LeaderboardEntry", b =>
                {
                    b.HasOne("LeaderboardService.Models.Leaderboard")
                        .WithMany("Entries")
                        .HasForeignKey("LeaderboardId");
                });
#pragma warning restore 612, 618
        }
    }
}