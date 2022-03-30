﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace PerfSqlite;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    private static async Task Main(string[] args)
    {
        if (args[0].Equals("insert")) {
            await Insert();
        } else if (args[0].Equals("query")) {
            await Query(false);
        } else if (args[0].Equals("print")) {
            await Query(true);
        }
    }

    private static async Task Insert() {
        Console.WriteLine("Download cars from WEB.");
        var url = "https://vpic.nhtsa.dot.gov/api/vehicles/getallmakes?format=json";
        var streamTask = client.GetStreamAsync(url);
        var carsRepo = await JsonSerializer.DeserializeAsync<CarsRepository>(await streamTask);
        Console.WriteLine($"Cars downloaded count: {carsRepo.Count}");
        
        using (var db = new CarsContext())
        {
            // Note: This sample requires the database to be created before running.
            Console.WriteLine($"Database path: {db.DbPath}.");
                
            // Insert multiple elements
            Console.WriteLine("Inserting...");
            Stopwatch insertWatch = new Stopwatch();
            
            insertWatch.Start();
            foreach (var carDto in carsRepo.Results) {
                db.Add(new Car { MakeId = carDto.Make_ID, MakeName = carDto.Make_Name, CreatedTimestamp = DateTime.UtcNow });
            }
            db.SaveChanges();            
            insertWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = insertWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("INSERTION " + elapsedTime);
        }
    }

    private static async Task Query(bool print) {
        using (var db = new CarsContext())
        {
            // Read
            Console.WriteLine("Querying for cars ordered by name");
            Stopwatch queryWatch = new Stopwatch();
            queryWatch.Start();
            var cars = db.Cars
                .OrderBy(c => c.MakeName);
            queryWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = queryWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime= String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"QUERY in {elapsedTime}");

            if (print) {
                foreach (var car in cars) {
                    Console.WriteLine(car.MakeName);
                }
            }
        }
    }
}

public class CarsRepository
{
    public int Count { get; set; }
    public List<CarDto> Results { get; set; } = new();
}

public class CarDto {
    public int Make_ID { get; set; }
    public string Make_Name { get; set; }
}