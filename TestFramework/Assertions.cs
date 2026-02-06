using System;
using System.Collections;

namespace TestFramework
{
    public class TestFailedException : Exception
    {
        public TestFailedException(string message) : base(message) { }
    }

    public static class Assert
    {
        public static void IsTrue(bool condition, string msg = "") { if (!condition) throw new TestFailedException($"True expected. {msg}"); }
        public static void IsFalse(bool condition, string msg = "") { if (condition) throw new TestFailedException($"False expected. {msg}"); }
        public static void AreEqual(object expected, object actual) { if (!Equals(expected, actual)) throw new TestFailedException($"Expected <{expected}>, but got <{actual}>"); }
        public static void AreNotEqual(object expected, object actual) { if (Equals(expected, actual)) throw new TestFailedException($"Values are equal, but expected different."); }
        public static void IsNull(object obj) { if (obj != null) throw new TestFailedException("Expected null."); }
        public static void IsNotNull(object obj) { if (obj == null) throw new TestFailedException("Expected not null."); }

        public static void Contains(string substring, string fullString)
        {
            if (!fullString.Contains(substring)) throw new TestFailedException($"String '{fullString}' does not contain '{substring}'");
        }

        public static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            catch (Exception ex) { throw new TestFailedException($"Expected exception {typeof(T).Name}, but got {ex.GetType().Name}"); }
            throw new TestFailedException($"Expected exception {typeof(T).Name}, but none was thrown.");
        }

        public static void IsEmpty(IEnumerable collection)
        {
            if (collection.GetEnumerator().MoveNext()) throw new TestFailedException("Collection is not empty.");
        }

        public static void GreaterThan(int val, int threshold)
        {
            if (val <= threshold) throw new TestFailedException($"{val} is not greater than {threshold}");
        }
    }
}