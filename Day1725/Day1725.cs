using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using static AdventOfCode.Day1725.StringHelper;

namespace AdventOfCode.Day1725
{
    public class Day1725Solution:SolutionBase<string,string>
    {
        public override IEnumerable<string> GetPart1SampleInputs() => new[]
                                                                      {
                                                                          "Begin in state A." + Environment.NewLine +
                                                                          "Perform a diagnostic checksum after 6 steps." + Environment.NewLine +
                                                                          "" + Environment.NewLine +
                                                                          "In state A:" + Environment.NewLine +
                                                                          "  If the current value is 0:" + Environment.NewLine +
                                                                          "    - Write the value 1." + Environment.NewLine +
                                                                          "    - Move one slot to the right." + Environment.NewLine +
                                                                          "    - Continue with state B." + Environment.NewLine +
                                                                          "  If the current value is 1:" + Environment.NewLine +
                                                                          "    - Write the value 0." + Environment.NewLine +
                                                                          "    - Move one slot to the left." + Environment.NewLine +
                                                                          "    - Continue with state B." + Environment.NewLine +
                                                                          "" + Environment.NewLine +
                                                                          "In state B:" + Environment.NewLine +
                                                                          "  If the current value is 0:" + Environment.NewLine +
                                                                          "    - Write the value 1." + Environment.NewLine +
                                                                          "    - Move one slot to the left." + Environment.NewLine +
                                                                          "    - Continue with state A." + Environment.NewLine +
                                                                          "  If the current value is 1:" + Environment.NewLine +
                                                                          "    - Write the value 1." + Environment.NewLine +
                                                                          "    - Move one slot to the right." + Environment.NewLine +
                                                                          "    - Continue with state A." + Environment.NewLine
                                                                      };

        public override string GetPart1Input() => ReadInput();

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"3"};

        public override object SolvePart1(string input)
        {
            var cpu = new Cpu(input);
            cpu.DoAllCommands();
            return cpu.TouringMachine.CalculateDiagnosticsChecksum();
        }
    }

    internal class Cpu
    {
        private readonly Dictionary<char, Command> _commands;

        private char _currentState;

        private readonly int _dumpChecksumAfterOps;

        public TouringMachine TouringMachine { get; } = new TouringMachine();

        public Cpu(string program)
        {
            var programParts = SplitByEmptyLine(program);

            var header = SplitToLines(programParts[0]);
            _currentState = ParseWordFromEnd(header[0])[0];
            _dumpChecksumAfterOps = int.Parse(ParseWordFromEnd(header[1], 1));

            _commands = programParts.Skip(1)
                                    .Select(Command.Parse)
                                    .ToDictionary(x => x.InState, x => x);
        }

        public void DoNextCommand()
        {
            var command = _commands[_currentState];
            var currentState = TouringMachine.Current;

            var operation = currentState
                ? command.TrueOperation
                : command.FalseOperation;

            TouringMachine.Current = operation.WriteValue;
            TouringMachine.Move(operation.MoveDistance);
            _currentState = operation.NextState;
        }

        public void DoAllCommands()
        {
            for (var i = 0; i < _dumpChecksumAfterOps; i++)
                DoNextCommand();
        }
    }

    internal class TouringMachine
    {
        private readonly Dictionary<int, bool> _memory = new Dictionary<int, bool>();
        private int _currentAddress;

        public int CalculateDiagnosticsChecksum() => _memory.Values.Count(x => x);

        public void Move(int distance) => _currentAddress += distance;
        
        public bool Current
        {
            get
            {
                _memory.TryGetValue(_currentAddress, out var value);
                return value;
            }
            set => _memory[_currentAddress] = value;
        }
    }

    public class Command
    {
        public char InState { get; private set; }
        public Operation FalseOperation { get; private set; }
        public Operation TrueOperation { get;private set; }

        public static Command Parse(string s)
        {
            var lines = SplitToLines(s);

            return new Command
                   {
                       InState = ParseWordFromEnd(lines[0])[0],
                       FalseOperation = Operation.Parse(new ArraySegment<string>(lines, 2, 3).ToArray()),
                       TrueOperation = Operation.Parse(new ArraySegment<string>(lines, 6, 3).ToArray()),
                   };
        }
    }

    public class Operation
    {
        public bool WriteValue { get; private set; }
        public int MoveDistance { get; private set; }
        public char NextState { get; private set; }

        public static Operation Parse(string[] lines)
        {
            return new Operation
                   {
                       WriteValue = ParseWordFromEnd(lines[0]) == "1",
                       MoveDistance = ParseWordFromEnd(lines[1]) == "right" ? 1 : -1,
                       NextState = ParseWordFromEnd(lines[2])[0]
                   };
        }
    }

    static class StringHelper
    {
        public static string[] SplitToLines(string input) => input.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);

        public static string[] SplitByEmptyLine(string input) => input.Split(new[] {"\r\n\r\n", "\r\r", "\n\n"}, StringSplitOptions.None);

        public static string ParseWordFromEnd(string input, int wordIndexEol = 0)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Reverse().Skip(wordIndexEol).FirstOrDefault()?.TrimEnd('.');
        }
    }

    public class Day1725Test:TestBase<Day1725Solution,string,string>
    {
        public Day1725Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
