using System;

// Собственное исключение для проваленных проверок
public class MyAssertException : Exception
{
    public MyAssertException(string msg) : base(msg) { }
}

public static class Assert
{
    public static void IsTrue(bool condition) { if (!condition) throw new MyAssertException("Expected True"); }
    public static void IsFalse(bool condition) { if (condition) throw new MyAssertException("Expected False"); }
    public static void AreEqual(object exp, object act) { if (!Equals(exp, act)) throw new MyAssertException($"Expected {exp}, but got {act}"); }
    public static void AreNotEqual(object exp, object act) { if (Equals(exp, act)) throw new MyAssertException($"Expected NOT {exp}, but got {act}"); }
    public static void IsNull(object obj) { if (obj != null) throw new MyAssertException("Expected Null"); }
    public static void IsNotNull(object obj) { if (obj == null) throw new MyAssertException("Expected Not Null"); }
    public static void GreaterThan(int a, int b) { if (a <= b) throw new MyAssertException($"{a} not greater than {b}"); }
    public static void LessThan(int a, int b) { if (a >= b) throw new MyAssertException($"{a} not less than {b}"); }
    public static void IsType<T>(object obj) { if (!(obj is T)) throw new MyAssertException($"Expected type {typeof(T).Name}"); }
    public static void Contains(string container, string item) { if (!container.Contains(item)) throw new MyAssertException($"'{container}' does not contain '{item}'"); }

    // Достаточно 10
}
