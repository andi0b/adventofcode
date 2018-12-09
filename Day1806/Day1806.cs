using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AdventOfCode.Day1806
{
    public class Day1806Solution : SolutionBase<string[],(int times, string[] coordinates)>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs() => new[]
                                                                        {
                                                                            new[]
                                                                            {
                                                                                "1, 1",
                                                                                "1, 6",
                                                                                "8, 3",
                                                                                "3, 4",
                                                                                "5, 5",
                                                                                "8, 9",
                                                                            }
                                                                        };

        public override IEnumerable<(int times, string[] coordinates)> GetPart2SampleInputs() => new[] {(32, GetPart1SampleInputs().First())};

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"17"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"16"};
        
        public override string[] GetPart1Input() => ReadInputLines();
        public override (int times, string[] coordinates) GetPart2Input() => (10000, ReadInputLines());

        public override object SolvePart1(string[] input)
        {
            var map = new Map(input);
            var infiniteIds = map.InfiniteIds();

            var areaSizes = from value in map.Values
                            group value by value.nearestCoordinateId
                            into g
                            where !infiniteIds.Contains(g.Key)
                            select g.Count();

            return areaSizes.Max();
        }

        public override object SolvePart2((int times, string[] coordinates) input)
        {
            var map = new Map(input.coordinates);

            return (
                from v in map.Values
                where v.totalDistance < input.times
                select true
            ).Count();
        }
    }

    class Map
    {
        internal int ManhattanDistance((int x, int y) a, (int x, int y) b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        public List<((int x, int y) point, int nearestCoordinateId, int totalDistance)> Values { get; set; }
        public int CountX { get; set; }
        public int CountY { get; set; }        


        public Map(string[] input)
        {
            var coordinates = (
                    from i in input
                    let split = i.Split(',')
                    let x = int.Parse(split[0])
                    let y = int.Parse(split[1])
                    select (x, y)
                ).Select((point, id) => (id, point))
                 .ToList();
                                              
            CountX = coordinates.Max(i => i.point.x) + 1;
            CountY = coordinates.Max(i => i.point.y) + 1;

            Values = (
                from x in Enumerable.Range(0, CountX)
                from y in Enumerable.Range(0, CountY)
                let point = (x, y)
                let totalDistance = TotalDistance(point)
                let nearestCoordinateId = NearestCoordinateId(point)
                select (point, nearestCoordinateId, totalDistance)
            ).ToList();

            int NearestCoordinateId((int x, int y) point)
                => (
                        from c in coordinates
                        select (c.id, distance: ManhattanDistance(point, c.point))
                    ).Aggregate((distance: int.MaxValue, id: -1),
                                (prev, next)
                                    => prev.distance == next.distance
                                        ? (prev.distance, -1)
                                        : next.distance < prev.distance
                                            ? (next.distance, next.id)
                                            : prev)
                     .id;

            int TotalDistance((int x, int y) point)
                => (from coordinate in coordinates
                    select ManhattanDistance(point, coordinate.point)).Sum();
        }

        internal IEnumerable<int> InfiniteIds()
        {
            var topRow = from x in Enumerable.Range(0, CountX) select (x, 0);
            var botRow = from x in Enumerable.Range(0, CountX) select (x, CountY - 1);
            var lefRow = from y in Enumerable.Range(0, CountY) select (0, y);
            var rigRow = from y in Enumerable.Range(0, CountY) select (CountX - 1, y);
            var outerPoints = topRow.Concat(botRow).Concat(lefRow).Concat(rigRow);

            var dict = Values.ToDictionary(x => x.point, x => x.nearestCoordinateId);

            return (
                from point in outerPoints
                select dict[point]
            ).Distinct();
        }
    }

    public class Day1806Test : TestBase<Day1806Solution, string[], (int,string[])>
    {
        public Day1806Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
