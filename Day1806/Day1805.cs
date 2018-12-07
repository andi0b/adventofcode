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

            var grouped = from value in map.Values
                          group value by value.Value
                          into g
                          where !infiniteIds.Contains(g.Key)
                          select (g.Key, count: g.Count());

            return grouped.Max(x => x.count);
        }

        public override object SolvePart2((int times, string[] coordinates) input)
        {
            var map = new Map(input.coordinates);
            var b = map.b();
            var c = b.Where(x => x.Value < input.times);
            return c.Count();
        }
    }

    class Map
    {
        internal int ManhattanDistance((int x, int y) a, (int x, int y) b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        public Map(string[] input)
        {
            var coordinates = from i in input
                              let split = i.Split(',')
                              let x = int.Parse(split[0])
                              let y = int.Parse(split[1])
                              select (x, y);

            Coordinates = coordinates.Select((point, i) => new Coordinate(point, i))
                                     .ToList();

            CountX = Coordinates.Max(i => i.Point.x) + 1;
            CountY = Coordinates.Max(i => i.Point.y) + 1;

            Values = NewMethod();
        }

        public Dictionary<(int x, int y), int> Values { get; set; }

        public Dictionary<(int x, int y), int> NewMethod()
        {
            return (
                from x in Enumerable.Range(0, CountX)
                from y in Enumerable.Range(0, CountY)
                let point = (x, y)
                select (point, value: MapValueForPoint(point))
            ).ToDictionary(x => x.point, x => x.value);


            int MapValueForPoint((int x, int y) point)
            {
                var distances = from c in Coordinates
                                select new {c.Id, Distance = ManhattanDistance(point, c.Point)};

                var minDistance = distances.Min(x => x.Distance);

                try
                {
                    return distances.Single(x => x.Distance == minDistance).Id;
                }
                catch
                {
                    return -1;
                }
            }
        }

        public Dictionary<(int x, int y), int> b()
        {
            return (
                from x in Enumerable.Range(0, CountX)
                from y in Enumerable.Range(0, CountY)
                let point = (x, y)
                let totalDistance = (from coordinate in Coordinates
                                     select ManhattanDistance(point, coordinate.Point)).Sum()
                select (point, totalDistance)
            ).ToDictionary(x => x.point, x => x.totalDistance);
        }

        public int CountX { get; set; }
        public int CountY { get; set; }        
        public List<Coordinate> Coordinates { get; }


        internal IEnumerable<int> InfiniteIds()
        {
            var topRow = from x in Enumerable.Range(0, CountX) select (x, 0);
            var botRow = from x in Enumerable.Range(0, CountX) select (x, CountY - 1);
            var lefRow = from y in Enumerable.Range(0, CountY) select (0, y);
            var rigRow = from y in Enumerable.Range(0, CountY) select (CountX - 1, y);
            var outerPoints = topRow.Concat(botRow).Concat(lefRow).Concat(rigRow);

            return (
                from point in outerPoints
                select Values[point]
            ).Distinct();
        }
    }

    public class Coordinate
    {
        public int Id { get; }
        public (int x, int y) Point { get; }

        public Coordinate((int x, int y) point, int id)
        {
            Point = point;
            Id = id;
        }
    }

    public class Day1806Test : TestBase<Day1806Solution, string[], (int,string[])>
    {
        public Day1806Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
