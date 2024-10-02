using Microsoft.Extensions.DependencyInjection;

namespace MyTestMicroserviceApp.gRpcClientTestStubForService2;

file static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddGrpcClient<Service2.Service2Client>(o =>
        {
            o.Address = new Uri("https://localhost:5290");
        });
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<Service2.Service2Client>();
        
        double latitude, longitude;
        Console.Write("Введите географические координаты в градусах точки местоположения магазина ({широта}; {долгота}): ");
        var input = Console.ReadLine();
        while (true)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var stringsOfLatitudeAndLongitude = input.Split("; ");
                if (stringsOfLatitudeAndLongitude.Length == 2)
                    if (double.TryParse(stringsOfLatitudeAndLongitude[0], out latitude) &&
                        double.TryParse(stringsOfLatitudeAndLongitude[1], out longitude))
                        break;
            }
            
            Console.Write("Неправильный ввод. Исправьте и повторите попытку: ");
            input = Console.ReadLine();
        }

        var warehouseGuid = await client.GetNearestWarehouseGuidByPointAsync(
            new Point { Latitude = latitude, Longitude = longitude });
        Console.WriteLine($"Guid ближайшего к магазину склада: {warehouseGuid.Value}.");

        var products = await client.GetAllProductsInWarehouseAsync(new WarehouseGuid { Value = warehouseGuid.Value });
        Console.WriteLine($"Продукты на складе {warehouseGuid.Value}:");
        foreach (var product in products.Values)
        {
            Console.WriteLine($"\tGuid: {product.Guid}; Название: {product.Name}; Кол-во: {product.Quantity}.");
        }
        
        Console.Write("Для завершения программы нажмите любую клавишу...");
        Console.ReadKey();
        
        await Task.CompletedTask;
    }
}