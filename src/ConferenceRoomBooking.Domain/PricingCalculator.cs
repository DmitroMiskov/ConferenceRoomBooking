using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Domain
{
    public static class PricingCalculator
    {
        public static decimal CalculateHallCost(decimal baseHourlyRate, DateTime start, DateTime end)
        {
            if (end <= start)
                throw new InvalidOperationException("Час закінчення має бути пізнішим за час початку.");

            decimal totalCost = 0m;
            var current = start;

            // Погодинний прохід часового проміжку
            while (current < end)
            {
                var nextHour = current.AddHours(1);
                var segmentEnd = nextHour > end ? end : nextHour;
                var durationInHours = (decimal)(segmentEnd - current).TotalHours;

                var hourOfDay = current.Hour;
                decimal multiplier = 1.0m;

                if (hourOfDay >= 6 && hourOfDay < 9)
                {
                    multiplier = 0.9m; // Ранкові (-10%)
                }
                else if (hourOfDay >= 12 && hourOfDay < 14)
                {
                    multiplier = 1.15m; // Пікові (+15%)
                }
                else if (hourOfDay >= 18 && hourOfDay < 23)
                {
                    multiplier = 0.8m; // Вечірні (-20%)
                }
                else
                {
                    multiplier = 1.0m; // Стандартні
                }

                totalCost += baseHourlyRate * multiplier * durationInHours;
                current = segmentEnd;
            }

            return Math.Round(totalCost, 2);
        }
    }
}
