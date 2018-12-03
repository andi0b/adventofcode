using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1803
{
    public class Day1803Solution : SolutionBase<string[],string[]>
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


        private readonly Dictionary<(int, int), List<int>> _fabric = new Dictionary<(int, int), List<int>>();

        public override object SolvePart1(string[] input)
        {
            var cutInstructions = input.Select(FabricCutInstruction.Parse);
            foreach (var instruction in cutInstructions)
                ApplyCutInstruction(instruction);

            var overlappingSquareInches = _fabric.Values.Count(x => x.Count > 1);
            
            return overlappingSquareInches;
        }

        public override object SolvePart2(string[] input)
        {
            var cutInstructions = input.Select(FabricCutInstruction.Parse).ToArray();
            foreach (var instruction in cutInstructions)
                ApplyCutInstruction(instruction);

            var single = (
                from c in cutInstructions
                where (from fv in _fabric.Values
                       where fv.Contains(c.Id)
                       select fv).All(fv => fv.Count == 1)
                select c
            ).Single();

            return single.Id;
        }

        private void ApplyCutInstruction(FabricCutInstruction i)
        {
            foreach (var point in i.GetAllPoints())
            {
                if (!_fabric.TryGetValue(point, out var existing))
                {
                    existing = new List<int>();
                    _fabric.Add(point,existing);
                }

                existing.Add(i.Id);
            } 
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
