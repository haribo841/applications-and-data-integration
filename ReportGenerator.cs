using Evaluation_task_3;
using System;
using System.Collections.Generic;

internal class ReportGenerator : IReportGenerator
{
        public List<string[]> GenerateReport(List<Interval> interval)
        {
            List<string[]> intervals = new List<string[]>();

            foreach (var reportInterval in interval)
            {
                DateTime reportStartDate = TimeZoneInfo.ConvertTimeToUtc(reportInterval.StartDate, TimeZoneInfo.FindSystemTimeZoneById(reportInterval.TimeZone));
                DateTime reportEndDate = TimeZoneInfo.ConvertTimeToUtc(reportInterval.EndDate, TimeZoneInfo.FindSystemTimeZoneById(reportInterval.TimeZone));
                intervals.Add(new string[]
                {
                reportStartDate.ToString("yyyy-MM-ddTHH:mm:ss zzz"),
                reportEndDate.ToString("yyyy-MM-ddTHH:mm:ss zzz")
                });
            }

            return intervals;
        }

    internal List<string[]> GenerateReport(Interval interval)
    {
        throw new NotImplementedException();
    }

    List<string[]> IReportGenerator.GenerateReport(Interval interval)
    {
        throw new NotImplementedException();
    }
}