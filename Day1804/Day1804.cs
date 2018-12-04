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

        internal static IEnumerable<(DateTime date, int guard, IEnumerable<(int asleepMinute, int wakeUpMinute)> sleepTimes)> CalculateTimesheet(string observation)
        {
            var orderedObservations = ParseObservations(observation).OrderBy(x => x.timestamp);

            var shifts = from ob in orderedObservations
                         group ob by ob.timestamp.Hour != 0
                             ? ob.timestamp.AddDays(1).Date
                             : ob.timestamp.Date;

            return from shift in shifts
                   select (
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
        }

        public override object SolvePart1(string input) => SolvePart1And2(input).part1;
        public override object SolvePart2(string input) => SolvePart1And2(input).part2;

        private (int part1, int part2) SolvePart1And2(string input)
        {
            var timesheet = CalculateTimesheet(input).ToList();

            var guards = (from day in timesheet select day.guard)
                         .Distinct()
                         .ToArray();


            var guardsTotalSleepTimes = from guard in guards
                                        select (guard,
                                                sleepTime: (
                                                    from day in timesheet
                                                    where day.guard == guard
                                                    select day.sleepTimes
                                                ).SelectMany(x => x)
                                                 .Aggregate(0, (totalSleepTime, next) => totalSleepTime + next.wakeUpMinute - next.asleepMinute)
                                            );

            var sleepiestGuard = guardsTotalSleepTimes.OrderByDescending(x => x.sleepTime)
                                                      .First()
                                                      .guard;

            var minutes = Enumerable.Range(0, 60);
            var guardMinutes = from guard in guards
                               join minute in minutes on true equals true
                               select (guard, minute);

            var sleepTimesPerGuardMinute = (
                from guardMinute in guardMinutes
                select (guardMinute,
                        asleepCount: (
                            from day in timesheet
                            where day.guard == guardMinute.guard
                            where day.sleepTimes.Any(sleepTime => sleepTime.asleepMinute <= guardMinute.minute && sleepTime.wakeUpMinute > guardMinute.minute)
                            select day
                        ).Count()
                    )
                ).ToArray();

            var mostSleepMinuteOfSleepiestGuard = sleepTimesPerGuardMinute.Where(x => x.guardMinute.guard == sleepiestGuard)
                                                                          .Aggregate((a, b) => a.asleepCount > b.asleepCount ? a : b);

            var mostSleepMinute = sleepTimesPerGuardMinute.Aggregate((a, b) => a.asleepCount > b.asleepCount ? a : b);

            return (
                mostSleepMinuteOfSleepiestGuard.guardMinute.minute * sleepiestGuard,
                mostSleepMinute.guardMinute.minute * mostSleepMinute.guardMinute.guard
            );
        }
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
            var parsed = Day1804Solution.ParseObservations(input).Single();

            Assert.Equal(timestamp, parsed.timestamp);
            Assert.Equal(guardId, parsed.guardId);
            Assert.Equal(asleep, parsed.asleep);
        }
    }
}
