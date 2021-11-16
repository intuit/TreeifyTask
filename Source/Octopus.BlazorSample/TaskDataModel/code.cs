using System;
using System.Threading.Tasks;

namespace Octopus.BlazorSample
{
    delegate Task CancellableAsyncActionDelegate(Pr reporter, Tk token);
    class Pr
    {
        public void report() { }
    }
    struct Tk
    {

    }
    class AT
    {
        public AT(string id,
                  CancellableAsyncActionDelegate action,
                  params AT[] children)
        {
            Id = id;
            Action = action;
            Children = children;
        }

        public string Id { get; }
        public CancellableAsyncActionDelegate Action { get; }
        public AT[] Children { get; }
    }

    public class CodeTry
    {
        public void SyntaxCheck()
        {
            var rootTask =
                new AT("Root", null,
                    new AT("Task1", Task1,
                        new AT("Task1.1", Task1_1),
                        new AT("Task1.2", Task1_2)),
                    new AT("Task2", null,
                        new AT("Task2.1", null,
                            new AT("Task2.1.1", Task2_1_1),
                            new AT("Task2.1.2", Task_2_1_2)),
                        new AT("Task2.2", Task2_2)),
                    new AT("Task3", Task3),
                    new AT("Task4", Task4),
                    new AT("Task5", Task5),
                    new AT("Task6", Task6));

        }

        private async Task Task6(Pr reporter, Tk token)
        {
            await Task.Yield();
        }

        private Task Task5(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task4(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task3(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task2_2(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task_2_1_2(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task2_1_1(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task1(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task1_1(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }

        private Task Task1_2(Pr reporter, Tk token)
        {
            throw new NotImplementedException();
        }
    }
}