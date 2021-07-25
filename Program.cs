using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrderPizza
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ConfigProvider>()
                .AddSingleton<Display>()
                .AddSingleton<ConsoleApplication>()
                .BuildServiceProvider();

            IServiceScope scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ConsoleApplication>().Run();
            ((IDisposable)serviceProvider).Dispose();
        }
    }
}