using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace TestApplication
{
    public partial class TestDB : DbContext
    {
        public TestDB(): base("name=TestDB")
        {
        }

        public virtual DbSet<InventDim> InventDim { get; set; }
        public virtual DbSet<InventLocation> InventLocation { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventDim>()
                .Property(e => e.RowVersion)
                .IsFixedLength();

            modelBuilder.Entity<InventLocation>()
                .Property(e => e.RowVersion)
                .IsFixedLength();
        }
    }
}
