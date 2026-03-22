using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using TestFramework;

namespace TestRunner
{
    class Program
    {
        static int passed = 0;
        static int failed = 0;
        static readonly object consoleLock = new object();

        static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom("Tests.dll");
            var testTypes = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null)).ToList();

            var allTestTasks = new List<Action>();

            foreach (var type in testTypes)
            {
                var beforeMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<BeforeAttribute>() != null);
                var afterMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<AfterAttribute>() != null);
                var testMethods = type.GetMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null);

                foreach (var method in testMethods)
                {
                    var testCases = method.GetCustomAttributes<TestCaseAttribute>().ToList();
                    if (!testCases.Any()) testCases.Add(new TestCaseAttribute(null));

                    foreach (var tc in testCases)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            allTestTasks.Add(() => ExecuteSingleTest(type, method, beforeMethod, afterMethod, tc));
                        }
                    }
                }
            }

            Console.WriteLine($"tasks: {allTestTasks.Count}\n");

            using (var pool = new MyThreadPool(minThreads: 2, maxThreads: 6, idleTimeoutMs: 3000, hungTimeoutMs: 5000))
            {
                Console.WriteLine("\n--- 1. slow ---");
                for (int i = 0; i < 5; i++)
                {
                    pool.EnqueueTask(allTestTasks[0]);
                    allTestTasks.RemoveAt(0);
                    Thread.Sleep(500);
                }

                Console.WriteLine("\n--- 2. burst ---");
                for (int i = 0; i < 25; i++)
                {
                    pool.EnqueueTask(allTestTasks[0]);
                    allTestTasks.RemoveAt(0);
                }

                Console.WriteLine("\n--- 3. idle ---");
                Thread.Sleep(5000);

                Console.WriteLine("\n--- 4. rest ---");
                foreach (var task in allTestTasks)
                {
                    pool.EnqueueTask(task);
                }

                pool.WaitAllAndDispose();
            }

            PrintSummary();
        }

        static void ExecuteSingleTest(Type type, MethodInfo method, MethodInfo beforeMethod, MethodInfo afterMethod, TestCaseAttribute tc)
        {
            var instance = Activator.CreateInstance(type);
            string testName = $"{method.Name}";

            beforeMethod?.Invoke(instance, null);

            try
            {
                object result = method.Invoke(instance, tc.Parameters?.Where(p => p != null).ToArray());
                if (result is System.Threading.Tasks.Task t) t.GetAwaiter().GetResult();

                PrintResult("pass", testName, ConsoleColor.Green);
                Interlocked.Increment(ref passed);
            }
            catch (Exception ex)
            {
                var realEx = ex is TargetInvocationException tie ? tie.InnerException : ex;
                if (realEx is TestFailedException fail)
                    PrintResult("fail", $"{testName} -> {fail.Message}", ConsoleColor.Red);
                else
                    PrintResult("err", $"{testName} -> {realEx?.Message}", ConsoleColor.DarkRed);

                Interlocked.Increment(ref failed);
            }
            finally
            {
                afterMethod?.Invoke(instance, null);
            }
        }

        static void PrintResult(string status, string message, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.Write($"[{status}] ");
                Console.ResetColor();
                Console.WriteLine($"{message} ({Thread.CurrentThread.Name})");
            }
        }

        static void PrintSummary()
        {
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine($"done. passed: {passed}, failed: {failed}");
            Console.WriteLine(new string('=', 40));
        }
    }
}