using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace AdventOfCode.Day1804
{
    public class Day1804Solution : SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[]
                                                                      {
                                                                          string.Join(Environment.NewLine,
                                                                                      "[1518-11-01 00:00] Guard #10 begins shift",
                                                                                      "[1518-11-01 00:05] falls asleep",
                                                                                      "[1518-11-01 00:25] wakes up",
                                                                                      "[1518-11-01 00:30] falls asleep",
                                                                                      "[1518-11-01 00:55] wakes up",
                                                                                      "[1518-11-01 23:58] Guard #99 begins shift",
                                                                                      "[1518-11-02 00:40] falls asleep",
                                                                                      "[1518-11-02 00:50] wakes up",
                                                                                      "[1518-11-03 00:05] Guard #10 begins shift",
                                                                                      "[1518-11-03 00:24] falls asleep",
                                                                                      "[1518-11-03 00:29] wakes up",
                                                                                      "[1518-11-04 00:02] Guard #99 begins shift",
                                                                                      "[1518-11-04 00:36] falls asleep",
                                                                                      "[1518-11-04 00:46] wakes up",
                                                                                      "[1518-11-05 00:03] Guard #99 begins shift",
                                                                                      "[1518-11-05 00:45] falls asleep",
                                                                                      "[1518-11-05 00:55] wakes up"
                                                                          )
                                                                      };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"240"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"4455"};

        public override string GetPart1Input() => ReadInput();


        public override object SolvePart1(string input)
        {
            var timeSheet = TimeSheet.CalculateTimesheet(input);

            var guardsTotalSleepTimes = from guard in timeSheet.AllGuardIds()
                                        select (guard, sleepTime: timeSheet.GuardTotalSleepMinutes(guard));

            var sleepiestGuard = guardsTotalSleepTimes.OrderByDescending(x => x.sleepTime)
                                                      .First()
                                                      .guard;

            var sleepMatrix = timeSheet.DaysAsleepPerMinute(sleepiestGuard);

            var sleepiestMinute = sleepMatrix.OrderByDescending(x => x.asleepCount)
                                             .First();

            return sleepiestGuard * sleepiestMinute.minute;
        }

        public override object SolvePart2(string input)
        {
            var timeSheet = TimeSheet.CalculateTimesheet(input);

            var sleepTimesPerGuardMinute = from guard in timeSheet.AllGuardIds()
                                           from st in timeSheet.DaysAsleepPerMinute(guard)
                                           select (guard, st.minute, st.asleepCount);

            var sleepiestMinute = sleepTimesPerGuardMinute.OrderByDescending(x => x.asleepCount)
                                                          .First();

            return sleepiestMinute.minute * sleepiestMinute.guard;
        }
    }

    internal class TimeSheet
    {
        public TimeSheetEntry[] Entries { get; }

        private TimeSheet(IEnumerable<TimeSheetEntry> entries)
        {
            Entries = entries.ToArray();
        }

        public IEnumerable<int> AllGuardIds() => (from day in Entries select day.Guard).Distinct();

        public int GuardTotalSleepMinutes(int guard)
        {
            var sleepTimes = from day in Entries
                             where day.Guard == guard
                             from sleepTime in day.SleepTimes
                             select sleepTime;

            return sleepTimes.Aggregate(0, (totalSleepTime, next) => totalSleepTime + next.wakeUpMinute - next.asleepMinute);
        }

        public int DaysAsleepAt(int guard, int minute)
        {
            var sleepingDays = from day in Entries
                               where day.Guard == guard
                               where day.SleepTimes.Any(sleepTime => sleepTime.asleepMinute <= minute && sleepTime.wakeUpMinute > minute)
                               select day;

            return sleepingDays.Count();
        }

        public IEnumerable<(int minute, int asleepCount)> DaysAsleepPerMinute(int guard)
        {
            var minutes = Enumerable.Range(0, 60);

            return from minute in minutes
                   select (minute, asleepCount: DaysAsleepAt(guard, minute));
        }

        internal static TimeSheet CalculateTimesheet(string observation)
        {
            var observations = ParseObservations(observation);

            var shifts = from ob in observations
                         orderby ob.timestamp
                         group ob by ob.timestamp.Hour != 0
                             ? ob.timestamp.AddDays(1).Date
                             : ob.timestamp.Date;

            var entries = from shift in shifts
                          select new TimeSheetEntry(

                              date: shift.Key,

                              guard: (
                                  from s in shift
                                  where s.guardId != null
                                  select s.guardId.Value
                              ).First(),

                              sleepTimes: Enumerable.Zip(
                                  from s in shift where s.asleep == true select s.timestamp.Minute,
                                  from s in shift where s.asleep == false select s.timestamp.Minute,
                                  (x, y) => (asleepMinute: x, wakeUpMinute: y))
                          );

            return new TimeSheet(entries);
        }

        internal static IEnumerable<(DateTime timestamp, int? guardId, bool? asleep)> ParseObservations(string observation)
        {
            var matches = Regex.Matches(observation,
                                        @"\[(?<ts>.*)\] (Guard #(?<guardId>\d*) begins shift|(?<asleep>falls asleep)|(?<wake>wakes up))",
                                        RegexOptions.ExplicitCapture);

            return from m in matches
                   select (
                       DateTime.Parse(m.Groups["ts"].Value),
                       m.Groups["guardId"].Success ? int.Parse(m.Groups["guardId"].Value) : (int?) null,
                       m.Groups["asleep"].Success ? true : m.Groups["wake"].Success ? false : (bool?) null
                   );
        }
    }

    internal class TimeSheetEntry
    {
        public TimeSheetEntry(DateTime date, int guard, IEnumerable<(int asleepMinute, int wakeUpMinute)> sleepTimes)
        {
            Date = date;
            Guard = guard;
            SleepTimes = sleepTimes.ToArray();
        }

        public DateTime Date { get; }
        public int Guard { get; }
        public IEnumerable<(int asleepMinute, int wakeUpMinute)> SleepTimes { get; }
    }
}
