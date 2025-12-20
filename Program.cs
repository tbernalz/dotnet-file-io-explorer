using System;
using System.Text;
using Newtonsoft.Json;

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");

var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDirectory);

var salesTotal = CalculateSalesTotal(salesFiles);

File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

GenerateSalesReport(salesFiles, salesTotal, salesTotalDir);

IEnumerable<string> FindFiles(string folderName)
{
    List<string> salesFiles = new List<string>();

    var foundFiles = Directory.EnumerateFiles(
        folderName,
        "sales.json",
        SearchOption.AllDirectories
    );

    salesFiles.AddRange(foundFiles);

    return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;

    foreach (var file in salesFiles)
    {
        try
        {
            string salesJson = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(salesJson))
            {
                Console.WriteLine($"Warning: File {file} is empty.");
                continue;
            }

            SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
            if (data != null)
            {
                salesTotal += data.Total;
                Console.WriteLine($"Processed {file}: Total = {data.Total}");
            }
            else
            {
                Console.WriteLine($"Warning: Failed to parse JSON in {file}.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing {file}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading {file}: {ex.Message}");
        }
    }

    return salesTotal;
}

void GenerateSalesReport(IEnumerable<string> salesFiles, double salesTotal, string salesTotalDir)
{
    var report = new StringBuilder();
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($"Total Sales: {salesTotal:C}");
    report.AppendLine("\nDetails:");

    foreach (var file in salesFiles)
    {
        try
        {
            string salesJson = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(salesJson))
            {
                report.AppendLine($"  {Path.GetFileName(file)}: Empty file");
                continue;
            }

            SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
            if (data != null)
            {
                report.AppendLine($"  {Path.GetFileName(file)}: {data.Total:C}");
            }
            else
            {
                report.AppendLine($"  {Path.GetFileName(file)}: Invalid JSON");
            }
        }
        catch (JsonException)
        {
            report.AppendLine($"  {Path.GetFileName(file)}: Error parsing JSON");
        }
        catch (Exception)
        {
            report.AppendLine($"  {Path.GetFileName(file)}: Error reading file");
        }
    }

    File.WriteAllText(Path.Combine(salesTotalDir, "salesReport.txt"), report.ToString());
}

record SalesData(double Total);
