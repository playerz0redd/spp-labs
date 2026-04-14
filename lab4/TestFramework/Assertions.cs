using System;
using System.Collections;
using System.Linq.Expressions;

namespace TestFramework
{
    public class TestFailedException : Exception
    {
        public TestFailedException(string message) : base(message) { }
    }

    public static class Assert
    {
        public static void IsTrue(bool condition, string msg = "") { if (!condition) throw new TestFailedException($"true expected. {msg}"); }
        public static void IsFalse(bool condition, string msg = "") { if (condition) throw new TestFailedException($"false expected. {msg}"); }
        public static void AreEqual(object expected, object actual) { if (!Equals(expected, actual)) throw new TestFailedException($"expected <{expected}>, got <{actual}>"); }
        public static void IsNotNull(object obj) { if (obj == null) throw new TestFailedException("expected not null."); }

        public static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            catch (Exception ex) { throw new TestFailedException($"expected {typeof(T).Name}, got {ex.GetType().Name}"); }
            throw new TestFailedException($"expected {typeof(T).Name}, but none thrown.");
        }

        public static void GreaterThan(int val, int threshold)
        {
            if (val <= threshold) throw new TestFailedException($"{val} is not greater than {threshold}");
        }

        public static void Expr(Expression<Func<bool>> expr)
        {
            var func = expr.Compile(); 
            if (!func()) 
            {
                string detail = expr.Body.ToString();

                if (expr.Body is BinaryExpression bin)
                {
      
                    object left = null, right = null;
                    try { left = Expression.Lambda(bin.Left).Compile().DynamicInvoke(); } catch { }
                    try { right = Expression.Lambda(bin.Right).Compile().DynamicInvoke(); } catch { }

                 
                    detail = $"значения: [{left}] {bin.NodeType} [{right}]. структура AST: ({bin.Left.NodeType} {bin.NodeType} {bin.Right.NodeType})";
                }
                throw new TestFailedException($"expr упал -> {detail}");
            }
        }
    }

}