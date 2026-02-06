using System.Threading.Tasks;
using System;

[TestClass]
public class DemoTests
{
    private DataService _service;

    [Setup]
    public void Init()
    {
        _service = new DataService();
    }

    // 1. PASS: Обычный тест на сложение двух положительных чисел
    [MyTest]
    public void Test_Add_Positive()
    {
        Assert.AreEqual(15, _service.Add(5, 10));
    }

    // 2. FAIL: Ожидаем -1, а будет -2.
    [MyTest]
    public void Test_Add_Negative_ShouldFail()
    {
        Assert.AreEqual(-1, _service.Add(-5, 3));
    }

    // 3. PASS: Сложение с нулем
    [MyTest]
    public void Test_Add_WithZero()
    {
        Assert.AreEqual(100, _service.Add(0, 100));
    }

    // 4. PASS: Проверка асинхронного метода
    [MyTest]
    public async Task Test_Async_FetchData()
    {
        string result = await _service.FetchDataAsync();
        Assert.AreEqual("DataLoaded", result);
    }

    // 5. FAIL: Ожидаем пустую строку, а получаем int.
    [MyTest]
    public void Test_Add_TypeCheck_ShouldFail()
    {
        int result = _service.Add(1, 1);
        Assert.IsType<string>((object)result);
    }

    // 6. PASS: Проверка, что объект не null
    [MyTest]
    public void Test_Service_IsNotNull()
    {
        Assert.IsNotNull(_service);
    }

    // 7. FAIL: Проверка на null
    [MyTest]
    public void Test_NullCheck_ShouldFail()
    {
        object obj = "I am not null";
        Assert.IsNull(obj);
    }

    // 8. PASS: Сравнение строк (Contains)
    [MyTest]
    public void Test_StringContains()
    {
        Assert.Contains("HelloWorld", "World");
    }

    // 9. PASS: Проверка условия (GreaterThan)
    [MyTest]
    public void Test_GreaterThan()
    {
        Assert.GreaterThan(20, 10);
    }

    // 10. FAIL: Проверка условия (LessThan)
    [MyTest]
    public void Test_LessThan_ShouldFail()
    {
        Assert.LessThan(20, 10);
    }

    // Дополнительный 11-й тест для запаса
    [MyTest]
    public void Test_AreNotEqual_Pass()
    {
        Assert.AreNotEqual(1, 2);
    }


    // Набор с TestCase остается как демонстрация функционала
    [TestCase(1, 1, 2)]
    [TestCase(-1, 1, 0)]
    [TestCase(-10, -20, -25)] // FAIL
    public void Test_Add_Comprehensive(int a, int b, int expected)
    {
        Assert.AreEqual(expected, _service.Add(a, b));
    }


    [TearDown]
    public void Cleanup()
    {
    }
}
