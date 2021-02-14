﻿// <auto-generated />
using System;
using ArbitralSystem.Storage.MarketInfoStorageService.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Migrations
{
    [DbContext(typeof(MarketInfoBdContext))]
    [Migration("20210214170314_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Entities.DistributorState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("CurrentStatus")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Exchange")
                        .HasColumnType("tinyint");

                    b.Property<byte>("PreviousStatus")
                        .HasColumnType("tinyint");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("varchar(32)");

                    b.Property<DateTime>("UtcChangedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("DistributerStates");
                });

            modelBuilder.Entity("ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Entities.OrderbookPriceEntry", b =>
                {
                    b.Property<byte>("Exchange")
                        .HasColumnType("tinyint");

                    b.Property<bool>("OrderSide")
                        .HasColumnType("bit");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(19,9)");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(19,9)");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("varchar(32)");

                    b.Property<DateTime>("UtcCatchAt")
                        .HasColumnType("datetime2");

                    b.ToTable("OrderbookPriceEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
