namespace TimeRecorderConsoleApp
{
    using System;
    using System.Threading;

    using UsageStats;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var source = TimeRecorder.RunAsync();
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(1000);
            }

            source.Cancel();
        }
    }
}
