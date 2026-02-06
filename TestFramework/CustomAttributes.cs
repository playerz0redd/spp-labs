using System;

namespace TestFramework
{
    // Атрибут для пометки тестового метода
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
        public int Priority { get; set; } = 0; // Свойство: приоритет (0 - высший)
        public string Description { get; set; }
    }

    // Атрибут для передачи параметров в тест
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        public object[] Parameters { get; }
        public TestCaseAttribute(params object[] parameters) => Parameters = parameters;
    }

    // Атрибут для игнорирования теста
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreAttribute : Attribute
    {
        public string Reason { get; set; }
    }

    // Атрибуты для подготовки и очистки (Контекст)
    [AttributeUsage(AttributeTargets.Method)] public class BeforeAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class AfterAttribute : Attribute { }
}