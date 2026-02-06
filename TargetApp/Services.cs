using System.Threading.Tasks;

public class DataService
{
    public int Add(int a, int b) => a + b;

    public async Task<string> FetchDataAsync()
    {
        await Task.Delay(100); // Имитация долгой работы
        return "DataLoaded";
    }
}
