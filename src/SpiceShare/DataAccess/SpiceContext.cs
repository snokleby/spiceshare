using System;
using Microsoft.EntityFrameworkCore;

namespace SpiceShare.DataAccess
{
    public class SpiceContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Spice> Spices { get; set; }

        public DbSet<ManagementData> ManadagementData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./spiceshare.db");
        }
    }

    public class ManagementData
    {
        public DateTime NextPrune { get; set; }
        public int Id { get; set; }
    }
}
