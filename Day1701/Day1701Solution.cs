using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode.Day1701
{
    public class Day1701Solution : SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[]
                                                                      {
                                                                          "1122",
                                                                          "1111",
                                                                          "1234",
                                                                          "91212129",
                                                                      };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[]
                                                                       {
                                                                           "3",
                                                                           "4",
                                                                           "0",
                                                                           "9"
                                                                       };

        public override IEnumerable<string> GetPart2SampleInputs() => new[]
                                                                      {
                                                                          "1212",
                                                                          "1221",
                                                                          "123425",
                                                                          "123123",
                                                                          "12131415"
                                                                      };

        public override IEnumerable<string> GetPart2SampleOutputs() => new[]
                                                                       {
                                                                           "6",
                                                                           "0",
                                                                           "4",
                                                                           "12",
                                                                           "4"
                                                                       };



        public override string GetPart1Input() => File.ReadAllText("InputFiles\\Day1701.txt");

        public override string SolvePart1(string input)
        {
            var chars =  input.ToCharArray().Select(x=>int.Parse(x.ToString())).ToArray();

            var sum =0;
            for (int i=0;i<chars.Length;i++) {
                var thisChar = chars[i];
                var nextChar = chars[i==chars.Length-1?0:i+1];
                if (thisChar==nextChar) sum+=nextChar;
            }

            return sum.ToString();
        }

        public override string SolvePart2(string input)
        {
            var chars =  input.ToCharArray().Select(x=>int.Parse(x.ToString())).ToArray();

            var sum2 = 0;
            var lenHalf = chars.Length/2;
            if (chars.Length %2!=0) throw new Exception();
            for (int i = 0; i < chars.Length; i++)
            {
                var thisChar = chars[i];
                var nextChar = chars[(i+lenHalf)%chars.Length];
                if (thisChar == nextChar) sum2 += thisChar;
            }

            return sum2.ToString();
        }
    }
}
