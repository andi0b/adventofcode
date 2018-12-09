using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1809
{
    public class Day1809Solution : SolutionBase<(int playerCount, int highestMarble), (int playerCount, int highestMarble)>
    {
        public override IEnumerable<(int playerCount, int highestMarble)> GetPart1SampleInputs()
            => new[]
               {
                   (7,25),
                   (10, 1618),
                   (13, 7999),
                   (17, 1104),
                   (21, 6111),
                   (30, 5807)
               };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"32", "8317", "146373", "2764", "54718", "37305"};

        public override (int playerCount, int highestMarble) GetPart1Input() => (493, 71863);
        public override (int playerCount, int highestMarble) GetPart2Input() => (GetPart1Input().playerCount, GetPart1Input().highestMarble * 100);

        public override object SolvePart1((int playerCount, int highestMarble) input)
        {
            var scores = new long[input.playerCount];
            var ring = new Ring();

            for (int i = 0; i < input.highestMarble; i++)
            {
                var currentPlayer = i % input.playerCount;
                var marble = i + 1;

                if (marble % 23 == 0)
                {
                    ring.ChangeCurrentMarble(-7);
                    var removedMarble = ring.TakeCurrent();
                    scores[currentPlayer] = scores[currentPlayer] + marble + removedMarble;
                }
                else
                {
                    ring.ChangeCurrentMarble(1);
                    ring.AddAfterCurrent(marble);
                    ring.ChangeCurrentMarble(1);
                }
            }

            return scores.Max();
        }

        public override object SolvePart2((int playerCount, int highestMarble) input) => SolvePart1(input);
    }

    public class Ring
    {
        private readonly LinkedList<int> _data;
        private LinkedListNode<int> _currentNode;

        public Ring()
        {
            _data = new LinkedList<int>(new[] {0});
            _currentNode = _data.First;
        }

        public void ChangeCurrentMarble(int index)
        {
            if (index>0)
                for (var i = 0; i < index; i++)
                    _currentNode = _currentNode.Next ?? _data.First;
            else
                for (var i = 0; i > index; i--)
                    _currentNode = _currentNode.Previous ?? _data.Last;
        }

        public void AddAfterCurrent(int value)
        {
            _data.AddAfter(_currentNode, value);
        }

        public int TakeCurrent()
        {
            var newCurrent = _currentNode.Next ?? _data.First;
            var value = _currentNode.Value;
            _data.Remove(_currentNode);
            _currentNode = newCurrent;
            return value;
        }
    }

    public class Day1809Test : TestBase<Day1809Solution, (int, int), (int, int)>
    {
        public Day1809Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
