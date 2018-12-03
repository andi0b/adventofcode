using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1803
{
    public class Day1803Solution : SolutionBase<string[], string[]>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs() => new[]
                                                                        {
                                                                            new[]
                                                                            {
                                                                                "#1 @ 1,3: 4x4",
                                                                                "#2 @ 3,1: 4x4",
                                                                                "#3 @ 5,5: 2x2"
                                                                            }
                                                                        };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"4"};

        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"3"};

        public override string[] GetPart1Input() => ReadInputLines();



        public override object SolvePart1(string[] input)
        {
            var (fabric, _) = MarkFabric(input);

            var overlappingSquareInches = fabric.Values.Count(x => x.Count > 1);

            return overlappingSquareInches;
        }

        public override object SolvePart2(string[] input)
        {
            var (fabric, cutInstructions) = MarkFabric(input);

            var single = (
                from c in cutInstructions
                where c.GetAllPoints().All(p => fabric[p].Count == 1)
                select c
            ).Single();

            return single.Id;
        }

        private (Dictionary<(int, int), List<int>>, FabricCutInstruction[]) MarkFabric(string[] input)
        {
            var fabric = new Dictionary<(int, int), List<int>>();
            var cutInstructions = input.Select(FabricCutInstruction.Parse).ToArray();

            void ApplyCutInstruction(FabricCutInstruction i)
            {
                foreach (var point in i.GetAllPoints())
                {
                    if (!fabric.TryGetValue(point, out var existing))
                    {
                        existing = new List<int>();
                        fabric.Add(point, existing);
                    }

                    existing.Add(i.Id);
                }
            }

            foreach (var instruction in cutInstructions)
                ApplyCutInstruction(instruction);

            return (fabric, cutInstructions);
        }
    }


    internal class FabricCutInstruction
    {
        public int Id { get; private set; }
        public (int x, int y) Size { get; private set; }
        public (int x, int y) Position { get; private set; }

        public IEnumerable<(int x, int y)> GetAllPoints()
        {
            for (var x = Position.x; x < Size.x + Position.x; x++)
            for (var y = Position.y; y < Size.y + Position.y; y++)
                yield return (x, y);
        }

        public static FabricCutInstruction Parse(string s)
        {
            var parts = s.Split(' ');
            return new FabricCutInstruction
                   {
                       Id = int.Parse(parts[0].TrimStart('#')),
                       Position = ParseIntTouple(parts[2].TrimEnd(':'), ','),
                       Size = ParseIntTouple(parts[3], 'x'),
                   };

            (int,int) ParseIntTouple(string i, char seperator)
            {
                var p = i.Split(seperator);
                return (int.Parse(p[0]), int.Parse(p[1]));
            }
        }
    }

    public class Day1803Test : TestBase<Day1803Solution, string[],string[]>
    {
        public Day1803Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
