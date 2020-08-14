using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Text;

using TestApp.Models;

namespace TestApp.Database
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
    }
}
