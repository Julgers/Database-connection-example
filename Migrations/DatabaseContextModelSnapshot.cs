﻿// <auto-generated />
using DatabaseExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CommunityServerAPI.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DatabaseExample.Models.BannedWeapon", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Name");

                    b.ToTable("BannedWeapons");
                });

            modelBuilder.Entity("DatabaseExample.Models.GameServer", b =>
                {
                    b.Property<string>("Ip")
                        .HasColumnType("varchar(45)");

                    b.Property<ushort>("Port")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("Token")
                        .HasColumnType("longtext");

                    b.ToTable("GameServers");
                });

            modelBuilder.Entity("DatabaseExample.Models.ServerPlayer", b =>
                {
                    b.Property<ulong>("SteamId")
                        .HasColumnType("bigint unsigned");

                    b.Property<byte[]>("Stats")
                        .HasColumnType("longblob");

                    b.HasKey("SteamId");

                    b.ToTable("Player");
                });
#pragma warning restore 612, 618
        }
    }
}
