using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1808
{
    public class Day1808Solution : SolutionBase<int[], int[]>
    {
        public override IEnumerable<int[]> GetPart1SampleInputs() => new[] {new[] {2, 3, 0, 3, 10, 11, 12, 1, 1, 0, 1, 99, 2, 1, 1, 2}};
        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"138"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"66"};


        public override int[] GetPart1Input() => ReadInput().Split(' ')
                                                            .Select(int.Parse)
                                                            .ToArray();

        public override object SolvePart1(int[] input) => GetRootNode(input).MetadataSum;
        public override object SolvePart2(int[] input) => GetRootNode(input).Value;

        private TreeItem GetRootNode(int[] input) => ParseTreeItem(input, out var _);

        private TreeItem ParseTreeItem(ArraySegment<int> data, out ArraySegment<int> remainingData)
        {
            var childNodeCount = data[0];
            var metadataLength = data[1];

            data = data.Slice(2);

            var childNodes = (
                from id in Enumerable.Range(0, childNodeCount)
                select ParseTreeItem(data, out data)
            ).ToArray();

            var metadata = data.Slice(0, metadataLength);

            remainingData = data.Slice(metadataLength);

            return new TreeItem {Children = childNodes, Metadata = metadata};
        }
    }

    class TreeItem
    {
        public TreeItem[] Children { get; set; }
        public ArraySegment<int> Metadata { get; set; }

        public int MetadataSum
            => Metadata.Sum() + (
                   from child in Children
                   select child.MetadataSum
               ).Sum();

        public int Value
            => Children.Any()
                ? (from id in Metadata
                   let child = Children.ElementAtOrDefault(id - 1)
                   where child != null
                   select child.Value).Sum()
                : Metadata.Sum();
    }

    public class Day1808Test : TestBase<Day1808Solution, int[], int[]>
    {
        public Day1808Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
