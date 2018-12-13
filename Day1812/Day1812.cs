using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Xunit.Abstractions;

namespace AdventOfCode.Day1812
{
    public class Day1812Solution : SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[]{ ReadInput("_Example.txt") };
        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"325"};

        public override string GetPart1Input()=>ReadInput();

        public override object SolvePart1(string input) => Solve(input, 20);
        public override object SolvePart2(string input) => Solve2(input, 50_000_000_000);

        public int Solve(string input, long generations)
        {
            var instructions = new Instructions(input);

            ArraySegment<bool> state = instructions.InitialState;
            var offset = 0;

            for (var i = 0; i < generations; i++)
            {
                var (offsetDelta, newstate) = CalculateNextGeneration(instructions, state);
                state = newstate;
                offset += offsetDelta;
            }

            return PlantSum(state, offset);
        }

        public long Solve2(string input, long generations)
        {
            var instructions = new Instructions(input);

            ArraySegment<bool> state = instructions.InitialState;
            var offset = 0;

            var plantSum = PlantSum(state, offset);
            var diffs = new List<int>();
            for (var i = 0; i < generations; i++)
            {
                var (offsetDelta, newstate) = CalculateNextGeneration(instructions, state);
                state = newstate;
                offset += offsetDelta;

                var newPlantSum = PlantSum(state, offset);
                diffs.Add(newPlantSum - plantSum);
                plantSum = newPlantSum;

                var lastValues = diffs.TakeLast(10).ToArray();
                if (lastValues.Length == 10 && lastValues.Distinct().Count() == 1)
                {
                    // assume linear progression
                    var generationsLeft = generations - i - 1;
                    var incrementPerGeneration = diffs.Last();
                    return plantSum + incrementPerGeneration * generationsLeft;
                }
            }

            return PlantSum(state, offset);
        }

        private static int PlantSum(ArraySegment<bool> state, int offset)
        {
            return state.Select((x, i) => (value: x, index: i - offset))
                        .Where(x => x.value)
                        .Sum(x => x.index);
        }

        private (int offset, ArraySegment<bool> newState) CalculateNextGeneration(Instructions instructions, ArraySegment<bool> state)
        {
            const int offset = 2;
            var newState = new bool[state.Count + 4];

            for (var i = 0; i < newState.Length; i++)
            {
                var sliceStart = i - offset - 2;
                var sliceEnd = i - offset + 2;
                var padLeft = sliceStart < 0 ? -sliceStart : 0;
                var padRight = sliceEnd > state.Count - 1 ? sliceEnd - (state.Count - 1) : 0;

                var pattern = state.Slice(sliceStart + padLeft, sliceEnd - (sliceStart + padLeft) - padRight + 1).ToArray();
                if (padLeft > 0) pattern = Enumerable.Repeat(false, padLeft).Concat(pattern).ToArray();
                if (padRight > 0) pattern = pattern.Concat(Enumerable.Repeat(false, padRight)).ToArray();

                newState[i] = instructions.InstructionDict[instructions.PatternToInt(pattern)];
            }

            var shrinkLeft = newState.TakeWhile(x => !x).Count();
            var shrinkRight = newState.Reverse().TakeWhile(x => !x).Count();

            return (offset - shrinkLeft, new ArraySegment<bool>(newState, shrinkLeft, newState.Length - shrinkRight - shrinkLeft));
        }
    }

    class Instructions
    {
        public Instructions(string input)
        {
            bool ParseChar(char c) => c == '#';
            bool[] ParseString(string s) => s.Select(ParseChar).ToArray();

            var initialState = Regex.Matches(input, @"initial state: (?<initialState>.*)").First().Groups["initialState"].Value;
            var patternMatches = Regex.Matches (input, @"(?<pattern>.{5}) => (?<nextState>.)");
            
            var instructions = from match in patternMatches
                               let pattern = ParseString(match.Groups["pattern"].Value)
                               let nextState = ParseChar( match.Groups["nextState"].Value[0])
                               select (pattern, nextState);

            InitialState = ParseString(initialState);
            AllInstructions = instructions.ToArray();

            InstructionDict = AllInstructions.ToDictionary(x => PatternToInt(x.pattern), x => x.nextState);

        }

        public Dictionary<int, bool> InstructionDict { get; }

        public (bool[] pattern, bool nextState)[] AllInstructions { get; }
        public bool[] InitialState { get; }

        public bool GetNextState(bool[] pattern) => AllInstructions.FirstOrDefault(x => x.pattern.SequenceEqual(pattern)).nextState;

        public int PatternToInt(bool[] pattern) => (pattern[0] ? 1 : 0) |
                                                   (pattern[1] ? 1 : 0) << 1 |
                                                   (pattern[2] ? 1 : 0) << 2 |
                                                   (pattern[3] ? 1 : 0) << 3 |
                                                   (pattern[4] ? 1 : 0) << 4;
    }

    public class Day1812Test : TestBase<Day1812Solution, string, string>
    {
        public Day1812Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
