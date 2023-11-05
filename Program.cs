using Evaluation_task_3;
using System;
using System.Collections.Generic;
namespace Evaluation_task_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string intervalFilePath = "C:\\Users\\Adam\\Desktop\\interval.csv";
            string contractfilePath = "C:\\Users\\Adam\\Desktop\\test.csv";
            CsvIntervalProvider intervalProvider = new CsvIntervalProvider();
            Interval interval = intervalProvider.GetIntervalFromCsv(intervalFilePath);
            IContractProvider contractProvider = new CsvContractProvider();
            List<Contract> contracts = contractProvider.GetContractsFromCsv(contractfilePath);
            ReportGenerator reportGenerator = new ReportGenerator();
            List<string[]> report = reportGenerator.GenerateReport(interval);
            if (interval != null)
            {
                Console.WriteLine("Generated Report:");
                    Console.WriteLine($"Interval: {interval.StartDate} - {interval.EndDate}");
            }
            else
            {
                Console.WriteLine("Nie udało się wczytać danych z pliku CSV.");
            }
        }
    }
}