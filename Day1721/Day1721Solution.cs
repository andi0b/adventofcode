using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1721
{
    public class Day1721Solution : SolutionBase<(int, string[]),(int, string[])>
    {
        public override IEnumerable<(int, string[])> GetPart1SampleInputs() => new List<(int, string[])>
                                                                        {
                                                                            (2, new[]
                                                                                {
                                                                                    "../.# => ##./#../...",
                                                                                    ".#./..#/### => #..#/..../..../#..#"
                                                                                })
                                                                        };

        public override IEnumerable<string> GetPart1SampleOutputs() => new List<string>
                                                                       {
                                                                           "12"
                                                                       };

        public override (int, string[]) GetPart1Input() =>(5, ReadInputLines());

        public override (int, string[]) GetPart2Input() =>(18, ReadInputLines());

        public override object SolvePart1((int, string[]) input)
        {
            var (iterations,rulesText) = input;
            var rules = rulesText.Select(Rule.Parse);

            var all = rules.First().GetInputAllDirections();

            var rulesDict = rules.SelectMany(x => x.GetInputAllDirections().Distinct(), (x, y) => new {Input = y, Output = x.Output})
                                 .ToDictionary(x => x.Input, x => x.Output);

            var art = BitMap.Parse(".#./..#/###");

            for (int i = 0; i < iterations; i++)
            {
                var slices = art.BreakIntoSlices();
                var replacedSlices = slices.Select(x => rulesDict[x]).ToArray();
                art = new BitMap(replacedSlices);
            }

            return art.GetSetCount().ToString();
        }

        public override object SolvePart2((int, string[]) input) => SolvePart1(input);
    }

    public class Rule
    {
        public BitMap Output { get; }
        public BitMap Input { get; }

        public IEnumerable<BitMap> GetInputAllDirections()
        {
            yield return Input;
            yield return Input.Turn();
            yield return Input.Turn().Turn();
            yield return Input.Turn().Turn().Turn();
            yield return Input.Flip();
            yield return Input.Flip().Turn();
            yield return Input.Flip().Turn().Turn();
            yield return Input.Flip().Turn().Turn().Turn();
        }

        public Rule(BitMap input, BitMap output)
        {
            Input = input;
            Output = output;
        }

        public static Rule Parse(string text)
        {
            var parts = text.Split(" => ");
            return new Rule(BitMap.Parse(parts[0]),
                            BitMap.Parse(parts[1]));
        }
    }

    public class BitMap
    {        
        private readonly bool[][] _rows;

        public int Dimension => _rows.Length;

        public BitMap(bool[][] rows)
        {
            if (rows.Any(x => x.Length != rows.Length))
                throw new Exception();

            _rows = rows;
        }

        public BitMap(BitMap[] bitMaps)
        {
            var partCountSqrt = (int) Math.Sqrt(bitMaps.Length);
            var partSize = bitMaps[0].Dimension;
            var newDimension = partSize * partCountSqrt;
            _rows = Enumerable.Range(0, newDimension).Select(x => new bool[newDimension]).ToArray();

            void CopyRow(BitMap bitMap, int offsetX, int offsetY)
            {
                for (var x = 0; x < bitMap.Dimension; x++)
                for (var y = 0; y < bitMap.Dimension; y++)
                {
                    _rows[x + offsetX][y + offsetY] = bitMap._rows[x][y];
                }
            }

            for (var x = 0; x < partCountSqrt; x++)
            for (var y = 0; y < partCountSqrt; y++)
            {
                var index = x * partCountSqrt + y;
                CopyRow(bitMaps[index], x * partSize, y * partSize);
            }
        }

        public IEnumerable<BitMap> BreakIntoSlices()
        {
            var partSize = Dimension % 2 == 0 ? 2 : 3;
            var partCountSqrt = Dimension / partSize;
            
            for (var x = 0; x<partCountSqrt;x++)
            for (var y = 0; y < partCountSqrt; y++)
            {
                var a = new ArraySegment<bool[]>(_rows, x * partSize, partSize).ToArray();
                var newRows = a.Select(row => new ArraySegment<bool>(row, y * partSize, partSize).ToArray()).ToArray();
                yield return new BitMap(newRows);
            }
        }

        public BitMap Flip()
        {
            bool[] Flip(bool[] row) => row.Reverse().ToArray();
            var flippedRows = _rows.Select(Flip);
            return new BitMap(flippedRows.ToArray());
        }

        public BitMap Turn()
        {
            var turnedRows = Enumerable.Range(0, Dimension).Select(x => new bool[Dimension]).ToArray();
            
            for (var x = 0; x<Dimension;x++)
            for (var y = 0; y < Dimension; y++)
            {
                turnedRows[x][Dimension - 1 - y] = _rows[y][x];
            }

            return new BitMap(turnedRows);
        }

        public static BitMap Parse(string text)
        {
            var rows = text.Split("/");
            return new BitMap(rows.Select(x => MapRow(x.ToCharArray())).ToArray());

            bool[] MapRow(char[] chars) => chars.Select(MapChar).ToArray();
            bool MapChar(char c) => c == '#';
        }

        public override string ToString()
        {
            var strRows = _rows.Select(row => new string(row.Cast<bool>().Select(chr => chr ? '#' : '.').ToArray()));
            return string.Join('/', strRows);
        }

        public int GetSetCount() => _rows.SelectMany(x => x).Count(x => x);

        public bool[] GetAsOne() => _rows.SelectMany(x => x).ToArray();

        protected bool Equals(BitMap other) => GetAsOne().SequenceEqual(other.GetAsOne());

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BitMap) obj);
        }

        public override int GetHashCode()
        {
            var firstBits = GetAsOne().Take(32).Select((x, i) => (x, i));

            var hashCode = 0;
            foreach (var (x,i) in firstBits)
            {
                if (!x) continue;

                hashCode |= 1 << i;
            }

            return hashCode;
        }
    }

    public class Day1721Test : TestBase<Day1721Solution, (int, string[]),(int, string[])>
    {
        public Day1721Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Turn()
        {
            var input = BitMap.Parse(".#/..");
            var turned = input.Turn();
            Assert.Equal("../.#", turned.ToString());
        }

        [Fact]
        public void BreakCombine()
        {
            var input = BitMap.Parse("#..#/..../..../#..#");
            var slices = input.BreakIntoSlices().ToArray();
            var output = new BitMap(slices);
            Assert.Equal(input.ToString(),output.ToString());
        }
    }
}
