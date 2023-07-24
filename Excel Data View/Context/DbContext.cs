using ExcelDataView.Entity;
using Microsoft.EntityFrameworkCore;

namespace ExcelDataView.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ExcelRecord> ExcelRecords { get; set; }
    }
}
