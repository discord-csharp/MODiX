namespace Modix
{
    class Program
    {
        static void Main(string[] args) => new ModixBot().Run().GetAwaiter().GetResult();
    }
}