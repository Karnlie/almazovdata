using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphVis.Models.Medicine;

namespace GraphVis.Helpers
{
    class HelperDate
    {
        public static DateTime GetFirstDayInMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Date.Year, dateTime.Date.Month, 1);
        }

        public static List<IGrouping<DateTime, Visit>> GetGroupByMonth(List<Visit> visits)
        {
            return visits
            .GroupBy(x => GetFirstDayInMonth(x.date))
            .ToList();
        }


        public static Dictionary<string, List<Visit>> getVisitsByDate(List<Visit> visits, String datePattern)
        {
            var visitByDate = visits.GroupBy(visit => visit.date.ToString(datePattern)).ToDictionary(g => g.Key, g => g.ToList());
            return visitByDate;
        }

        public static Dictionary<string, List<Visit>> getVisitsByWeek(List<Visit> visits)
        {
            Func<DateTime, int> weekProjector =
            d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    d,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Sunday);
            var visitsByDate = visits.GroupBy(p => weekProjector(p.date).ToString()).ToDictionary(g => g.Key, g => g.ToList());
            return visitsByDate;
        }

    }
}
