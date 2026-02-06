using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static void Main(string[] args)
    {

        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom("/Users/pavelplayerz0redd/Projects/spp-lab-1/spp-lab-1/bin/Debug/net7.0/Tests.dll");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: MyTests.dll not found");
            return;
        }
        // -----------------------

        var context = new TestContext();

        Console.WriteLine("Starting Test Run:");

        foreach (var type in assembly.GetTypes().Where(t => t.GetCustomAttribute<TestClassAttribute>() != null))
        {
            Console.WriteLine($"\n--- Running tests in {type.Name} ---");
            var instance = Activator.CreateInstance(type);
            var methods = type.GetMethods();

            // Find Setup and TearDown methods once per class
            var setupMethod = methods.FirstOrDefault(m => m.GetCustomAttribute<SetupAttribute>() != null);
            var tearDownMethod = methods.FirstOrDefault(m => m.GetCustomAttribute<TearDownAttribute>() != null);

            foreach (var method in methods.Where(m => m.GetCustomAttribute<MyTestAttribute>() != null || m.GetCustomAttributes<TestCaseAttribute>().Any()))
            {
                try
                {
                    // Setup (requires context injection)
                    if (setupMethod != null)
                    {
                        if (setupMethod.GetParameters().Any()) setupMethod.Invoke(instance, new[] { context });
                        else setupMethod.Invoke(instance, null);
                    }

                    // Handle TestCase attributes
                    var testCases = method.GetCustomAttributes<TestCaseAttribute>();
                    if (testCases.Any())
                    {
                        foreach (var tc in testCases)
                        {
                            ExecuteTestMethod(method, instance, tc.Params);
                        }
                    }
                    else
                    {
                        ExecuteTestMethod(method, instance, null);
                    }

                    Console.WriteLine($"[PASS] {method.Name}");

                }
                catch (TargetInvocationException ex)
                {
                    // Перехват внутреннего исключения (MyAssertException)
                    Console.WriteLine($"[FAIL] {method.Name}: {ex.InnerException?.Message}");
                }
                finally
                {
                    // TearDown (очистка)
                    tearDownMethod?.Invoke(instance, null);
                }
            }
        }
        Console.WriteLine("\n--- Test Run Completed ---");
    }

    // Хелпер для обработки асинхронных методов
    private static void ExecuteTestMethod(MethodInfo method, object instance, object[] parameters)
    {
        if (method.ReturnType == typeof(Task) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
        {
            var task = (Task)method.Invoke(instance, parameters);
            task.Wait();
        }
        else
        {
            method.Invoke(instance, parameters);
        }
    }
}
