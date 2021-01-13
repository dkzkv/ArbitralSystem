﻿// <auto-generated />
using System;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArbitralSystem.Distributor.MQDistributor.MQPersistence.Migrations
{
    [DbContext(typeof(DistributorDbContext))]
    [Migration("20200829191545_NullableServer")]
    partial class NullableServer
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
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
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("ServerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(900)")
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("Name", "Type")
                        .IsUnique();

                    b.ToTable("Distributors","mqd");
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
#pragma warning restore 612, 618
        }
    }
}
