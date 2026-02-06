using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestClassAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class MyTestAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SetupAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class TearDownAttribute : Attribute { }

// Атрибут с параметрами для TestCase
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute : Attribute
{
    public object[] Params { get; }
    public TestCaseAttribute(params object[] parameters) => Params = parameters;
}
