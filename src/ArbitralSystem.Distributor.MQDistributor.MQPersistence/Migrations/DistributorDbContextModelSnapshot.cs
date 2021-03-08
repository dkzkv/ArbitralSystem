﻿// <auto-generated />
using System;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArbitralSystem.Distributor.MQDistributor.MQPersistence.Migrations
{
    [DbContext(typeof(DistributorDbContext))]
    partial class DistributorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Distributor", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("ModifyAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ServerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("Distributors","mqd");
                });

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.DistributorExchange", b =>
                {
                    b.Property<Guid>("DistributorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ExchangeId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("HeartBeat")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("DistributorId", "ExchangeId");

                    b.HasIndex("ExchangeId");

                    b.ToTable("DistributorExchanges","mqd");
                });

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Exchange", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Exchanges","mqd");
                });

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("MaxWorkersCount")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("ModifyAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerType")
                        .IsRequired()
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Servers","mqd");
                });

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Distributor", b =>
                {
                    b.HasOne("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Server", "Server")
                        .WithMany("Distributors")
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.DistributorExchange", b =>
                {
                    b.HasOne("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Distributor", "Distributor")
                        .WithMany("Exchanges")
                        .HasForeignKey("DistributorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities.Exchange", "Exchange")
                        .WithMany("Distributors")
                        .HasForeignKey("ExchangeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
