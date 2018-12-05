using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1805
{
    public class Day1805Solution : SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[] {"dabAcCaCBAcCcaDA"};
        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"10"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"4"};
        public override string GetPart1Input() => ReadInput();

        private static bool CanReact(char a, char b)
            => a != b && char.ToLowerInvariant(a) == char.ToLowerInvariant(b);

        private static List<char> DoReactions(IEnumerable<char> input)
        {
            var list = new List<char>();

            foreach (var next in input)
            {
                var previous = list.LastOrDefault();
                if (previous != default && CanReact(previous, next))
                {
                    list.RemoveAt(list.Count - 1);
                }
                else
                {
                    list.Add(next);
                }
            }

            return list;
        }

        public override object SolvePart1(string input) => DoReactions(input.ToList()).Count;

        public override object SolvePart2(string input)
        {
            var allUppercaseLetters = from charCode in Enumerable.Range('A', 26)
                                      select ((char) charCode).ToString();

            var inputsWithoutOneChar = from uppercaseLetter in allUppercaseLetters
                                       select input.Replace(uppercaseLetter, string.Empty, StringComparison.InvariantCultureIgnoreCase);

            var reduced = from i in inputsWithoutOneChar
                          select DoReactions(i.ToList()).Count;

            return reduced.Min(x => x);
        }
    }

    public class Day1805Test : TestBase<Day1805Solution, string, string>
    {
        public Day1805Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
