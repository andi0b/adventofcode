using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1802
{
    public class Day1802Solution : SolutionBase<string[],string[]>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs() => new List<string[]>
                                                                        {
                                                                            new []
                                                                            {
                                                                                "abcdef",
                                                                                "bababc",
                                                                                "abbcde",
                                                                                "abcccd",
                                                                                "aabcdd",
                                                                                "abcdee",
                                                                                "ababab",
                                                                            }
                                                                        };

        
        public override IEnumerable<string> GetPart1SampleOutputs() => new[]
                                                                       {
                                                                           "12"
                                                                       };

        public override IEnumerable<string[]> GetPart2SampleInputs() => new List<string[]>
                                                                        {
                                                                            new []
                                                                            {
                                                                                "abcde",
                                                                                "fghij",
                                                                                "klmno",
                                                                                "pqrst",
                                                                                "fguij",
                                                                                "axcye",
                                                                                "wvxyz",
                                                                            }
                                                                        };

        public override IEnumerable<string> GetPart2SampleOutputs() => new[]
                                                                       {
                                                                           "Found one different char between elements 'fghij' and 'fguij'. PuzzleOutput: 'fgij'"
                                                                       };


        public override string[] GetPart1Input() => ReadInputLines();

        internal static (bool twoTimes, bool threeTimes) CountDuplicates<T>(IEnumerable<T> elements)
        {
            var duplicates = (
                from e in elements
                group e by e
                into g
                select g.Count()
            ).ToArray();

            return (duplicates.Any(x => x == 2), duplicates.Any(x => x == 3));
        }

        public override object SolvePart1(string[] input)
        {
            var duplicates = input.Select(CountDuplicates).ToArray();
            return duplicates.Count(x => x.twoTimes) * duplicates.Count(x => x.threeTimes);
        }

        internal static int GetDifferenceCount(string a, string b)
            => a.Zip(b, ValueTuple.Create)
                .Count(x => x.Item1 != x.Item2);

        public override object SolvePart2(string[] input)
        {
            string RemoveDiffChars(string a, string b) => string.Concat(
                a.Zip(b, ValueTuple.Create)
                 .Where(x => x.Item1 == x.Item2)
                 .Select(x=>x.Item1)
            );
                    
            foreach (var item in input)
            {
                var oneDiffElement = input.FirstOrDefault(i => GetDifferenceCount(item, i) == 1);
                
                if (oneDiffElement != default)
                {
                    var output = RemoveDiffChars(item, oneDiffElement);

                    return $"Found one different char between elements '{item}' and '{oneDiffElement}'. " +
                           $"PuzzleOutput: '{output}'";
                }
            }

            throw new Exception("No element found");
        }
    }

    public class Day1802Test : TestBase<Day1802Solution, string[],string[]>
    {
        public Day1802Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData("abcdef", false, false)] // contains no letters that appear exactly two or three times.
        [InlineData("bababc", true, true)]   // contains two a and three b, so it counts for both
        [InlineData("abbcde", true, false)]  // contains two b, but no letter appears exactly three times.
        [InlineData("abcccd", false, true)]  // contains three c, but no letter appears exactly two times.
        [InlineData("aabcdd", true, false)]  // contains two a and two d, but it only counts once.
        [InlineData("abcdee", true, false)]  // contains two e.
        [InlineData("ababab", false, true)]  // contains three a and three b, but it only counts once.
        public void Part1_CountDuplicates(string input, bool twoTimes, bool threeTimes)
        {
            var result = Day1802Solution.CountDuplicates(input);
            Assert.Equal(twoTimes, result.twoTimes);
            Assert.Equal(threeTimes, result.threeTimes);
        }

        [Theory]
        [InlineData("abcde", "axcye", 2)]
        [InlineData("fghij", "fguij", 1)]
        public void Part2_DifferenceCount(string a, string b, int count)
        {
            Assert.Equal(Day1802Solution.GetDifferenceCount(a,b), count);
        }
    }
}
