using System;

namespace TestFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
        public int Priority { get; set; } = 0;
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        public object[] Parameters { get; }
        public TestCaseAttribute(params object[] parameters) => Parameters = parameters;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreAttribute : Attribute
    {
        public string Reason { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)] public class BeforeAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class AfterAttribute : Attribute { }
}