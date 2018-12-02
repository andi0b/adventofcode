using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1801
{
    public class Day1801Solution : SolutionBase<int[], int[]>
    {
        public override int[] GetPart1Input() => ReadInputLines().Select(int.Parse).ToArray();

        public override object SolvePart1(int[] input)
        {
            var result = 0;
            foreach (var i in input)
                result += i;

            return result;
        }

        public override object SolvePart2(int[] input)
        {
            var pastResults = new List<int>();
            var result = 0;
            while (true)
                foreach (var i in input)
                {
                    result += i;
                    if (pastResults.Contains(result))
                    {
                        return result;
                    }

                    pastResults.Add(result);
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
