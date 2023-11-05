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
            string contractFilePath = "C:\\Users\\Adam\\Desktop\\test.csv";

            IIntervalProvider intervalProvider = new CsvIntervalProvider();
            Interval reportInterval = intervalProvider.GetIntervalFromCsv(intervalFilePath);

            IContractProvider contractProvider = new CsvContractProvider();
            List<Contract> contracts = contractProvider.GetContractsFromCsv(contractFilePath);

            ReportGenerator reportGenerator = new ReportGenerator();
            List<string[]> report = reportGenerator.GenerateReport(contracts, reportInterval);

            foreach (string[] intervalData in report)
            {
                string intervalStart = intervalData[0];
                string intervalEnd = intervalData[1];
                string contractId = intervalData[2];
                string workTime = intervalData[3];

                Console.WriteLine($"Początek: {intervalStart}");
                Console.WriteLine($"Koniec: {intervalEnd}");
                Console.WriteLine($"ID Kontraktu: {contractId}");
                Console.WriteLine($"Czas Pracy: {workTime}");
                Console.WriteLine();
            }
        }
    }
}



