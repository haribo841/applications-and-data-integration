using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Evaluation_task_3
{
    internal class CsvIntervalProvider : IIntervalProvider
    {
        public Interval GetIntervalFromCsv(string filePath)
        {
            Interval interval = null;

            using (var reader = new StreamReader(filePath))
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string pattern = @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\+\d{2}:\d{2}) - (\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\+\d{2}:\d{2}) \(([^)]+)\)";
                    var match = Regex.Match(line, pattern);

                    if (match.Success && match.Groups.Count == 4)
                    {
                        DateTime startDate = DateTime.Parse(match.Groups[1].Value);
                        DateTime endDate = DateTime.Parse(match.Groups[2].Value);
                        string timeZone = match.Groups[3].Value;

                        interval = new Interval
                        {
                            StartDate = startDate,
                            EndDate = endDate,
                            TimeZone = timeZone
                        };
                    }
                }
            }

            return interval;
        }
    }
}