using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode.Day1816
{
    public class Day1816Solution : SolutionBase<string[], (string[] examples, string[] operations)>
    {
        public override IEnumerable<string[]> GetPart1SampleInputs()
            => new[]
               {
                   new[]
                   {
                       "Before: [3, 2, 1, 1]" + Environment.NewLine +
                       "9 2 1 2" + Environment.NewLine +
                       "After:  [3, 2, 2, 1]"
                   }
               };

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"1"};

        public override string[] GetPart1Input()
        {
            var text = ReadInput("_examples.txt");
            var examples = text.Split(Environment.NewLine + Environment.NewLine);
            return examples;
        }

        public override (string[] examples, string[] operations) GetPart2Input()
            => (GetPart1Input(), ReadInputLines("_instructions.txt"));

        public override object SolvePart1(string[] input)
        {
            var examples = input.Select(x => new Example(x));

            var results = examples.Select(x => RunExample(x))
                                  .ToList();

            return (
                from result in results
                where result.Count(x => x.exampleValid) >= 3
                select result
            ).Count();
        }

        public override object SolvePart2((string[] examples, string[] operations) input)
        {
            var examples = input.examples.Select(x => new Example(x));
            var operations = input.operations.Select(x => x.Split(" ").Select(int.Parse).ToArray());

            var instructionsByOpcodes = Enumerable.Range(0, 16)
                                                  .Select(i => Cpu.Instructions.ToList())
                                                  .ToArray();

            foreach (var example in examples)
            {
                
                var opcode = example.Instruction[0];
                var opcodeInstructions = instructionsByOpcodes[opcode];
                var results = RunExample(example, opcodeInstructions);
                var notPossibleInstructions = from result in results
                                              where !result.exampleValid
                                              select result.instruction;

                foreach(var notPossibleInstruction in notPossibleInstructions.ToArray())
                        opcodeInstructions.Remove(notPossibleInstruction);
            }

            do
            {
                var certainInstructions = (
                    from i in instructionsByOpcodes
                    where i.Count == 1
                    select i[0]
                ).ToArray();

                var uncertain =
                    from i in instructionsByOpcodes
                    where i.Count > 1
                    select i;

                foreach (var u in uncertain)
                foreach (var c in certainInstructions)
                    if (u.Contains(c))
                        u.Remove(c);

            } while (!instructionsByOpcodes.All(x => x.Count == 1));


            var cpu = new Cpu();
            foreach (var op in operations)
            {
                var instruction = instructionsByOpcodes[op[0]][0];
                cpu.DoInstruction(instruction, op);
            }

            return cpu.Registers[0];
        }

        private IEnumerable<(Instruction instruction, int[] result, bool exampleValid)> RunExample(Example example, IEnumerable<Instruction> instructions = null)
            => from instruction in instructions ?? Cpu.Instructions
               let cpu = new Cpu(example.RegistersBefore)
               let result = cpu.DoInstruction(instruction, example.Instruction)
               select (instruction, result, result.SequenceEqual(example.RegistersAfter));
    }

    class Cpu
    {
        public int[] Registers { get; }


        public Cpu(int[] initialRegisterStates = null)
        {
            Registers = (initialRegisterStates ?? new int[4]).ToArray();
        }

        public int[] DoInstruction(Instruction i, int[] parameters)
        {
            var paramA = parameters[1];
            var paramB = parameters[2];
            var outputRegister = parameters[3];

            var a = i.ParamAVal ? paramA : Registers[paramA];
            var b = i.ParamBVal ? paramB : Registers[paramB];

            var result = i.Do(a, b);

            Registers[outputRegister] = result;

            return Registers;
        }

        public static readonly Instruction[] Instructions =
        {
            new Instruction("addr", (a, b) => a + b),
            new Instruction("addi", (a, b) => a + b, paramBVal: true),

            new Instruction("mulr", (a, b) => a * b),
            new Instruction("muli", (a, b) => a * b, paramBVal:true),

            new Instruction("banr", (a, b) => a & b),
            new Instruction("bani", (a, b) => a & b, paramBVal:true),

            new Instruction("borr", (a, b) => a | b),
            new Instruction("bori", (a, b) => a | b, paramBVal:true),

            new Instruction("setr", (a, b) => a),
            new Instruction("seti", (a, b) => a, paramAVal: true),

            new Instruction("gtir", (a, b) => a > b ? 1 : 0, paramAVal:true),
            new Instruction("gtri", (a, b) => a > b ? 1 : 0, paramBVal:true),
            new Instruction("gtrr", (a, b) => a > b ? 1 : 0),

            new Instruction("eqir", (a, b) => a == b ? 1 : 0, paramAVal:true),
            new Instruction("eqri", (a, b) => a == b ? 1 : 0, paramBVal:true),
            new Instruction("eqrr", (a, b) => a == b ? 1 : 0),
        };
    }

    class Instruction
    {
        public bool ParamAVal { get; }
        public bool ParamBVal { get; }
        public string Name { get; }
        public DoInstruction Do { get; }


        public Instruction(string name, DoInstruction @do, bool paramAVal = false, bool paramBVal = false)
        {
            Name = name;
            Do = @do;
            ParamAVal = paramAVal;
            ParamBVal = paramBVal;
        }

        public delegate int DoInstruction(int a, int b);
    }


    class Example
    {
        public int[] RegistersBefore { get; }
        public int[] Instruction { get; }
        public int[] RegistersAfter { get; }

        public Example(string input)
        {
            var lines = input.Split(Environment.NewLine);

            RegistersBefore = SplitParse(lines[0].Replace("Before: [", "").Replace("]", ""), ", ");
            Instruction = SplitParse(lines[1], " ");
            RegistersAfter = SplitParse(lines[2].Replace("After:  [", "").Replace("]", ""), ", ");

            int[] SplitParse(string str, string seperator)
                => str.Split(seperator)
                      .Select(int.Parse)
                      .ToArray();
        }
    }

    public class Day1816Test : TestBase<Day1816Solution, string[], (string[] examples, string[] operations)>
    {
        public Day1816Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
