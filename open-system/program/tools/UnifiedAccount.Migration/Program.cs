using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Migration.Exporters;
using UnifiedAccount.Migration.Importers;
using UnifiedAccount.Migration.Validators;

// Shift_JIS / CP932 サポート
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var command = args[0].ToLowerInvariant();

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Services.AddSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

builder.Services.AddTransient<BankBranchImporter>();
builder.Services.AddTransient<CompanyImporter>();
builder.Services.AddTransient<ContractImporter>();
builder.Services.AddTransient<CalendarImporter>();
builder.Services.AddTransient<CsvExporter>();
builder.Services.AddTransient<DataValidator>();

var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var batchSize = config.GetValue("Migration:BatchSize", 1000);
var inputDir = config.GetValue<string>("Migration:InputDirectory") ?? "./input";
var outputDir = config.GetValue<string>("Migration:OutputDirectory") ?? "./output";

try
{
    using var scope = host.Services.CreateScope();
    var sp = scope.ServiceProvider;

    switch (command)
    {
        case "import-banks":
        {
            var csvPath = GetCsvPath(args, inputDir, "BankBranches.csv");
            var importer = sp.GetRequiredService<BankBranchImporter>();
            await importer.ImportAsync(csvPath, batchSize);
            break;
        }
        case "import-companies":
        {
            var csvPath = GetCsvPath(args, inputDir, "Companies.csv");
            var importer = sp.GetRequiredService<CompanyImporter>();
            await importer.ImportAsync(csvPath, batchSize);
            break;
        }
        case "import-contracts":
        {
            var csvPath = GetCsvPath(args, inputDir, "Contracts.csv");
            var importer = sp.GetRequiredService<ContractImporter>();
            await importer.ImportAsync(csvPath, batchSize);
            break;
        }
        case "import-calendars":
        {
            var csvPath = GetCsvPath(args, inputDir, "ProcessingCalendars.csv");
            var importer = sp.GetRequiredService<CalendarImporter>();
            await importer.ImportAsync(csvPath, batchSize);
            break;
        }
        case "validate":
        {
            var validator = sp.GetRequiredService<DataValidator>();
            var ok = await validator.ValidateAsync();
            return ok ? 0 : 2;
        }
        case "export-csv":
        {
            var exporter = sp.GetRequiredService<CsvExporter>();
            await exporter.ExportAllAsync(outputDir);
            break;
        }
        default:
            Log.Error("Unknown command: {Command}", command);
            PrintUsage();
            return 1;
    }

    Log.Information("Command '{Command}' completed successfully.", command);
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Migration failed.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static string GetCsvPath(string[] args, string inputDir, string defaultFileName)
{
    if (args.Length >= 2)
        return args[1];

    return Path.Combine(inputDir, defaultFileName);
}

static void PrintUsage()
{
    Console.WriteLine("""
        Usage: UnifiedAccount.Migration <command> [options]

        Commands:
          import-banks       Import BankBranch data from CSV
          import-companies   Import Company data from CSV
          import-contracts   Import Contract data from CSV
          import-calendars   Import Calendar data from CSV
          validate           Validate imported data (counts, checksums)
          export-csv         Export current DB data to CSV for reconciliation

        Options:
          [file]             Path to CSV file (defaults to input/<TableName>.csv)
        """);
}
