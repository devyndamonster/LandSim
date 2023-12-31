﻿// <auto-generated />
using System;
using LandSim.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LandSim.Migrations
{
    [DbContext(typeof(MapContext))]
    [Migration("20230909224046_AddedVegitation")]
    partial class AddedVegitation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.10");

            modelBuilder.Entity("LandSim.Areas.Map.Models.GenerationSettings", b =>
                {
                    b.Property<int>("GenerationSettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<float>("Frequency")
                        .HasColumnType("REAL");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Seed")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("GenerationSettingsId");

                    b.ToTable("GenerationSettings");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.TerrainSelector", b =>
                {
                    b.Property<int>("TerrainSelectorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GenerationSettingsId")
                        .HasColumnType("INTEGER");

                    b.Property<float>("MaxValue")
                        .HasColumnType("REAL");

                    b.Property<float>("MinValue")
                        .HasColumnType("REAL");

                    b.Property<int>("TerrainType")
                        .HasColumnType("INTEGER");

                    b.HasKey("TerrainSelectorId");

                    b.HasIndex("GenerationSettingsId");

                    b.ToTable("TerrainSelectors");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.TerrainTile", b =>
                {
                    b.Property<int>("TerrainTileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<float>("Height")
                        .HasColumnType("REAL");

                    b.Property<int>("TerrainType")
                        .HasColumnType("INTEGER");

                    b.Property<float>("VegetationLevel")
                        .HasColumnType("REAL");

                    b.Property<int>("XCoord")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YCoord")
                        .HasColumnType("INTEGER");

                    b.HasKey("TerrainTileId");

                    b.ToTable("TerrainTiles");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.TerrainSelector", b =>
                {
                    b.HasOne("LandSim.Areas.Map.Models.GenerationSettings", null)
                        .WithMany("TerrainSelectors")
                        .HasForeignKey("GenerationSettingsId");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.GenerationSettings", b =>
                {
                    b.Navigation("TerrainSelectors");
                });
#pragma warning restore 612, 618
        }
    }
}
