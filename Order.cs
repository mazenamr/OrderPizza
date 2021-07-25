using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OrderPizza
{
    public class Order
    {
        public record Item(Pizza Pizza, List<Topping> Toppings, Size Size);
        public List<Item> Items { get; set; } = new();
        public static double GetPrice(Item item) => Models.GetPrice(item.Pizza, item.Toppings, item.Size);
        public double TotalPrice => Items.Sum(x => GetPrice(x));

        public void SaveOrder(string path) =>
            File.WriteAllText(path, JsonSerializer.Serialize<Order>(this));

        public static Order? LoadOrder(string path)
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<Order>(File.ReadAllText(path));
            }
            return null;
        }

        public static List<string> GetSavedOrders(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return new();
            }

            List<string> orders = Directory.GetFiles(path, "*.json")
                .Select(x => Path.GetFileName(x)).ToList();
            return orders;
        }
    }
}