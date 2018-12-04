using System.Linq;
using Xunit;

namespace AdventOfCode.Day1804
{
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
            var sleepMinutes = TimeSheet.DaysAsleepPerMinute(10);

            Assert.Equal(2, sleepMinutes.Where(x => x.minute == 24)
                                        .Select(x => x.asleepCount)
                                        .Single());

            Assert.All(sleepMinutes.Where(x => x.minute != 24)
                                   .Select(x => x.asleepCount),
                       i => Assert.InRange(i, 0, 1));
        }
    }
}