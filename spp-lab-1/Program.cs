using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestFramework;

namespace TestRunner
{
    class Program
    {
        // Счетчики для статистики
        static int passed = 0;
        static int failed = 0;
        static int skipped = 0;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Запуск автоматизированного тестирования ===\n");

            // Находим все типы, в которых есть методы с атрибутом [Test]
            // Для корректной работы убедитесь, что в TestRunner добавлена ссылка на проект Tests
            var assembly = Assembly.LoadFrom("/Users/pavelplayerz0redd/Projects/spp-lab-1/spp-lab-1/bin/Debug/net7.0/Tests.dll");
            var testTypes = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null));

            foreach (var type in testTypes)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"--- Тестовый класс: {type.Name} ---");
                Console.ResetColor();

                var instance = Activator.CreateInstance(type);

                var beforeMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<BeforeAttribute>() != null);
                var afterMethod = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<AfterAttribute>() != null);

                var testMethods = type.GetMethods()
                    .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
                    .OrderBy(m => m.GetCustomAttribute<TestAttribute>().Priority);

                foreach (var method in testMethods)
                {
                    var testAttr = method.GetCustomAttribute<TestAttribute>();
                    var ignoreAttr = method.GetCustomAttribute<IgnoreAttribute>();

                    // 1. Обработка SKIP (пропуск)
                    if (ignoreAttr != null)
                    {
                        PrintResult("SKIP", $"{method.Name} (Причина: {ignoreAttr.Reason})", ConsoleColor.Yellow);
                        skipped++;
                        continue;
                    }

                    var testCases = method.GetCustomAttributes<TestCaseAttribute>().ToList();
                    if (!testCases.Any()) testCases.Add(new TestCaseAttribute(null));

                    foreach (var tc in testCases)
                    {
                        string paramsInfo = tc.Parameters != null ? $"({string.Join(", ", tc.Parameters)})" : "";

                        try
                        {
                            // Подготовка (Before)
                            beforeMethod?.Invoke(instance, null);

                            // Выполнение теста
                            object result = method.Invoke(instance, tc.Parameters?.Where(p => p != null).ToArray());
                            if (result is Task task) await task;

                            // 2. Обработка PASS (успех)
                            PrintResult("PASS", $"{method.Name}{paramsInfo}", ConsoleColor.Green);
                            passed++;
                        }
                        catch (TargetInvocationException ex)
                        {
                            // 3. Обработка FAIL (ошибка логики/проверки)
                            if (ex.InnerException is TestFailedException fail)
                            {
                                PrintResult("FAIL", $"{method.Name}{paramsInfo} -> {fail.Message}", ConsoleColor.Red);
                            }
                            else // Обработка непредвиденной ошибки в коде
                            {
                                PrintResult("ERROR", $"{method.Name}{paramsInfo} -> Внезапное исключение: {ex.InnerException?.Message}", ConsoleColor.DarkRed);
                            }
                            failed++;
                        }
                        finally
                        {
                            // Очистка (After)
                            afterMethod?.Invoke(instance, null);
                        }
                    }
                }
            }

            PrintSummary();
        }

        // Вспомогательный метод для красивого вывода
        static void PrintResult(string status, string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write($"[{status}] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        // Вывод итоговой статистики
        static void PrintSummary()
        {
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine("ИТОГИ ТЕСТИРОВАНИЯ:");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Пройдено: {passed}");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Провалено: {failed}");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Пропущено: {skipped}");

        }
    }
}