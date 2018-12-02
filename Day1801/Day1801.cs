using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1801
{
    public class Day1801Solution : SolutionBase<int[], int[]>
    {
        public override IEnumerable<int[]> GetPart1SampleInputs() => new[]
                                                                     {
                                                                         new[] {+1, +1, +1},
                                                                         new[] {+1, +1, -2},
                                                                         new[] {-1, -2, -3},
                                                                     };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[]
                                                                       {
                                                                           3,
                                                                           0,
                                                                           -6
                                                                       }.Select(x => x.ToString());


        public override IEnumerable<int[]> GetPart2SampleInputs() => new[]
                                                                     {
                                                                         new[] {+1, -1},
                                                                         new[] {+3, +3, +4, -2, -4},
                                                                         new[] {-6, +3, +8, +5, -6},
                                                                         new[] {+7, +7, -2, -7, -4},
                                                                     };

        public override IEnumerable<string> GetPart2SampleOutputs() => new[]
                                                                       {
                                                                           0,
                                                                           10,
                                                                           5,
                                                                           14
                                                                       }.Select(x => x.ToString());


        public override int[] GetPart1Input() => ReadInputLines().Select(int.Parse).ToArray();

        public override object SolvePart1(int[] input) => input.Sum();

        public override object SolvePart2(int[] input)
        {
            var result = 0;
            var pastResults = new HashSet<int> {0};
            while (true)
                foreach (var i in input)
                {
                    result += i;

                    if (!pastResults.Add(result))
                        return result;
                }
        }
    }

    public class Day1801Test : TestBase<Day1801Solution, int[],int[]>
    {
        public Day1801Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
