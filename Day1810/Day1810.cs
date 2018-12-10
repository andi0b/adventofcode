using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Xunit.Abstractions;

namespace AdventOfCode.Day1810
{
    public class Day1810Solution : SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[]
                                                                      {
                                                                          string.Join(Environment.NewLine, 
                                                                                      "position=< 9,  1> velocity=< 0,  2>",
                                                                                      "position=< 7,  0> velocity=<-1,  0>",
                                                                                      "position=< 3, -2> velocity=<-1,  1>",
                                                                                      "position=< 6, 10> velocity=<-2, -1>",
                                                                                      "position=< 2, -4> velocity=< 2,  2>",
                                                                                      "position=<-6, 10> velocity=< 2, -2>",
                                                                                      "position=< 1,  8> velocity=< 1, -1>",
                                                                                      "position=< 1,  7> velocity=< 1,  0>",
                                                                                      "position=<-3, 11> velocity=< 1, -2>",
                                                                                      "position=< 7,  6> velocity=<-1, -1>",
                                                                                      "position=<-2,  3> velocity=< 1,  0>",
                                                                                      "position=<-4,  3> velocity=< 2,  0>",
                                                                                      "position=<10, -3> velocity=<-1,  1>",
                                                                                      "position=< 5, 11> velocity=< 1, -2>",
                                                                                      "position=< 4,  7> velocity=< 0, -1>",
                                                                                      "position=< 8, -2> velocity=< 0,  1>",
                                                                                      "position=<15,  0> velocity=<-2,  0>",
                                                                                      "position=< 1,  6> velocity=< 1,  0>",
                                                                                      "position=< 8,  9> velocity=< 0, -1>",
                                                                                      "position=< 3,  3> velocity=<-1,  1>",
                                                                                      "position=< 0,  5> velocity=< 0, -1>",
                                                                                      "position=<-2,  2> velocity=< 2,  0>",
                                                                                      "position=< 5, -2> velocity=< 1,  2>",
                                                                                      "position=< 1,  4> velocity=< 2,  1>",
                                                                                      "position=<-2,  7> velocity=< 2, -2>",
                                                                                      "position=< 3,  6> velocity=<-1, -1>",
                                                                                      "position=< 5,  0> velocity=< 1,  0>",
                                                                                      "position=<-6,  0> velocity=< 2,  0>",
                                                                                      "position=< 5,  9> velocity=< 1, -2>",
                                                                                      "position=<14,  7> velocity=<-2,  0>",
                                                                                      "position=<-3,  6> velocity=< 2, -1>"
                                                                          )
                                                                      };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[]
                                                                       {
                                                                           string.Join(Environment.NewLine,
                                                                                       "#...#..###",
                                                                                       "#...#...#.",
                                                                                       "#...#...#.",
                                                                                       "#####...#.",
                                                                                       "#...#...#.",
                                                                                       "#...#...#.",
                                                                                       "#...#...#.",
                                                                                       "#...#..###"
                                                                           )
                                                                       };

        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"3"};

        public override string GetPart1Input() => ReadInput();

        public override object SolvePart1(string input)
        {
            var (sky, _) = CalculateSky(input);
            var skyString = sky.ToString();
            File.WriteAllText("Day1810_Part1.txt", skyString);
            return skyString;
        }

        public override object SolvePart2(string input) => CalculateSky(input).steps;

        private (Sky sky, int steps) CalculateSky(string input)
        {
            var sky = new Sky(input);

            int i = 0;
            for (; sky.CountRowsOfMoreThan(6) < 2; i++)
                sky.CalculateNextStep();

            return (sky, i);
        }
    }

    class Sky
    {
        private Matrix<float> _positionMatrix;
        private Matrix<float> _velocityMatrix;

        public Sky(string input)
        {
            var lights = (
                from match in Regex.Matches(input, @"position=<\s*(?<posX>-?\d+),\s*(?<posY>-?\d+)> velocity=<\s*(?<velX>-?\d+),\s*(?<velY>-?\d+)>")
                let position = new[] {float.Parse(match.Groups["posX"].Value), float.Parse(match.Groups["posY"].Value)}
                let velocity = new[] {float.Parse(match.Groups["velX"].Value), float.Parse(match.Groups["velY"].Value)}
                select (position, velocity)
            ).ToArray();

            var M = Matrix<float>.Build;
            _positionMatrix = M.DenseOfRowArrays(lights.Select(l => l.position));
            _velocityMatrix = M.DenseOfRowArrays(lights.Select(l => l.velocity));
        }

        public void CalculateNextStep()
        {
            _positionMatrix = _positionMatrix.Add(_velocityMatrix);
        }

        public (int minX, int minY, int maxX, int maxY) BoundingBox()
        {
            var min = _positionMatrix.ReduceRows((prev, next) => prev.PointwiseMinimum(next));
            var max = _positionMatrix.ReduceRows((prev, next) => prev.PointwiseMaximum(next));

            return (
                minX: (int)min[0],
                minY: (int)min[1],
                maxX: (int)max[0],
                maxY: (int)max[1]
            );
        }

        public override string ToString()
        {
            var boundingBox = BoundingBox();
            var width = boundingBox.maxX - boundingBox.minX+1;
            var height = boundingBox.maxY - boundingBox.minY+1;
            var lines = Enumerable.Range(0, height).Select(x => Enumerable.Repeat('.', width).ToArray()).ToArray();

            foreach (var point in _positionMatrix.EnumerateRows())
            {
                var x = (int) point[0] - boundingBox.minX;
                var y = (int) point[1] - boundingBox.minY;
                lines[y][x] = '#';
            }

            var strings = lines.Select(line => new string(line));
            return string.Join(Environment.NewLine, strings);
        }

        public int CountRowsOfMoreThan(int lightCount)
        {
            var columns = from row in _positionMatrix.EnumerateRows()
                          let coordinate = (x: (int) row[0], y: (int) row[1])
                          group coordinate by coordinate.x
                          into cols
                          select cols.Select(x => x.y).Distinct().OrderBy(x => x);

            var consecutiveGroups = from col in columns
                                    select CountConsecutiveGroups(col.OrderBy(x => x));

            var filtered = from g in consecutiveGroups
                           where g.Any(x => x > lightCount)
                           select g;

            return filtered.Count();

        }

        public static IEnumerable<int> CountConsecutiveGroups(IEnumerable<int> numbers)
        {
            var previous = int.MinValue;
            var consecutiveCount = 1;
            foreach (var current in numbers)
            {
                if (previous + 1 == current)
                {
                    consecutiveCount++;
                }
                else
                {
                    yield return consecutiveCount;
                    consecutiveCount = 1;
                }

                previous = current;
            }

            yield return consecutiveCount;
        }
    }

    public class Day1810Test : TestBase<Day1810Solution, string, string>
    {
        public Day1810Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
