using System;
using System.Threading.Tasks;

namespace Modix
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                await new ModixBot().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(ex.HResult);
            }
        }
    }
}