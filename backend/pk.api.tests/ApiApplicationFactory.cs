using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using pk.data;
using pk.data.Models;

namespace pk.api.tests;

public class ApiApplicationFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Hangfire:Disabled", bool.TrueString);
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Hangfire:Disabled", bool.TrueString),
            });
        });
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll<AppDbContext>();

            var databaseName = $"ApiIntegrationTests_{Guid.NewGuid():N}";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName, _databaseRoot);
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();

            SeedDatabase(db);
        });
    }

    private static void SeedDatabase(AppDbContext db)
    {
        if (db.KickstarterProjects.Any())
        {
            return;
        }

        var designCategory = new Category { Id = 1, Name = "Design" };
        var gamesCategory = new Category { Id = 2, Name = "Games" };

        var creatorAlice = new Creator { Id = 100, Name = "Alice" };
        var creatorBob = new Creator { Id = 101, Name = "Bob" };

        db.Categories.AddRange(designCategory, gamesCategory);
        db.Creators.AddRange(creatorAlice, creatorBob);

        var baseDate = DateTime.UtcNow.Date;

        db.KickstarterProjects.AddRange(
            CreateProject(
                id: 1_000_001,
                name: "Design Gadget",
                category: designCategory,
                creator: creatorAlice,
                country: "US",
                pledged: 2500m,
                percentFunded: 250m,
                launchedAt: baseDate.AddMonths(-1)
            ),
            CreateProject(
                id: 1_000_002,
                name: "Game Master",
                category: gamesCategory,
                creator: creatorBob,
                country: "US",
                pledged: 1800m,
                percentFunded: 180m,
                launchedAt: baseDate.AddMonths(-2)
            ),
            CreateProject(
                id: 1_000_003,
                name: "Design Lamp",
                category: designCategory,
                creator: creatorAlice,
                country: "CA",
                pledged: 900m,
                percentFunded: 90m,
                launchedAt: baseDate.AddMonths(-3),
                state: "failed"
            ),
            CreateProject(
                id: 1_000_004,
                name: "Design Bag",
                category: designCategory,
                creator: creatorBob,
                country: "US",
                pledged: 5000m,
                percentFunded: 500m,
                launchedAt: baseDate.AddMonths(-4)
            ),
            CreateProject(
                id: 1_000_005,
                name: "Game Quest",
                category: gamesCategory,
                creator: creatorBob,
                country: "GB",
                pledged: 2200m,
                percentFunded: 220m,
                launchedAt: baseDate.AddMonths(-5)
            )
        );

        db.SaveChanges();
    }

    private static KickstarterProject CreateProject(
        long id,
        string name,
        Category category,
        Creator creator,
        string country,
        decimal pledged,
        decimal percentFunded,
        DateTime launchedAt,
        string state = "successful")
    {
        return new KickstarterProject
        {
            Id = id,
            Name = name,
            Goal = 1000m,
            Pledged = pledged,
            PercentFunded = percentFunded,
            State = state,
            Country = country,
            Currency = "USD",
            Deadline = launchedAt.AddDays(30),
            CreatedAt = launchedAt.AddDays(-10),
            LaunchedAt = launchedAt,
            BackersCount = 100,
            UsdPledged = pledged,
            StateChangedAt = launchedAt.AddDays(15),
            Slug = name.ToLowerInvariant().Replace(' ', '-'),
            CountryDisplayableName = country,
            CurrencySymbol = "$",
            StaticUsdRate = 1m,
            ConvertedPledgedAmount = pledged,
            FxRate = 1m,
            UsdExchangeRate = 1m,
            CreatorId = creator.Id,
            Creator = creator,
            CategoryId = category.Id,
            Category = category,
            LocationId = null,
            IsLaunched = true,
            Spotlight = false
        };
    }
}
