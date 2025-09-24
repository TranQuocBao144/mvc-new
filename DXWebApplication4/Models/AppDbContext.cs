using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DXWebApplication4.Models
{
    public class AppDbContext:DbContext
    {
        public AppDbContext() : base("name=AppDbContext")
        {
        }
        public DbSet<DonHang> DonHangs { get; set; }
    }
}