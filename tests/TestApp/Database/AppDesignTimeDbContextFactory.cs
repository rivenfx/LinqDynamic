using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp.Database
{
    class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = AppConsts.ConnectionString;

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} start");
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} current database connection string: {connectionString}");

            var builder = new DbContextOptionsBuilder<AppDbContext>();

            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }

    }
}

