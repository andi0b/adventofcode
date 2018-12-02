using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode
{
    public abstract class SolutionBase
    {

    }
    
    public abstract class SolutionBase<TPart1,TPart2> : SolutionBase
    {
        public virtual IEnumerable<TPart1> GetPart1SampleInputs() => Enumerable.Empty<TPart1>();

        public virtual IEnumerable<TPart2> GetPart2SampleInputs()
            => typeof(TPart1) == typeof(TPart2)
                ? (IEnumerable<TPart2>) GetPart1SampleInputs()
                : Enumerable.Empty<TPart2>();

        public virtual IEnumerable<string> GetPart1SampleOutputs() => Enumerable.Empty<string>();

        public virtual IEnumerable<string> GetPart2SampleOutputs() => Enumerable.Empty<string>();

        public virtual TPart1 GetPart1Input() => default;

        public virtual TPart2 GetPart2Input()
            => typeof(TPart1) == typeof(TPart2)
                ? (TPart2) (object) GetPart1Input()
                : default;

        public virtual string SolvePart1(TPart1 input) => null;

        public virtual string SolvePart2(TPart2 input) => null;
    }

    public abstract class TestBase<TSolution, TPart1, TPart2> where TSolution : SolutionBase<TPart1, TPart2>, new()
    {
        private readonly ITestOutputHelper _outputHelper;
        private SolutionBase<TPart1, TPart2> _solution = new TSolution();

        public TestBase(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> Part1SampleData()
        {
            var solution = new TSolution();
            var inputs = solution.GetPart1SampleInputs();
            var outputs = solution.GetPart1SampleOutputs();
            if (!outputs.Any()) return new List<object[]> {new object[] {default(TPart1), null}};
            return inputs.Zip(outputs, (i, o) => new object[]{i, o});
        }

        public static IEnumerable<object[]> Part2SampleData()
        {
            var solution = new TSolution();
            var inputs = solution.GetPart2SampleInputs();
            var outputs = solution.GetPart2SampleOutputs();
            if (!outputs.Any()) return new List<object[]> {new object[] {default(TPart2), null}};
            return inputs.Zip(outputs, (i, o) => new object[]{i, o});
        }

        [SkippableFact]
        public void Part1()
        {
            var input = _solution.GetPart1Input();
            Skip.If(input == null, "No Input");
            var output = _solution.SolvePart1(input);
            Skip.If(output == null);
            _outputHelper.WriteLine("Solution Part 1: " + output);
        }

        [SkippableFact]
        public void Part2()
        {
            var input = _solution.GetPart2Input();
            Skip.If(input == null, "No Input");
            var output = _solution.SolvePart2(input);
            Skip.If(output == null);
            _outputHelper.WriteLine("Solution Part 2: " + output);
        }

        [SkippableTheory]
        [MemberData(nameof(Part1SampleData))]
        public void Part1Tests(TPart1 input, string expectedOutput)
        {
            Skip.If(expectedOutput == null);
            var output = _solution.SolvePart1(input);
            Skip.If(output == null);
            Assert.Equal(expectedOutput, output);
        }

        [SkippableTheory]
        [MemberData(nameof(Part2SampleData))]
        public void Part2Tests(TPart2 input, string expectedOutput)
        {
            Skip.If(expectedOutput == null);
            var output = _solution.SolvePart2(input);
            Skip.If(output == null);
            Assert.Equal(expectedOutput, output);
        }



    }
}
