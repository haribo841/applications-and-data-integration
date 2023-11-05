using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Evaluation_task_3
{
    internal class CsvContractProvider : IContractProvider
    {
        public List<Contract> GetContractsFromCsv(string filePath)
        {
            var contracts = new List<Contract>();

            using (var reader = new StreamReader(filePath))
            {
                string headerLine = reader.ReadLine(); 
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(',');

                    if (parts.Length >= 2)
                    {
                        DateTime startDate = ParseDate(parts[0]);
                        DateTime? endDate = ParseNullableDate(parts[1]);

                        contracts.Add(new Contract
                        {
                            StartDate = startDate,
                            EndDate = endDate
                        });
                    }
                }
            }

            return contracts;
        }

        private DateTime ParseDate(string dateString)
        {
            DateTime parsedDate;

            if (DateTime.TryParse(dateString, out parsedDate))
            {

                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }
            else
            {

                throw new Exception("Nie można sparsować daty.");
            }
        }

        private DateTime? ParseNullableDate(string dateString)
        {
            if (dateString == "-")
            {
                return null; 
            }
            else
            {
                return ParseDate(dateString);
            }
        }
    }
}
