using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

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

        public override IEnumerable<string> GetPart1SampleOutputs() => new []{"240"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new []{"4455"};
        
        public override string GetPart1Input() => ReadInput();


        public override object SolvePart1(string input)
        {
            var timeSheet = TimeSheet.CalculateTimesheet(input);

            var guardsTotalSleepTimes = from guard in timeSheet.AllGuardIds()
                                        select (guard, sleepTime: timeSheet.GuardTotalSleepMinutes(guard));

            var sleepiestGuard = guardsTotalSleepTimes.OrderByDescending(x => x.sleepTime)
                                                      .First()
                                                      .guard;

            var sleepMatrix = timeSheet.SleepTimePerMinute(sleepiestGuard);

            var sleepiestMinute = sleepMatrix.Aggregate((a, b) => a.asleepCount > b.asleepCount ? a : b);

            return sleepiestGuard * sleepiestMinute.minute;

        }

        public override object SolvePart2(string input)
        {
            var timeSheet = TimeSheet.CalculateTimesheet(input);

            var sleepTimesPerGuardMinute = (
                from guard in timeSheet.AllGuardIds()
                select (
                    from st in timeSheet.SleepTimePerMinute(guard)
                    select (guard, st.minute, st.asleepCount)
                )
            ).SelectMany(x => x);

            var sleepiestMinute = sleepTimesPerGuardMinute.Aggregate((a, b) => a.asleepCount > b.asleepCount ? a : b);

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

        public IEnumerable<int> AllGuardIds()
            => (from day in Entries select day.Guard)
               .Distinct()
               .ToArray();


        public int GuardTotalSleepMinutes(int guard)
            => (
                    from day in Entries
                    where day.Guard == guard
                    select day.SleepTimes
                ).SelectMany(x => x)
                 .Aggregate(
                     0,
                     (totalSleepTime, next) => totalSleepTime + next.wakeUpMinute - next.asleepMinute);


        public int DaysAsleepAt(int guard, int minute)
            => (
                from day in Entries
                where day.Guard == guard
                where day.SleepTimes.Any(sleepTime => sleepTime.asleepMinute <= minute && sleepTime.wakeUpMinute > minute)
                select day
            ).Count();


        public (int minute, int asleepCount)[] SleepTimePerMinute(int guard)
        {
            var minutes = Enumerable.Range(0, 60);

            return (
                from minute in minutes
                select (
                    minute,
                    asleepCount: DaysAsleepAt(guard, minute)
                )
            ).ToArray();
        }


        internal static TimeSheet CalculateTimesheet(string observation)
        {
            var orderedObservations = ParseObservations(observation).OrderBy(x => x.timestamp);

            var shifts = from ob in orderedObservations
                         group ob by ob.timestamp.Hour != 0
                             ? ob.timestamp.AddDays(1).Date
                             : ob.timestamp.Date;

            var entries = from shift in shifts
                          select new TimeSheetEntry(
                              date: shift.Key,
                              guard: shift.First(x => x.guardId.HasValue).guardId.Value,
                              sleepTimes: (
                                  (from s in shift where s.asleep == true select s)
                                  .Zip(
                                      from s in shift where s.asleep == false select s,
                                      (x, y) => (
                                          asleepMinute: x.timestamp.Minute,
                                          wakeUpMinute: y.timestamp.Minute
                                      )
                                  )
                              )
                          );

            return new TimeSheet(entries);
        }

        internal static IEnumerable<(DateTime timestamp, int? guardId, bool? asleep)> ParseObservations(string observation)
        {
            var matches = Regex.Matches(observation,
                                        @"\[(?<ts>.*)\] (Guard #(?<guardId>\d*) begins shift|(?<asleep>falls asleep)|(?<wake>wakes up))",
                                        RegexOptions.ExplicitCapture);

            return from m in matches
                   select
                   (
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

    public class Day1804Test : TestBase<Day1804Solution, string, string>
    {
        public Day1804Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public static object[][] ParserData =
        {
            new object[] {"[1518-11-01 00:00] Guard #10 begins shift", new DateTime(1518, 11, 1, 0, 0, 0), 10, null},
            new object[] {"[1518-11-01 00:05] falls asleep", new DateTime(1518, 11, 1, 0, 5, 0), null, true},
            new object[] {"[1518-11-01 00:25] wakes up", new DateTime(1518, 11, 1, 0, 25, 0), null, false},
            new object[] {"[1518-11-05 00:03] Guard #99 begins shift", new DateTime(1518, 11, 5, 0, 3, 0), 99, null},
        };

        [Theory, MemberData(nameof(ParserData))]
        public void TestParser(string input, DateTime timestamp, int? guardId, bool? asleep)
        {
            var parsed = TimeSheet.ParseObservations(input).Single();

            Assert.Equal(timestamp, parsed.timestamp);
            Assert.Equal(guardId, parsed.guardId);
            Assert.Equal(asleep, parsed.asleep);
        }

    }

    public class Day1804_TimesheetTests
    {
        public Day1804_TimesheetTests()
        {
            var observations = new Day1804Solution().GetPart1SampleInputs().First();
            TimeSheet = TimeSheet.CalculateTimesheet(observations);
        }

        internal TimeSheet TimeSheet { get; }

        [Theory]
        [InlineData(10, 50)]
        [InlineData(99, 30)]
        public void TotalSleepTime(int guard, int totalSleepMinutes)
        {
            var calculated = TimeSheet.GuardTotalSleepMinutes(guard);
            Assert.Equal(totalSleepMinutes, calculated);
        }

        [Fact]
        public void Guard10_SleepTime()
        {
            var sleepMinutes = TimeSheet.SleepTimePerMinute(10);

            Assert.Equal(2, sleepMinutes.Where(x => x.minute == 24)
                                        .Select(x => x.asleepCount)
                                        .Single());

            Assert.All(sleepMinutes.Where(x => x.minute != 24)
                                   .Select(x => x.asleepCount),
                       i => Assert.InRange(i, 0, 1));
        }
    }
}
