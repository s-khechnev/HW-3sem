using System;
using System.Diagnostics;

namespace LinqTasks
{
    class Program
    {

        static int IsPerfect(int num)
        {
            int sum = 1;

            sum += Enumerable.Range(2, (int)Math.Sqrt(num) - 1).Where(n => num % n == 0)
                    .Select(n => new { FirstDiv = (int)n, SecondDiv = (int)num / n })
                    .Sum(n => n.FirstDiv + n.SecondDiv);

            if (sum == num)
                return num;
            else
                return -1;
        }

        static void Main(string[] args)
        {
            int N = 10000000;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Enumerable.Range(2, N).AsParallel().Select(num => IsPerfect(num)).Where(num => num != -1).ForAll(Console.WriteLine);

            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine(elapsedTime);
        }
    }
}
