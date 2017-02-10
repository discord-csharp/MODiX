namespace Monk
{
    class Program
    {
        static void Main(string[] args) => new MonkBot().Run().GetAwaiter().GetResult();
    }
}