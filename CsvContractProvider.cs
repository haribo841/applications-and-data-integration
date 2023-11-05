using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation_task_3
{
    internal class CsvContractProvider : IContractProvider
    {
        public List<Contract> GetContractsFromCsv(string filePath)
        {
            var contracts = new List<Contract>();

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(' ');
                    if (parts.Length >= 2)
                    {
                        DateTime startDate;
                        if (DateTime.TryParse(parts[0], out startDate))
                        {
                            if (parts.Length >= 3)
                            {
                                string endDateStr = parts[2];
                                DateTime? endDate = !string.IsNullOrWhiteSpace(endDateStr) ? DateTime.Parse(endDateStr) : (DateTime?)null;
                                string timeZone = parts[parts.Length - 1];
                                contracts.Add(new Contract
                                {
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    TimeZone = timeZone
                                });
                            }
                        }
                    }
                }
            }

            return contracts;
        }
    }
}
