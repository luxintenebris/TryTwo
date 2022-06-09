﻿// <auto-generated />
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20220609084307_GameSessions-init")]
    partial class GameSessionsinit
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Entity.GameSession", b =>
                {
                    b.Property<int>("sessionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("player1")
                        .HasColumnType("int");

                    b.Property<int>("player2")
                        .HasColumnType("int");

                    b.HasKey("sessionId");

                    b.ToTable("GameSessions");
                });

            modelBuilder.Entity("Entity.OpenGames", b =>
                {
                    b.Property<int>("GameID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("Player1")
                        .HasColumnType("int");

                    b.Property<bool>("Started")
                        .HasColumnType("bit");

                    b.HasKey("GameID");

                    b.ToTable("OpenGames");
                });

            modelBuilder.Entity("Entity.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("binary(500)")
                        .IsFixedLength(true);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
