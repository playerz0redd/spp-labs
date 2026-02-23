using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestFramework;

namespace TestRunner
{
    class Program
    {
        static int passed = 0;
        static int failed = 0;
        static int skipped = 0;
        static int maxDegree = 5;

        static readonly object consoleLock = new object();

        static async Task Main(string[] args)
        {
            var assembly = Assembly.LoadFrom("Tests.dll");
            var testTypes = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null)).ToList();

            Console.WriteLine("=== СРАВНЕНИЕ ЭФФЕКТИВНОСТИ ===");

            Console.WriteLine("\n[Запуск 1: ПОСЛЕДОВАТЕЛЬНО]");
            ResetCounters();
            var swSeq = Stopwatch.StartNew();
            await RunTests(testTypes, maxDegreeOfParallelism: 1);
            swSeq.Stop();

            Console.WriteLine($"\n[Запуск 2: ПАРАЛЛЕЛЬНО]");
            ResetCounters();
            var swPar = Stopwatch.StartNew();

            await RunTests(testTypes, maxDegreeOfParallelism: maxDegree);
            swPar.Stop();

            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine("=== СРАВНЕНИЕ ВРЕМЕНИ ===");
            Console.WriteLine($"Последовательно: {swSeq.ElapsedMilliseconds} мс");
            Console.WriteLine($"Параллельно:     {swPar.ElapsedMilliseconds} мс");
            Console.WriteLine(new string('=', 40));
        }

        static void ResetCounters()
        {
            passed = 0; failed = 0; skipped = 0;
        }

        static async Task RunTests(List<Type> testTypes, int maxDegreeOfParallelism)
        {
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = new List<Task>();

            foreach (var type in testTypes)
            {
                var beforeMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<BeforeAttribute>() != null);
                var afterMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<AfterAttribute>() != null);

                var testMethods = type.GetMethods()
                    .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
                    .OrderBy(m => m.GetCustomAttribute<TestAttribute>().Priority);

                foreach (var method in testMethods)
                {
                    var ignoreAttr = method.GetCustomAttribute<IgnoreAttribute>();
                    if (ignoreAttr != null)
                    {
                        PrintResult("SKIP", $"{method.Name} (Причина: {ignoreAttr.Reason})", ConsoleColor.Yellow);
                        Interlocked.Increment(ref skipped);
                        continue;
                    }

                    var testCases = method.GetCustomAttributes<TestCaseAttribute>().ToList();
                    if (!testCases.Any()) testCases.Add(new TestCaseAttribute(null));

                    foreach (var tc in testCases)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(); 
                            try
                            {
                                await ExecuteSingleTestAsync(type, method, beforeMethod, afterMethod, tc);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                }
            }

            await Task.WhenAll(tasks);
            PrintSummary();
        }

        static async Task ExecuteSingleTestAsync(Type type, MethodInfo method, MethodInfo beforeMethod, MethodInfo afterMethod, TestCaseAttribute tc)
        {
            var instance = Activator.CreateInstance(type);
            string paramsInfo = tc.Parameters != null ? $"({string.Join(", ", tc.Parameters)})" : "";
            string testName = $"{method.Name}{paramsInfo}";

            var timeoutAttr = method.GetCustomAttribute<TimeoutAttribute>();
            int timeoutMs = timeoutAttr?.Milliseconds ?? Timeout.Infinite;

            beforeMethod?.Invoke(instance, null);

            try
            {
                Task testTask = Task.Run(async () =>
                {
                    object result = method.Invoke(instance, tc.Parameters?.Where(p => p != null).ToArray());
                    if (result is Task t) await t;
                });

                Task completedTask = await Task.WhenAny(testTask, Task.Delay(timeoutMs));

                if (completedTask == testTask)
                {
                    await testTask;
                    PrintResult("PASS", testName, ConsoleColor.Green);
                    Interlocked.Increment(ref passed); 
                }
                else
                {

                    PrintResult("FAIL", $"{testName} -> Превышено время ожидания ({timeoutMs} мс)", ConsoleColor.Magenta);
                    Interlocked.Increment(ref failed);
                }
            }
            catch (Exception ex)
            {
                var realEx = ex is TargetInvocationException tie ? tie.InnerException : ex;

                if (realEx is TestFailedException fail)
                    PrintResult("FAIL", $"{testName} -> {fail.Message}", ConsoleColor.Red);
                else
                    PrintResult("ERROR", $"{testName} -> Внезапное исключение: {realEx?.Message}", ConsoleColor.DarkRed);

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
                Console.WriteLine($"{message}");
            }
        }

        static void PrintSummary()
        {
            lock (consoleLock)
            {
                Console.WriteLine("\nИТОГИ:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"Пройдено: {passed}   ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"Провалено: {failed}   ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Пропущено: {skipped}");
                Console.ResetColor();
            }
        }
    }
}