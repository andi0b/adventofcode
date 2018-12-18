using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdventOfCode;
using RoyT.AStar;

namespace AdventOfCode.Day1815
{
    public class Day1815Solution : SolutionBase<string[], string[]>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs()
            => new[]
               {
                   new[]
                   {
                       "#######",
                       "#.G...#",
                       "#...EG#",
                       "#.#.#G#",
                       "#..G#E#",
                       "#.....#",
                       "#######",
                   },
                   new[]
                   {
                       "#######",
                       "#G..#E#",
                       "#E#E.E#",
                       "#G.##.#",
                       "#...#E#",
                       "#...E.#",
                       "#######",
                   },
                   new[]
                   {
                       "#######",
                       "#E..EG#",
                       "#.#G.E#",
                       "#E.##E#",
                       "#G..#.#",
                       "#..E#.#",
                       "#######",
                   }
               };

        public override IEnumerable<string> GetPart1SampleOutputs()
            => new[]
               {
                   "Outcome: 47 * 590 = 27730",
                   "Outcome: 37 * 982 = 36334",
                   "Outcome: 46 * 859 = 39514"
               };

        public override IEnumerable<string[]> GetPart2SampleInputs()
            => new[]
               {
                   new[]
                   {
                       "#######",
                       "#.G...#",
                       "#...EG#",
                       "#.#.#G#",
                       "#..G#E#",
                       "#.....#",
                       "#######",
                   },
                   new[]
                   {
                       "#######",
                       "#E..EG#",
                       "#.#G.E#",
                       "#E.##E#",
                       "#G..#.#",
                       "#..E#.#",
                       "#######"
                   },
                   new[]
                   {
                       "#######",
                       "#E.G#.#",
                       "#.#G..#",
                       "#G.#.G#",
                       "#G..#.#",
                       "#...E.#",
                       "#######",
                   }
               };

        public override IEnumerable<string> GetPart2SampleOutputs()
            => new[]
               {
                   "Needed AttackPower: 15, Outcome: 29 * 172 = 4988",
                   "Needed AttackPower: 4, Outcome: 33 * 948 = 31284",
                   "Needed AttackPower: 15, Outcome: 37 * 94 = 3478"
               };

        public override string[] GetPart1Input() => ReadInputLines();


        internal PlayingField PlayingField;

        public override object SolvePart1(string[] input)
        {
            PlayingField = new PlayingField(input);

            var round = 0;
            while (DoRound())
            {
                round++;
            }

            var hitpoints = PlayingField.GetUnits().Sum(x => x.Hitpoints);

            return $"Outcome: {round} * {hitpoints} = {round * hitpoints}";
        }

        public override object SolvePart2(string[] input)
        {
            for (var elfAttackPower = 4;; elfAttackPower++)
            {
                PlayingField = new PlayingField(input);
                var elves = PlayingField.GetUnits('E').ToArray();
                foreach (var elf in elves)
                    elf.AttackPower = elfAttackPower;

                var round = 0;
                while (DoRound())
                {
                    round++;
                    if (elves.Any(x => !x.IsAlive))
                        break;
                }

                var anyDeadElves = elves.Any(x => !x.IsAlive);
                if (!anyDeadElves)
                {
                    var hitpoints = PlayingField.GetUnits().Sum(x => x.Hitpoints);
                    return $"Needed AttackPower: {elfAttackPower}, Outcome: {round} * {hitpoints} = {round * hitpoints}";
                }
            }
        }

        internal bool DoRound()
        {
            var units = PlayingField.GetUnits()
                                     .ToArray();

            foreach (var currentUnit in units)
            {
                if (!currentUnit.IsAlive)
                    continue;

                var possibleTargets = (
                    from unit in PlayingField.GetUnits()
                    where unit.Team != currentUnit.Team &&
                          unit.IsAlive
                    select unit
                ).ToArray();

                if (!possibleTargets.Any())
                    return false;

                Move(currentUnit, possibleTargets);
                Attack(currentUnit);
            }

            return true;
        }

