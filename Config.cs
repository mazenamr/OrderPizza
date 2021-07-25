using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OrderPizza
{
    public class Config
    {
        public string OrdersPath { get; set; } = string.Empty;
        public List<Pizza> Pizzas { get; set; } = new();
        public List<Topping> Toppings { get; set; } = new();
        public List<Size> Sizes { get; set; } = new();
        public Pizza? GetPizza(string pizza) =>
            Pizzas.Where(p => p.Name == pizza).FirstOrDefault();
        public Topping? GetTopping(string topping) =>
            Toppings.Where(t => t.Name == topping).FirstOrDefault();
        public Size? GetSize(string size) =>
            Sizes.Where(s => s.Name == size).FirstOrDefault();
        public List<Topping> GetToppings(List<string> toppings)
        {
            List<Topping> result = new();
            toppings.ForEach(t =>
            {
                Topping? topping = GetTopping(t);
                if (topping is not null)
                {
                    result.Add(topping);
                }
            });
            return result;
        }

        public static Config Default => new Config
        {
            OrdersPath = "Orders",
            Pizzas = new List<Pizza>
            {
                new("Cheese", 25),
                new("Tuna", 40),
                new("Pepperoni", 50),
                new("Chicken", 50)
            },
            Toppings = new List<Topping>
            {
                new("Olive", 5),
                new("Pepper", 5),
                new("Mushroom", 10)
            },
            Sizes = new List<Size>
            {
                new("Small", 1),
                new("Medium", 1.5),
                new("Big", 2.5)
            }
        };
    }

    public class ConfigProvider
    {
        static string ConfigPath { get; set; } = "config.json";
        public Config Config { get; set; }

        public void SaveConfig() =>
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize<Config>(Config));

        private Config? LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                return JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
            }
            return null;
        }

        public ConfigProvider()
        {
            Config? config = LoadConfig();
            if (config is null)
            {
                Config = Config.Default;
                SaveConfig();
            }
            else
            {
                Config = config;
            }
        }
    }
}