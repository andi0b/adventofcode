using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1813
{
    public class Day1813Solution : SolutionBase<string[], string[]>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs()
            => new[]
               {
                   new[]
                   {
                       @"/->-\        ",
                       @"|   |  /----\",
                       @"| /-+--+-\  |",
                       @"| | |  | v  |",
                       @"\-+-/  \-+--/",
                       @"  \------/   "
                   },
                   new[]
                   {
                       @"|",
                       @"v",
                       @"|",
                       @"|",
                       @"|",
                       @"^",
                       @"|",
                   },
                   new[]
                   {
                       @"|",
                       @"v",
                       @"|",
                       @"|",
                       @"^",
                       @"|",
                   }
               };

        public override IEnumerable<string[]> GetPart2SampleInputs()
            => new[]
               {
                   new[]
                   {
                       @"/>-<\  ",
                       @"|   |  ",
                       @"| /<+-\",
                       @"| | | v",
                       @"\>+</ |",
                       @"  |   ^",
                       @"  \<->/",
                   }
               };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"7,3", "0,3", "0,3"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"6,4"};
        public override string[] GetPart1Input() => ReadInputLines();

        public override object SolvePart1(string[] input)
        {
            var track = new Track(input);

            while (true)
            {
                foreach (var c in track.CartsOrdered)
                {
                    track.MoveCart(c);

                    var collissions = (
                        from cart in track.Carts
                        group cart by cart.Location
                        into g
                        where g.Count() > 1
                        select g.Key
                    ).ToArray();

                    if (collissions.Any())
                        return collissions[0].x + "," + collissions[0].y;
                }
            }
        }


        public override object SolvePart2(string[] input)
        {
            var track = new Track(input);

            while (true)
            {
                foreach (var c in track.CartsOrdered)
                {
                    track.MoveCart(c);

                    var collidingCarts =
                        from cart in track.Carts
                        group cart by cart.Location
                        into g
                        where g.Count() > 1
                        from cart in g
                        select cart;

                    foreach (var coll in collidingCarts)
                    {
                        track.Carts.Remove(coll);
                    }
                }

                if (track.Carts.Count == 1)
                {
                    var singleCart = track.Carts.Single();
                    return singleCart.X + "," + singleCart.Y;
                }
            }
        }
    }


    class Track
    {
        public char[,] Tiles { get; }
        public int SizeY { get; set; }
        public int SizeX { get; set; }
        public List<Cart> Carts { get; }

        public IEnumerable<Cart> CartsOrdered => from cart in Carts
                                                 orderby cart.Y, cart.X
                                                 select cart;

        public Track(string[] input)
        {
            SizeX = input[0].Length;
            SizeY = input.Length;
            Tiles = new char[SizeX, SizeY];

            for (var x = 0; x < SizeX; x++)
            for (var y = 0; y < SizeY; y++)
                Tiles[x, y] = input[y][x];

            Carts = (
                from x in Enumerable.Range(0, SizeX)
                from y in Enumerable.Range(0, SizeY)
                let tileValue = Tiles[x, y]
                where Directions.AllDirections.Contains(tileValue)
                select new Cart(x, y, Tiles[x, y])
            ).ToList();

            foreach (var cart in Carts)
            {
                var tileBelowCart = cart.Direction == '<' || cart.Direction == '>' ? '-' : '|';
                Tiles[cart.X, cart.Y] = tileBelowCart;
            }
        }

        public void MoveCart(Cart c)
        {
            var next = c.NextLocation;

            switch (Tiles[next.x, next.y])
            {
                case '/':
                    switch (c.Direction)
                    {
                        case '^':
                            c.Direction = '>';
                            break;
                        case '>':
                            c.Direction = '^';
                            break;
                        case 'v':
                            c.Direction = '<';
                            break;
                        case '<':
                            c.Direction = 'v';
                            break;
                        default: 
                            throw new InvalidOperationException();
                    }

                    break;

                case '\\':
                    switch (c.Direction)
                    {
                        case '^':
                            c.Direction = '<';
                            break;
                        case '>':
                            c.Direction = 'v';
                            break;
                        case 'v':
                            c.Direction = '>';
                            break;
                        case '<':
                            c.Direction = '^';
                            break;
                        default: 
                            throw new InvalidOperationException();
                    }

                    break;

                case '+':
                    var turnDirection = c.NextCrossingDescicion();
                    if (turnDirection == 'L') 
                    {
                        c.Direction = Directions.NextTurnDirection(c.Direction, -1);
                    }
                    else if (turnDirection == 'R')
                    {
                        c.Direction = Directions.NextTurnDirection(c.Direction, +1);
                    }
                    break;

                case '|':
                case '-':
                    break;

                default: 
                    throw new InvalidOperationException();


            }

            c.X = next.x;
            c.Y = next.y;
        }
    }

    public class Directions
    {
        public static readonly char[] AllDirections = {'^', '>', 'v', '<'};

        public static char NextTurnDirection(char direction, int turnClockwiseCount)
        {
            var directionIdx = Array.IndexOf(AllDirections, direction);
            var newIndex = (100 * AllDirections.Length + directionIdx + turnClockwiseCount) % AllDirections.Length;
            return AllDirections[newIndex];
        }
    }

    internal class Cart
    {
        public int X { get; set;  }
        public int Y { get; set; }

        public (int x, int y) Location => (X, Y);

        public char Direction { get; set; }

        private char _crossingDescicion = 'R';

        public Cart(int x, int y, char direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }

        public (int x, int y) NextLocation
        {
            get
            {
                switch (Direction)
                {
                    case '>': return (X + 1, Y);
                    case '<': return (X - 1, Y);
                    case '^': return (X, Y - 1);
                    case 'v': return (X, Y + 1);
                    default: throw new InvalidOperationException();
                }
            }
        }

        public char NextCrossingDescicion()
        {
            switch (_crossingDescicion)
            {
                case 'L':
                    _crossingDescicion = 'S';
                    break;
                case 'S':
                    _crossingDescicion = 'R';
                    break;
                case 'R':
                    _crossingDescicion = 'L';
                    break;
                default: throw new InvalidOperationException();
            }

            return _crossingDescicion;
        }
    }

    public class Day1813Test : TestBase<Day1813Solution, string[], string[]>
    {
        public Day1813Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
