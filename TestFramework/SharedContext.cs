using System.Collections.Generic;

// Реализация механизма разделяемого контекста (Shared Context)
public class TestContext
{
    public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();
}
