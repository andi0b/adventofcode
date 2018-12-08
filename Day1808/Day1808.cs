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
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] { "66" };


        public override int[] GetPart1Input() => ReadInput().Split(' ')
                                                            .Select(int.Parse)
                                                            .ToArray();

        public override object SolvePart1(int[] input) => GetRootNode(input).MetadataSum;
        public override object SolvePart2(int[] input) => GetRootNode(input).Value;

        TreeItem GetRootNode(IEnumerable<int> input) => ParseTreeItem(input.GetEnumerator());

        TreeItem ParseTreeItem(IEnumerator<int> enumerator)
        {
            var childNodeCount = enumerator.TakeNext();
            var metadataLength = enumerator.TakeNext();

            var childNodes = (
                from id in Enumerable.Range(0, childNodeCount)
                select ParseTreeItem(enumerator)
            ).ToArray();

            var metadata = enumerator.Take(metadataLength)
                                     .ToArray();

            return new TreeItem(childNodes, metadata);
        }
    }

    class TreeItem
    {
        private readonly TreeItem[] _children;
        private readonly int[] _metadata;

        public TreeItem(TreeItem[] children, int[] metadata)
        {
            _children = children;
            _metadata = metadata;
        }

        public int MetadataSum
            => _metadata.Sum() + (
                   from child in _children
                   select child.MetadataSum
               ).Sum();

        public int Value
            => _children.Any()
                ? (from id in _metadata
                   let child = _children.ElementAtOrDefault(id - 1)
                   where child != null
                   select child.Value).Sum()
                : _metadata.Sum();
    }

    static class EnumeratorExtensions
    {
        public static IEnumerable<T> Take<T>(this IEnumerator<T> enumerator, int count)
        {
            for (int i = 0; i < count && enumerator.MoveNext(); i++)
                yield return enumerator.Current;
        }

        public static T TakeNext<T>(this IEnumerator<T> enumerator)
        {
            if (!enumerator.MoveNext()) throw new Exception("No next value!");
            return enumerator.Current;
        }
    }


    public class Day1808Test : TestBase<Day1808Solution, int[], int[]>
    {
        public Day1808Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
