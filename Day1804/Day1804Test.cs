using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1804
{
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
}