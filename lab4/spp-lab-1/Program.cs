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
            var testTypes = assembly.GetTypes().Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null)).ToList();

            var allTestTasks = new List<Action>();

            Func<MethodInfo, bool> filterDelegate = m =>
            {
                var cat = m.GetCustomAttribute<CategoryAttribute>();
                return cat == null || cat.Name != "skip";
            };

            foreach (var type in testTypes)
            {
                var before = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<BeforeAttribute>() != null);
                var after = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<AfterAttribute>() != null);

               
                var testMethods = type.GetMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null).Where(filterDelegate);

                foreach (var method in testMethods)
                {
          
                    var testCases = method.GetCustomAttributes<TestCaseAttribute>().ToList();
                    foreach (var tc in testCases)
                    {
                        for (int i = 0; i < 3; i++) allTestTasks.Add(() => ExecuteSingleTest(type, method, before, after, tc.Parameters));
                    }

                    if (!testCases.Any() && method.GetCustomAttribute<TestCaseSourceAttribute>() == null)
                    {
                        for (int i = 0; i < 3; i++) allTestTasks.Add(() => ExecuteSingleTest(type, method, before, after, null));
                    }

        
                    var sourceAttr = method.GetCustomAttribute<TestCaseSourceAttribute>();
                    if (sourceAttr != null)
                    {
                        var sourceMethod = type.GetMethod(sourceAttr.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (sourceMethod != null)
                        {

                            var data = (IEnumerable<object[]>)sourceMethod.Invoke(null, null);
                            foreach (var argsData in data)
                            {
                                allTestTasks.Add(() => ExecuteSingleTest(type, method, before, after, argsData));
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"tasks after filter: {allTestTasks.Count}\n");

            using (var pool = new MyThreadPool(minThreads: 2, maxThreads: 6, idleTimeoutMs: 3000, hungTimeoutMs: 5000))
            {
              
                pool.OnPoolEvent += (msg, color) =>
                {
                    lock (consoleLock) { Console.ForegroundColor = color; Console.WriteLine(msg); Console.ResetColor(); }
                };

                Console.WriteLine("\n--- start load ---");
                foreach (var task in allTestTasks) pool.EnqueueTask(task);
                pool.WaitAllAndDispose();
            }

            Console.WriteLine($"\ndone. passed: {passed}, failed: {failed}");
            Thread.Sleep(30000000);
        }

        static void ExecuteSingleTest(Type type, MethodInfo method, MethodInfo before, MethodInfo after, object[] args)
        {
            var instance = Activator.CreateInstance(type);
            string argsStr = args != null ? $"({string.Join(",", args)})" : "";
            string testName = $"{method.Name}{argsStr}";

            before?.Invoke(instance, null);
            try
            {
                object result = method.Invoke(instance, args);
                if (result is System.Threading.Tasks.Task t) t.GetAwaiter().GetResult();

                lock (consoleLock) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"[pass] {testName} ({Thread.CurrentThread.Name})"); Console.ResetColor(); }
                Interlocked.Increment(ref passed);
            }
            catch (Exception ex)
            {
                var realEx = ex is TargetInvocationException tie ? tie.InnerException : ex;
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[fail] {testName} -> {realEx?.Message} ({Thread.CurrentThread.Name})");
                    Console.ResetColor();
                }
                Interlocked.Increment(ref failed);
            }
            finally { after?.Invoke(instance, null); }
        }
    }
}