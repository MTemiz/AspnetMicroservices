﻿using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ordering.API.Extensions
{
	public static class HostExtensions
	{
	public static IHost MigrateDatabase<TContext>(this IHost host,
		Action<TContext, IServiceProvider> seeder, int? retry = 0) where TContext : DbContext
		{
			int retryForAvaiablity = retry.Value;

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;

				var logger = services.GetRequiredService<ILogger<TContext>>();

				var context = services.GetService<TContext>();

				try
				{
					logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    InvokeSeeder(seeder, context, services);

					logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
				catch (SqlException ex)
				{
					logger.LogError(ex, "An error occurred while migrating the database used on context");

					if (retryForAvaiablity < 50)
					{
						retryForAvaiablity++;

						System.Threading.Thread.Sleep(2000);

						MigrateDatabase<TContext>(host, seeder, retryForAvaiablity);
					}
				}

				return host;
			}
		}

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder,
			TContext context, IServiceProvider services) where TContext : DbContext
        {
			context.Database.Migrate();

			seeder(context, services);
        }
    }
}

