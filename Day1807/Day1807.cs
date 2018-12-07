using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace AdventOfCode.Day1807
{
    public class Day1807Solution : SolutionBase<string,(int workerCount, int additionalWorkTime, string steps)>
    {
        public override IEnumerable<string> GetPart1SampleInputs()
            => new[]
               {
                   string.Join(Environment.NewLine,

                               "Step C must be finished before step A can begin.",
                               "Step C must be finished before step F can begin.",
                               "Step A must be finished before step B can begin.",
                               "Step A must be finished before step D can begin.",
                               "Step B must be finished before step E can begin.",
                               "Step D must be finished before step E can begin.",
                               "Step F must be finished before step E can begin.")
               };
        public override IEnumerable<(int, int, string)> GetPart2SampleInputs() => new[] {(2, 0, GetPart1SampleInputs().First())};

        public override IEnumerable<string> GetPart1SampleOutputs() => new[] {"CABDFE"};
        public override IEnumerable<string> GetPart2SampleOutputs() => new[] {"15"};

        public override string GetPart1Input() => ReadInput();
        public override (int, int, string) GetPart2Input() => (5, 60, ReadInput());

        public override object SolvePart1(string input)
        {
            var stepper = new Stepper(input);
            while(stepper.CanStep) stepper.DoStep();
            return stepper.FinishedStepsString;
        }

        public override object SolvePart2((int workerCount, int additionalWorkTime, string steps) input)
        {
            int StepDuration(char step) => input.additionalWorkTime + step - 'A' + 1;

            var stepper = new Stepper(input.steps);
            var timeOffset = 0;
            var workers = Enumerable.Repeat(true, input.workerCount)
                                    .Select(x => new Worker())
                                    .ToArray();

            while (true)
            {
                CollectFinishedWork();

                var nextSteps = stepper.NextSteps().ToArray();

                if (!nextSteps.Any()) return timeOffset;

                foreach (var nextStep in nextSteps.Except(StepsInProcessing()))
                {
                    if (!ScheduleWork(nextStep))
                        break;
                }

                timeOffset = NextFinishTime();
            }

            void CollectFinishedWork()
            {
                foreach (var finishedWorker in FinishedWorkers())
                {
                    stepper.DoStep(finishedWorker.WorkinOnStep);
                    finishedWorker.BusyUntil = 0;
                }
            }

            bool ScheduleWork(char step)
            {
                var nextWorker = IdleWorkers().FirstOrDefault();
                if (nextWorker == null) return false;

                var workDuration = StepDuration(step);
                nextWorker.WorkinOnStep = step;
                nextWorker.BusyUntil = timeOffset + workDuration;
                return true;
            }

            int NextFinishTime() =>
            (
                from worker in WorkingWorkers()
                orderby worker.BusyUntil
                select worker.BusyUntil
            ).First();

            IEnumerable<Worker> IdleWorkers() => from worker in workers
                                                 where worker.BusyUntil == 0
                                                 select worker;

            IEnumerable<Worker> WorkingWorkers() => from worker in workers
                                                    where worker.BusyUntil > 0
                                                    select worker;

            IEnumerable<Worker> FinishedWorkers() => from worker in workers
                                                     where worker.BusyUntil > 0
                                                     where worker.BusyUntil <= timeOffset
                                                     select worker;

            IEnumerable<char> StepsInProcessing()=> from worker in WorkingWorkers()
                                                  select worker.WorkinOnStep;

        }

        class Worker
        {
            public char WorkinOnStep { get; set; }
            public int BusyUntil { get; set; }
        }
    }

    class Stepper
    {
        public List<char> FinishedSteps { get; } = new List<char>();

        public string FinishedStepsString => new string(FinishedSteps.ToArray());

        public Dictionary<char, char[]> Steps { get;  }

        public IEnumerable<char> NextSteps() => from step in Steps
                                                where !FinishedSteps.Contains(step.Key)
                                                where step.Value.All(depStep => FinishedSteps.Contains(depStep))
                                                select step.Key;

        public char NextStep() => NextSteps().First();

        public bool CanStep => FinishedSteps.Count < Steps.Count;

        public Stepper(string input)
        {
            Steps = Parse(input);
        }

        public void DoStep() => FinishedSteps.Add(NextStep());
        public void DoStep(char step) => FinishedSteps.Add(step);




        Dictionary<char, char[]> Parse(string input)
        {
            var matches = Regex.Matches(input,
                                        @"Step (?<dependency>.) must be finished before step (?<step>.) can begin.");

            var instructions = (
                from match in matches
                let step = match.Groups["step"].Value.Single()
                let dependentStep = match.Groups["dependency"].Value.Single()
                select (step, dependentStep)
            ).ToArray();

            var steps = (
                from i in instructions
                group i by i.step
                into g
                let dependentSteps = (
                    from depStep in g
                    select depStep.dependentStep
                ).ToArray()
                orderby g.Key
                select (step: g.Key, dependentSteps)
            ).ToDictionary(x => x.step, x => x.dependentSteps);

            var allDependentSteps = from i in instructions
                                    select i.dependentStep;

            var firstStep = allDependentSteps.Except(steps.Keys).Single();

            steps.Add(firstStep, new char[0]);

            return steps;
        }

    }

    public class Day1807Test : TestBase<Day1807Solution, string, (int,int,string)>
    {
        public Day1807Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}
