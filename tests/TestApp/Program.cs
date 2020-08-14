using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

using TestApp.Database;
using TestApp.Models;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = default(IServiceCollection);
            var serviceProvider = default(IServiceProvider);

            services = new ServiceCollection();

            services.AddDbContext<AppDbContext>((options) =>
            {
                options.UseSqlServer(AppConsts.ConnectionString);
            });

            serviceProvider = services.BuildServiceProvider();


            if (CreateDbIfNotExists(serviceProvider))
            {
                TryCreateUsers(serviceProvider);


                TryFilter(serviceProvider);

            }



            Console.WriteLine("程序运行结束，按回车键结束程序");
            Console.ReadLine();
        }


        /// <summary>
        /// 数据库不存在则创建数据库
        /// </summary>
        /// <param name="serviceProvider"></param>
        private static bool CreateDbIfNotExists(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();


                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"自动创建数据库出错!");
                    Console.WriteLine($"错误信息: {ex.Message}");
                    Console.WriteLine($"错误详情: {ex.ToString()}");

                    return false;
                }
            }
        }


        /// <summary>
        /// 尝试创建用户
        /// </summary>
        /// <param name="serviceProvider"></param>
        static void TryCreateUsers(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                if (dbContext.Users.Count() == 0)
                {
                    dbContext.Add(new User()
                    {
                        Name = "ObjectA",
                        CreationTime = DateTime.Parse("2020-08-01 00:00:00"),
                        IsActive = true,
                        CanNullVal = 1
                    });

                    dbContext.Add(new User()
                    {
                        Name = "ObjectB",
                        CreationTime = DateTime.Parse("2020-08-05 00:00:00"),
                        IsActive = true,
                        CanNullVal = 2
                    });

                    dbContext.Add(new User()
                    {
                        Name = "ObjectC",
                        CreationTime = DateTime.Parse("2020-08-10 00:00:00"),
                        IsActive = false
                    });

                    dbContext.Add(new User()
                    {
                        Name = "TestA",
                        CreationTime = DateTime.Parse("2020-08-11 00:00:00"),
                        IsActive = false
                    });

                    dbContext.Add(new User()
                    {
                        Name = "TestB",
                        CreationTime = DateTime.Parse("2020-08-12 00:00:00"),
                        IsActive = false
                    });

                    dbContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        /// <param name="serviceProvider"></param>
        static void TryFilter(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

                var query = dbContext.Set<User>().AsQueryable();
                var v1 = query.Where(o => o.IsActive.Equals(false)).FirstOrDefault();


                var v2 = query.Where("IsActive == @0", false).FirstOrDefault();

                var v3 = query.Where(
                        "CreationTime > @0 and CreationTime <=@1",
                        DateTime.Parse("2020-08-01 00:00:00"),
                        DateTime.Parse("2020-08-10 00:00:00")
                    )
                    .ToList();

                var nameArray = new List<string> { "ObjectA", "ObjectC" };
                var v4 = query.Where("Name in @0", nameArray)
                    .ToList();

                var v5 = query.Where("name.StartsWith(@0)", "Test")
                    .ToList();

                var v6 = query.Where("name.EndsWith(@0)", "C")
                    .ToList();

                var v7 = query.Where("name.Contains(@0)", "Object")
                  .ToList();

                var v8 = query.Where("CanNullVal == null")
                  .ToList();


                var queryConds = new List<QueryCondition>();
                queryConds.Add(new QueryCondition()
                {
                    Field = "Name",
                    Operator = QueryOperator.NotEqual,
                    Value = "ObjectC"
                });
                queryConds.Add(new QueryCondition()
                {
                    Field = "CreationTime",
                    Operator = QueryOperator.BetweenEqualStart,
                    Value = "2020-08-01 00:00:00|2020-08-20 00:00:00"
                });
                queryConds.Add(new QueryCondition()
                {
                    Field = "Name",
                    Operator = QueryOperator.In,
                    Value = "TestA|TestB"
                });

                var v9 = query.Where(queryConds).ToList();
            }
        }
    }
}
