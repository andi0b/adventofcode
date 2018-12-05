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

        private static bool DoReact(char a, char b)
        {
            var sameCharacter = char.ToLowerInvariant(a) == char.ToLowerInvariant(b);
            var differentPolarity = char.IsUpper(a) && char.IsLower(b) || char.IsLower(a) && char.IsUpper(b);
            return sameCharacter && differentPolarity;
        }

        private static List<char> DoReactions(IEnumerable<char> input)
        {
            var list = new List<char>();

            foreach (var next in input)
            {
                var previous = list.LastOrDefault();
                if (previous != default && DoReact(previous, next))
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

        private static List<char> DoAllReactions(List<char> reduced)
        {
            while (reduced.Count != (reduced = DoReactions(reduced)).Count){}
            return reduced;
        }

        public override object SolvePart1(string input) => DoAllReactions(input.ToList()).Count;

        public override object SolvePart2(string input)
        {
            var allUppercaseLetters = from charCode in Enumerable.Range(65, 26)
                                      select (char) charCode;

            var inputsWithoutOneChar = from uppercaseLetter in allUppercaseLetters
                                       let lowercaseLetter = char.ToLowerInvariant(uppercaseLetter)
                                       select input.Replace(uppercaseLetter.ToString(), string.Empty)
                                                   .Replace(lowercaseLetter.ToString(), string.Empty);

            var reduced = from i in inputsWithoutOneChar
                          select DoAllReactions(i.ToList()).Count;

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
