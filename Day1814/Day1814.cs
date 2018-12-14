using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1814
{
    public class Day1814Solution : SolutionBase<int,string>
    {
        public override IEnumerable<int> GetPart1SampleInputs() => new[] {9, 5, 18, 2018};
        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"5158916779", "0124515891", "9251071085", "5941429882"};

        public override IEnumerable<string> GetPart2SampleInputs() => new[] {"51589", "01245", "92510", "59414"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"9", "5", "18", "2018"};

        public override int GetPart1Input() => 920831;
        public override string GetPart2Input() => GetPart1Input().ToString();


        public override object SolvePart1(int input)
        {
            var recipeBoard = new RecipeBoard();
            
            for (var i = 0; i<input+10; i++)
                recipeBoard.Step();

            return recipeBoard.GetScores(input, 10).ToString("D10");
        }

        public override object SolvePart2(string input)
        {
            var recipeBoard = new RecipeBoard();

            var length = input.Length;
            var inputNumber = int.Parse(input);
            while (true)
            {
                recipeBoard.Step();

                if (recipeBoard.Count - 1 < length)
                    continue;

                if (recipeBoard.GetScores(recipeBoard.Count - length, length) == inputNumber)
                    return recipeBoard.Count - length;
                if (recipeBoard.GetScores(recipeBoard.Count - length - 1, length) == inputNumber)
                    return recipeBoard.Count - length - 1;
            }
        }
    }


    internal class RecipeBoard
    {
        private readonly List<int> _recipies = new List<int> {3, 7};
        private readonly int[] _elfPositions = {0, 1};
        
        public int Count => _recipies.Count;

        private void AddNewReceipts()
        {
            var oldRecepiesSum = _recipies[_elfPositions[0]] + _recipies[_elfPositions[1]];

            if (oldRecepiesSum >= 10)
            {
                _recipies.Add(oldRecepiesSum / 10);
                _recipies.Add(oldRecepiesSum % 10);
            }
            else
                _recipies.Add(oldRecepiesSum);
        }

        public void Step()
        {
            AddNewReceipts();
            MoveElves();
        }

        private void MoveElves()
        {
            for (var i = 0; i < _elfPositions.Length; i++)
            {
                var curPos = _elfPositions[i];
                var increment = _recipies[curPos] + 1;
                _elfPositions[i] = (curPos + increment) % _recipies.Count;
            }
        }

        public long GetScores(int offset, int scoresCount)
        {
            var num = 0L;
            for (int i = 0; i < scoresCount; i++)
            {
                var idx = scoresCount - 1 - i;

                var mul = 1L;
                for (var j = 0; j < i; j++)
                    mul *= 10;

                num += _recipies[idx + offset] * mul;

            }

            return num;
        }
    }

    public class Day1814Test : TestBase<Day1814Solution,int,string>
    {
        public Day1814Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
