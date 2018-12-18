using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1818
{
    public class Day1818Solution : SolutionBase<string[], string[]>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs()
            => new[]
               {
                   new[]
                   {
                       ".#.#...|#.",
                       ".....#|##|",
                       ".|..|...#.",
                       "..|#.....#",
                       "#.#|||#|#|",
                       "...#.||...",
                       ".|....|...",
                       "||...#|.#|",
                       "|.||||..|.",
                       "...#.|..|."
                   }
               };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"1147"};

        public override string[] GetPart1Input() => ReadInputLines();

        public override object SolvePart1(string[] input)
        {
            var forrest = new Forrest(input);
            for (var i = 1; i <= 10; i++)
            {
                forrest = forrest.NextGeneration();
            }

            return forrest.TreeCount * forrest.LumberyardCount;
        }

        public override object SolvePart2(string[] input)
        {
            var previousForrests = new List<Forrest>();
            var forrest = new Forrest(input);
            previousForrests.Add(forrest);

            var iterations = 1_000_000_000;
            for (var i = 1; i <= iterations; i++)
            {
                forrest = forrest.NextGeneration();

                var sameExistingForrestIndex = previousForrests.IndexOf(forrest);
                if (sameExistingForrestIndex != -1)
                {
                    var repeatCount = i - sameExistingForrestIndex;
                    var remainingIterationsFromFirstRepeat = iterations - sameExistingForrestIndex;

                    var resultForrestIndex = sameExistingForrestIndex + (remainingIterationsFromFirstRepeat % repeatCount);
                    var resultForrest = previousForrests[resultForrestIndex];


                    return resultForrest.TreeCount * resultForrest.LumberyardCount;
                }

                previousForrests.Add(forrest);
            }

            throw new Exception("End of the World!");
        }
    }

    class Forrest
    {
        protected bool Equals(Forrest other)
        {
            if (!(SizeX == other.SizeX && SizeY == other.SizeY))
                return false;

            for (var i = 0; i < SizeX * SizeY; i++)
            {
                if (_tiles[i] != other._tiles[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Forrest) obj);
        }

        public const char Lumberyard = '#';
        public const char Tree = '|';
        public const char Open = '.';
        
        private char[] _tiles;

        public int SizeX { get; }
        public int SizeY { get; }

        public char this[(int x, int y) location]
        {
            get => _tiles[Index(location)];
            set => _tiles[Index(location)] = value;
        }

        public int Index((int x, int y) location) => SizeX * location.y + location.x;

        public int LumberyardCount => _tiles.Count(x => x == Lumberyard);
        public int TreeCount => _tiles.Count(x => x == Tree);


        public Forrest(string[] lines)
        {
            SizeX = lines[0].Length;
            SizeY = lines.Length;
            _tiles = new char[SizeX * SizeY];

            for (var x = 0; x < SizeX; x++)
            for (var y = 0; y < SizeY; y++)
                this[(x, y)] = lines[y][x];
        }

        public Forrest( int sizeX, int sizeY, IEnumerable<char> tiles = null)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _tiles = tiles as char[] ?? tiles?.ToArray() ?? new char[SizeX * SizeY];;
        }

        public IEnumerable<char> AdjacentTiles((int x, int y) location)
        {
            var minX = Math.Max(0, location.x-1);
            var maxX = Math.Min(SizeX - 1, location.x+1);
            var minY = Math.Max(0, location.y-1);
            var maxY = Math.Min(SizeY - 1, location.y+1);

            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
            {
                if (location != (x, y))
                {
                    yield return this[(x, y)];
                }
            }
        }

        public Forrest NextGeneration()
        {
            var newForrest = new Forrest(SizeX, SizeY);

            for (var x = 0; x < SizeX; x++)
            for (var y = 0; y < SizeY; y++)
            {
                var loc = (x, y);
                var adjacentTiles = AdjacentTiles(loc);

                void Set(char c) => newForrest[loc] = c;

                switch (this[(x, y)])
                {
                    case Open:
                        Set(adjacentTiles.Count(t => t == Tree) >= 3 ? Tree : Open);
                        break;

                    case Tree:
                        Set(adjacentTiles.Count(t => t == Lumberyard) >= 3 ? Lumberyard : Tree);
                        break;

                    case Lumberyard:
                        Set(adjacentTiles.Count(t => t == Lumberyard) >= 1 && 
                            adjacentTiles.Count(t => t == Tree) >= 1
                                ? Lumberyard
                                : Open);
                        break;
                }
            }

            return newForrest;
        }

        
    }

    public class Day1818Test : TestBase<Day1818Solution, string[], string[]>
    {
        public Day1818Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
