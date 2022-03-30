using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PerfSqlite;

public class CarsContext : DbContext
{
    public DbSet<Car> Cars { get; set; }

    public string DbPath { get; }

    public CarsContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "cars.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

[Index(nameof(MakeName))]
public class Car
{
    public int CarId { get; set; }
    public int MakeId { get; set; }

    public string MakeName { get; set; }

    public DateTime CreatedTimestamp { get; set; }
}