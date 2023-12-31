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
    [Migration("20230905225150_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.10");

            modelBuilder.Entity("LandSim.Areas.Map.Models.Color", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<float>("Blue")
                        .HasColumnType("REAL");

                    b.Property<float>("Green")
                        .HasColumnType("REAL");

                    b.Property<float>("Red")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Colors");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.ColorSelector", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ColorId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GenerationSettingsId")
                        .HasColumnType("INTEGER");

                    b.Property<float>("MaxValue")
                        .HasColumnType("REAL");

                    b.Property<float>("MinValue")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("ColorId");

                    b.HasIndex("GenerationSettingsId");

                    b.ToTable("ColorSelectors");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.GenerationSettings", b =>
                {
                    b.Property<int>("Id")
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

                    b.HasKey("Id");

                    b.ToTable("GenerationSettings");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.ColorSelector", b =>
                {
                    b.HasOne("LandSim.Areas.Map.Models.Color", "Color")
                        .WithMany()
                        .HasForeignKey("ColorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LandSim.Areas.Map.Models.GenerationSettings", null)
                        .WithMany("ColorSelectors")
                        .HasForeignKey("GenerationSettingsId");

                    b.Navigation("Color");
                });

            modelBuilder.Entity("LandSim.Areas.Map.Models.GenerationSettings", b =>
                {
                    b.Navigation("ColorSelectors");
                });
#pragma warning restore 612, 618
        }
    }
}