        internal void Move(Unit unit, Unit[] possibleTargets)
        {
            var inRangeLocations = (
                from target in possibleTargets
                from location in target.AdjacentLocations
                let tile = PlayingField[location]
                where tile == null || tile == unit
                select location
            ).Distinct().ToArray();

            if (!inRangeLocations.Contains(unit.Location))
            {
                var grid = CalculateAstarGrid(unit);

                var paths = (
                    from inRangeLocation in inRangeLocations.AsParallel()
                    let readingDirectionIndex = PlayingField.ReadingDirectionIndex(inRangeLocation)
                    let shortestPath = grid.GetPath(GetPosition(unit.Location), GetPosition(inRangeLocation), MovementPatterns.LateralOnly)
                    orderby shortestPath.Length, readingDirectionIndex
                    select new {inRangeLocation, shortestPath, readingDirectionIndex}
                ).ToArray();

                var chosenPath = paths.FirstOrDefault(path => path.shortestPath.Length > 0);

                if (chosenPath != null)
                    PlayingField.MoveUnit(unit, GetLocation(chosenPath.shortestPath[1]));
            }
        }

        private Grid CalculateAstarGrid(Unit unit)
        {
            var grid = new Grid(PlayingField.SizeX, PlayingField.SizeY, 10f);

            var unitAdjacentLocations = unit.AdjacentLocations;
            for (var i = 0; i < 4; i++)
            {
                var position = GetPosition(unitAdjacentLocations[i]);
                grid.SetCellCost(position, 1f + 1f * i);
            }

            foreach (var tile in PlayingField.GetTiles())
                grid.BlockCell(GetPosition(tile.Location));
            
            return grid;
        }

        internal void Attack(Unit unit)
        {
            var adjacentTargets = from adjacentLocation in unit.AdjacentLocations
                                  let adjacentUnit = PlayingField[adjacentLocation] as Unit
                                  where adjacentUnit != null &&
                                        adjacentUnit.Team != unit.Team
                                  orderby adjacentUnit.Hitpoints
                                  select adjacentUnit;

            var chosenTarget = adjacentTargets.FirstOrDefault();
            if (chosenTarget != null)
            {
                chosenTarget.Hitpoints -= unit.AttackPower;

                if (!chosenTarget.IsAlive)
                {
                    PlayingField[chosenTarget.Location] = null;
                }
            }
        }

        private Position GetPosition((int x, int y) location) => new Position(location.x, location.y);
        private (int,int) GetLocation(Position position) => (position.X, position.Y);

    }

    class PlayingField
    {
        private readonly Tile[] _tiles;

        public int SizeY { get; }
        public int SizeX { get; }

        public Tile this[(int x, int y) location]
        {
            get => _tiles[ReadingDirectionIndex(location)];
            set => _tiles[ReadingDirectionIndex(location)] = value;
        }

        public int ReadingDirectionIndex((int x, int y) location) => SizeX * location.y + location.x;

        public PlayingField(string[] lines)
        {
            SizeX = lines[0].Length;
            SizeY = lines.Length;
            _tiles = new Tile[SizeX * SizeY];

            for (var x = 0; x < SizeX; x++)
            for (var y = 0; y < SizeY; y++)
                this[(x, y)] = Tile.Create(lines[y][x], (x, y));
        }

        public IEnumerable<Unit> GetUnits(char? team = null)
        {
            var allUnits = _tiles.OfType<Unit>();
            return team == null ? allUnits : allUnits.Where(u => u.Team == team);
        }

        public IEnumerable<Tile> GetTiles() => _tiles.Where(tile => tile != null);

        public void MoveUnit(Unit unit, (int x, int y) newLocation)
        {
            if (this[newLocation] != null) throw new Exception($"Can't move from {unit.Location} to new location {newLocation}");
            this[unit.Location] = null;
            this[newLocation] = unit;
            unit.Location = newLocation;
        }
    }

    class Tile
    {
        public Tile((int x, int y) location)
        {
            Location = location;
        }

        public (int x, int y) Location { get; set; }

        public static Tile Create(char c, (int x, int y) location)
        {
            switch (c)
            {
                case '#': return new Wall(location);
                case 'E': return new Unit('E', location);
                case 'G': return new Unit('G', location);
                default: return null;
            }
        }

        public (int x, int y)[] AdjacentLocations => new[]
                                                     {
                                                         (Location.x, Location.y - 1),
                                                         (Location.x - 1, Location.y),
                                                         (Location.x + 1, Location.y),
                                                         (Location.x, Location.y + 1),
                                                     };
    }

    class Wall : Tile
    {
        public Wall((int x, int y) location) : base(location)
        {
        }
    }

    class Unit : Tile
    {
        public Unit(char team, (int x, int y) location) : base(location)
        {
            Team = team;
        }

        public int Hitpoints { get; set; } = 200;
        public int AttackPower { get; set; } = 3;
        public char Team { get; }
        public bool IsAlive => Hitpoints > 0;
    }
}