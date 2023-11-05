using System;
using System.Collections.Generic;
using System.Linq;

namespace Evaluation_task_3
{
    internal class ReportGenerator : IReportGenerator
    {
        public List<string[]> GenerateReport(List<Contract> contracts, Interval reportInterval)
        {
            List<string[]> reportIntervals = new List<string[]>();

            TimeZoneInfo europeWarsawTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
            DateTime reportStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(reportInterval.StartDate, DateTimeKind.Unspecified), europeWarsawTimeZone);
            DateTime reportEndDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(reportInterval.EndDate, DateTimeKind.Unspecified), europeWarsawTimeZone);

            Dictionary<int, TimeSpan> workTimeByContract = new Dictionary<int, TimeSpan>();

            foreach (var contract in contracts)
            {
                if (contract.StartDate <= reportEndDate && (!contract.EndDate.HasValue || contract.EndDate >= reportStartDate))
                {
                    DateTime start = contract.StartDate > reportStartDate ? contract.StartDate : reportStartDate;
                    DateTime end = !contract.EndDate.HasValue || contract.EndDate > reportEndDate ? reportEndDate : contract.EndDate.Value;

                    DateTime overlapStart = start > reportStartDate ? start : reportStartDate;
                    DateTime overlapEnd = end < reportEndDate ? end : reportEndDate;

                    TimeSpan overlapDuration = overlapEnd - overlapStart;
                }
            }

            foreach (var kvp in workTimeByContract)
            {
                string intervalStart = TimeZoneInfo.ConvertTimeFromUtc(reportStartDate, europeWarsawTimeZone).ToString("yyyy-MM-ddTHH:mm:ss zzz");
                string intervalEnd = TimeZoneInfo.ConvertTimeFromUtc(reportEndDate, europeWarsawTimeZone).ToString("yyyy-MM-ddTHH:mm:ss zzz");

                if (kvp.Key == -1) // okres bez zatrudnienia
                {
                    reportIntervals.Add(new string[]
                    {
                intervalStart,
                intervalEnd,
                "No Employment",
                kvp.Value.ToString(@"hh\:mm\:ss")
                    });
                }
                else // kontrakt
                {
                    reportIntervals.Add(new string[]
                    {
                intervalStart,
                intervalEnd,
                kvp.Key.ToString(),
                kvp.Value.ToString(@"hh\:mm\:ss")
                    });
                }
            }

            reportIntervals = reportIntervals.OrderBy(interval => DateTime.Parse(interval[0])).ToList();

            return reportIntervals;
        }
    }
}