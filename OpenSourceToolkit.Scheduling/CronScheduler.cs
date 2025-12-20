using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSourceToolkit.Scheduling
{
    public class CronScheduler : IDisposable
    {
        private readonly CrontabSchedule _schedule;
        private readonly Action _action;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        public CronScheduler(string cronExpression, Action action)
        {
            _schedule = CrontabSchedule.Parse(cronExpression);
            _action = action;
        }

        public static IEnumerable<DateTime> GetNextOccurrences(string cronExpression, int count)
        {
            var schedule = CrontabSchedule.Parse(cronExpression);
            var start = DateTime.Now;
            return schedule.GetNextOccurrences(start, start.AddYears(1)).Take(count);
        }

        public static string GetDescription(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return "Empty expression";

            var parts = expression.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return "Invalid expression format (must have 5 parts)";

            var minute = parts[0];
            var hour = parts[1];
            var day = parts[2];
            var month = parts[3];
            var weekday = parts[4];

            var description = "Runs ";

            // Minute
            if (minute == "*")
                description += "every minute";
            else if (minute.StartsWith("*/"))
                description += $"every {minute.Substring(2)} minutes";
            else
                description += $"at minute {minute}";

            // Hour
            if (hour != "*")
            {
                if (hour.StartsWith("*/"))
                    description += $", every {hour.Substring(2)} hours";
                else
                {
                    if (int.TryParse(hour, out int h))
                    {
                        var ampm = h >= 12 ? "PM" : "AM";
                        var displayHour = h == 0 ? 12 : (h > 12 ? h - 12 : h);
                        description += $" at {displayHour}:00 {ampm}";
                    }
                    else
                    {
                        description += $" at hour {hour}";
                    }
                }
            }

            // Day
            if (day != "*")
            {
                if (day.StartsWith("*/"))
                    description += $", every {day.Substring(2)} days";
                else
                    description += $" on day {day}";
            }

            // Month
            if (month != "*")
            {
                var months = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                if (int.TryParse(month, out int m) && m >= 1 && m <= 12)
                    description += $" in {months[m]}";
                else
                    description += $" in month {month}";
            }

            // Weekday
            if (weekday != "*")
            {
                var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                if (weekday == "1-5")
                    description += " on weekdays";
                else if (weekday == "0,6")
                    description += " on weekends";
                else if (int.TryParse(weekday, out int w) && w >= 0 && w <= 6)
                    description += $" on {days[w]}";
                else
                    description += $" on weekday {weekday}";
            }

            return description;
        }

        public void Start()
        {
            if (_runningTask != null) return;

            _cts = new CancellationTokenSource();
            _runningTask = RunLoopAsync(_cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _runningTask = null;
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var nextOccurrence = _schedule.GetNextOccurrence(DateTime.Now);
                var delay = nextOccurrence - DateTime.Now;

                if (delay.TotalMilliseconds > 0)
                {
                    try
                    {
                        await Task.Delay(delay, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

                if (!token.IsCancellationRequested)
                {
                    try
                    {
                        _action?.Invoke();
                    }
                    catch
                    {
                        // Log or swallow exception to keep scheduler alive
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
