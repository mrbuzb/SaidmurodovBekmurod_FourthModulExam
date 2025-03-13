using Microsoft.EntityFrameworkCore;
using MyFirstEBot.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstEBot;

public class MainContext : DbContext
{
    public DbSet<BotUser> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer( "Server=WIN-OVG8EVF4A91\\SQLEXPRESS;Database=botTgTtest;User Id=sa;Password=1;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }


}
