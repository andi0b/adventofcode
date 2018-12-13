using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1811
{
    public class Day1811Solution : SolutionBase<int,int>
    {
        public override IEnumerable<int> GetPart1SampleInputs() => new[] {42};
        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"21,61"};
        public override IEnumerable<int> GetPart2SampleInputs() => new[] {18, 42};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"90,269,16(113)", "232,251,12(119)"};

        public override int GetPart1Input() => 5093;

        public override object SolvePart1(int input)
        {
            var fuelGrid = new FuelGrid(input);

            var powerLevelSquares =
                from x in Enumerable.Range(1, 298)
                from y in Enumerable.Range(1, 298)
                let powerLevel = (
                    from xOff in Enumerable.Range(0, 3)
                    from yOff in Enumerable.Range(0, 3)
                    let xPos = x + xOff
                    let yPos = y + yOff
                    select fuelGrid[xPos, yPos]
                ).Sum()
                select (x, y, powerLevel);

            var max = powerLevelSquares.Aggregate((x: 0, y: 0, powerLevel: int.MinValue),
                                                  (prev, next) => (next.powerLevel > prev.powerLevel) ? next : prev);

            return $"{max.x},{max.y}";
        }

        public override object SolvePart2(int input)
        {
            var fuelGridArray = new FuelGrid(input).ToArray(301);

            (int x, int y, int size) max = default;
            var maxValue = int.MinValue;

            for (var x = 1; x <= 300;x++)
            for (var y = 1; y <= 300; y++)
            {
                var maxBoxSize = 300 - Math.Max(x, y);
                for(var size = 0; size<maxBoxSize; size++)
                {
                    var powerLevel = 0;
                    for (var xOff = 0; xOff <= size; xOff++)
                    for (var yOff = 0; yOff <= size; yOff++)
                    {
                        var xPos = x + xOff;
                        var yPos = y + yOff;
                        powerLevel += fuelGridArray[xPos][yPos];
                    }

                    if (powerLevel > maxValue)
                    {
                        max = (x, y, size + 1);
                        maxValue = powerLevel;
                    }
                }
            }

            return $"{max.x},{max.y},{max.size}({maxValue})";
        }
    }

    class FuelGrid
    {
        public int SerialNumber { get; }

        public FuelGrid(int serialNumber)
        {
            SerialNumber = serialNumber;
        }

        public int this[int x, int y]
        {
            get
            {
                var rackId = x + 10;
                var initialPowerLevel = rackId * y;
                var powerLevel = (initialPowerLevel + SerialNumber) * rackId;
                var hundredsDigit = powerLevel / 100 % 10;
                return hundredsDigit - 5;
            }
        }

        public int[][] ToArray(int size)
        {
            var array = new int[size][];
            for (int x = 0; x < size; x++)
            {
                array[x] = new int[size];
                for (int y = 0; y < size; y++)
                {
                    array[x][y] = this[x, y];
                }
            }

            return array;
        }
    }

    public class Day1811Test:TestBase<Day1811Solution, int, int>
    {
        public Day1811Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(3, 5, 8, 4)]
        [InlineData(122, 79, 57, -5)]
        [InlineData(217, 196, 39, 0)]
        [InlineData(101, 153, 71, 4)]
        public void Test(int x, int y, int serialNumber, int expectedPowerLevel)
        {
            var calculatedPowerLevel = new FuelGrid(serialNumber)[x, y];
            Assert.Equal(expectedPowerLevel, calculatedPowerLevel);
        }
    }
}
