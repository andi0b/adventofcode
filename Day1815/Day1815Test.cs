using Xunit;
using Xunit.Abstractions;

namespace AdventOfCode.Day1815
{
    public class Day1815Test : TestBase<Day1815Solution, string[], string[]>
    {
        public Day1815Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void MovementTest()
        {
            var initial = new[]
                          {
                              "#########",
                              "#G..G..G#",
                              "#.......#",
                              "#.......#",
                              "#G..E..G#",
                              "#.......#",
                              "#.......#",
                              "#G..G..G#",
                              "#########",
                          };

            var solution = new Day1815Solution {PlayingField = new PlayingField(initial)};
            AssertPosition('G', 1, 1);
            AssertPosition('G', 4, 1);
            AssertPosition('G', 7, 1);
            AssertPosition('G', 1, 4);
            AssertPosition('E', 4, 4);
            AssertPosition('G', 7, 4);
            AssertPosition('G', 1, 7);
            AssertPosition('G', 4, 7);
            AssertPosition('G', 7, 7);

            //After 1 round:
            //#########
            //#.G...G.#
            //#...G...#
            //#...E..G#
            //#.G.....#
            //#.......#
            //#G..G..G#
            //#.......#
            //#########
            solution.DoRound();
            AssertPosition('G', 2, 1);
            AssertPosition('G', 4, 2);
            AssertPosition('G', 6, 1);
            AssertPosition('G', 2, 4);
            AssertPosition('E', 4, 3);
            AssertPosition('G', 7, 3);
            AssertPosition('G', 1, 6);
            AssertPosition('G', 4, 6);
            AssertPosition('G', 7, 6);

            //After 2 rounds:
            //#########
            //#..G.G..#
            //#...G...#
            //#.G.E.G.#
            //#.......#
            //#G..G..G#
            //#.......#
            //#.......#
            //#########
            solution.DoRound();
            AssertPosition('G', 3, 1);
            AssertPosition('G', 4, 2);
            AssertPosition('G', 5, 1);
            AssertPosition('G', 2, 3);
            AssertPosition('E', 4, 3);
            AssertPosition('G', 6, 3);
            AssertPosition('G', 1, 5);
            AssertPosition('G', 4, 5);
            AssertPosition('G', 7, 5);

            //After 3 rounds:
            //#########
            //#.......#
            //#..GGG..#
            //#..GEG..#
            //#G..G...#
            //#......G#
            //#.......#
            //#.......#
            //#########
            solution.DoRound();
            AssertPosition('G', 3, 2);
            AssertPosition('G', 4, 2);
            AssertPosition('G', 5, 2);
            AssertPosition('G', 3, 3);
            AssertPosition('E', 4, 3);
            AssertPosition('G', 5, 3);
            AssertPosition('G', 1, 4);
            AssertPosition('G', 4, 4);
            AssertPosition('G', 7, 5);

            void AssertPosition(char expectedTeam, int x, int y)
            {
                var actual = solution.PlayingField[(x, y)];
                Assert.IsType<Unit>(actual);
                Assert.Equal(expectedTeam, ((Unit) actual).Team);
            }
        }

    }
}