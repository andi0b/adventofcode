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

            return recipeBoard.GetScores(input, 10);
        }

        public override object SolvePart2(string input)
        {
            var recipeBoard = new RecipeBoard();

            var length = input.Length;
            while (true)
            {
                recipeBoard.Step();

                //if (recipeBoard.GetScores(recipeBoard.Count - length, length) == input)
                //    return recipeBoard.Count - length;
                //if (recipeBoard.GetScores(recipeBoard.Count - length - 1, length) == input)
                //    return recipeBoard.Count - length - 1;

                if (recipeBoard.ScoreEnd(0,length) == input)
                    return recipeBoard.Count - length;
                if (recipeBoard.GetScores(1,length) == input)
                    return recipeBoard.Count - length - 1;
            }
        }
    }

    internal class RecipeBoard
    {
        private readonly LinkedList<int> _recipies;
        private readonly List<Elf> _elves;

        public int Count => _recipies.Count;

        public RecipeBoard()
        {
            _recipies = new LinkedList<int>(new[] {3, 7});
            _elves = _recipies.Nodes().Select(x => new Elf(x)).ToList();
        }

        public void Step()
        {
            foreach (var newRecepie in CalculateNewRecepies())
            {
                _recipies.AddLast(newRecepie);
            }

            MoveElves();
        }

        private IEnumerable<int> CalculateNewRecepies()
        {
            var oldRecepiesSum = _elves.Sum(x => x.Value);
            return oldRecepiesSum.ToString().Select(x => x - '0').ToArray();
        }

        private void MoveElves()
        {
            foreach (var elf in _elves)
                elf.Move();
        }

        public string GetScores(int offset, int scoresCount)
        {
            var slice = _recipies.Skip(offset).Take(scoresCount);
            return string.Join(string.Empty, slice);
        }

        public string ScoreEnd(int offset, int scoresCount)
        {
            var node = _recipies.Last;
            for (var i = 0; i < offset; i++)
                node = node.Previous ?? node.List.Last;

            var scores = new int[scoresCount];
            for (var i = 0; i < scoresCount; i++)
            {
                scores[scoresCount - i - 1] = node.Value;
                node = node.Previous ?? node.List.Last;
            }

            return string.Join(string.Empty, scores);
        }
    }

    internal class Elf
    {
        public Elf(LinkedListNode<int> currentRecipe)
        {
            CurrentRecipe = currentRecipe;
        }

        public LinkedListNode<int> CurrentRecipe { get; set; }

        public int Value => CurrentRecipe.Value;

        public void Move()
        {
            var moveCount = Value + 1;
            for (var i = 0; i < moveCount; i++)
                CurrentRecipe = CurrentRecipe.Next ?? CurrentRecipe.List.First;
        }
    }

    internal static class LinkedListExtensions
    {
        public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> list)
        {
            var node = list.First;
            do
            {
                yield return node;
            } while ((node = node.Next) != null);
        }
    }

    public class Day1814Test : TestBase<Day1814Solution,int,string>
    {
        public Day1814Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
