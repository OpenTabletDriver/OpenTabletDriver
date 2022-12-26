using System.Threading.Tasks;

namespace OpenTabletDriver.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ApplicationBuilder();
            var app = builder.Build();
            await app.Run(args);
        }
    }
}
