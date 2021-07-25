using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OrderPizza
{
    class ConsoleApplication
    {
        private readonly ConfigProvider _configProvider;
        private readonly Display _display;
        private Config Config => _configProvider.Config;
        private Order Order { get; set; } = new();

        public ConsoleApplication(ConfigProvider configProvider, Display display)
        {
            _configProvider = configProvider;
            _display = display;
        }

        public void Run()
        {
            while (true)
            {
                switch (_display.MainMenu(Order))
                {
                    case Selections.AddPizza:
                        AddPizza();
                        break;
                    case Selections.RemovePizza:
                        RemovePizza();
                        break;
                    case Selections.NewOrder:
                        NewOrder();
                        break;
                    case Selections.SaveOrder:
                        SaveOrder();
                        break;
                    case Selections.LoadOrder:
                        LoadOrder();
                        break;
                    case Selections.DeleteOrder:
                        DeleteOrder();
                        break;
                    case Selections.Settings:
                        Settings();
                        break;
                    default:
                        return;
                }
            }
        }

        private void AddPizza()
        {
            if (Config.Pizzas.Count == 0)
            {
                _display.NotAvailable("pizzas", "available");
                return;
            }
            if (Config.Sizes.Count == 0)
            {
                _display.NotAvailable("sizes", "available");
                return;
            }
            Pizza? pizza = Config.GetPizza(_display.GetPizza(Config.Pizzas));
            if (pizza is null)
                return;
            List<Topping> toppings = new();
            if (Config.Toppings.Count == 0)
            {
                _display.NotAvailable("toppings", "available");
            }
            else
            {
                toppings = Config.GetToppings(_display.GetToppings(Config.Toppings));
            }
            Size? size = Config.GetSize(_display.GetSize(Config.Sizes, Models.GetPrice(pizza, toppings)));
            if (size is null)
                return;
            Order.Items.Add(new(pizza, toppings, size));
        }

        private void RemovePizza()
        {
            if (Order.Items.Count == 0)
            {
                _display.NotAvailable("pizzas", "added to cart");
            }
            else
            {
                Order.Items.RemoveAt(_display.GetItem(Order.Items));
            }
        }

        private void NewOrder()
        {
            Order = new();
        }

        private void SaveOrder()
        {
            if (!Directory.Exists(Config.OrdersPath))
            {
                Directory.CreateDirectory(Config.OrdersPath);
            }

            string name = $"{_display.GetOrderName(Config.OrdersPath)}.json";
            Order.SaveOrder(Path.Combine(Config.OrdersPath, name));
        }

        private void LoadOrder()
        {
            if (!Directory.Exists(Config.OrdersPath))
            {
                Directory.CreateDirectory(Config.OrdersPath);
            }

            List<string> orders = Directory.GetFiles(Config.OrdersPath, "*.json")
                .Select(x => Path.GetFileName(x)).ToList();

            if (orders.Count == 0)
            {
                _display.NotAvailable("orders", "saved");
            }
            else
            {
                string? selected = _display.GetOrder(orders);
                selected = orders
                    .Where(x => x.StartsWith(selected)).FirstOrDefault();
                if (selected is null)
                {
                    return;
                }
                Order? order = Order.LoadOrder(Path.Combine(Config.OrdersPath, selected));
                if (order is null)
                {
                    return;
                }
                Order = order;
            }
        }

        private void DeleteOrder()
        {
            if (!Directory.Exists(Config.OrdersPath))
            {
                Directory.CreateDirectory(Config.OrdersPath);
            }

            List<string> orders = Directory.GetFiles(Config.OrdersPath, "*.json")
                .Select(x => Path.GetFileName(x)).ToList();

            if (orders.Count == 0)
            {
                _display.NotAvailable("orders", "saved");
            }
            else
            {
                string? selected = _display.GetOrder(orders);
                selected = orders
                    .Where(x => x.StartsWith(selected)).FirstOrDefault();
                if (selected is null)
                {
                    return;
                }
                File.Delete(Path.Combine(Config.OrdersPath, selected));
            }
        }

        private void Settings()
        {
            while (true)
            {
                switch (_display.SettingsMenu())
                {
                    case Selections.EditPizzas:
                        EditPizzas();
                        break;
                    case Selections.EditToppings:
                        EditToppings();
                        break;
                    case Selections.EditSizes:
                        EditSizes();
                        break;
                    default:
                        return;
                }
            }
        }

        private void EditPizzas()
        {
            while (true)
            {
                switch (_display.EditPizzas(Config.Pizzas))
                {
                    case Selections.Add:
                        NewPizza();
                        break;
                    case Selections.Remove:
                        DeletePizza();
                        break;
                    default:
                        return;
                }
            }
        }

        private void EditToppings()
        {
            while (true)
            {
                switch (_display.EditToppings(Config.Toppings))
                {
                    case Selections.Add:
                        NewTopping();
                        break;
                    case Selections.Remove:
                        DeleteTopping();
                        break;
                    default:
                        return;
                }
            }
        }

        private void EditSizes()
        {
            while (true)
            {
                switch (_display.EditSizes(Config.Sizes))
                {
                    case Selections.Add:
                        NewSize();
                        break;
                    case Selections.Remove:
                        DeleteSize();
                        break;
                    default:
                        return;
                }
            }
        }

        private void NewPizza()
        {
            string name = _display.GetName("pizza", Config.Pizzas.Select(x => x.Name).ToList());
            double price = _display.GetNum("pizza", "price");
            Config.Pizzas.Add(new Pizza(name, price));
            _configProvider.SaveConfig();
        }

        private void DeletePizza()
        {
            List<string> names = _display.GetNames(Config.Pizzas.Select(x => x.Name).ToList());
            if (names.Count > 0)
            {
                Order = new();
                Directory.Delete(Config.OrdersPath, true);
            }
            Config.Pizzas.RemoveAll(x => names.Contains(x.Name));
            _configProvider.SaveConfig();
        }

        private void NewTopping()
        {
            string name = _display.GetName("topping", Config.Toppings.Select(x => x.Name).ToList());
            double price = _display.GetNum("topping", "price");
            Config.Toppings.Add(new Topping(name, price));
            _configProvider.SaveConfig();
        }

        private void DeleteTopping()
        {
            List<string> names = _display.GetNames(Config.Toppings.Select(x => x.Name).ToList());
            if (names.Count > 0)
            {
                Order = new();
                Directory.Delete(Config.OrdersPath, true);
            }
            Config.Toppings.RemoveAll(x => names.Contains(x.Name));
            _configProvider.SaveConfig();
        }

        private void NewSize()
        {
            string name = _display.GetName("size", Config.Sizes.Select(x => x.Name).ToList());
            double multiplier = _display.GetNum("size", "multiplier");
            Config.Sizes.Add(new Size(name, multiplier));
            _configProvider.SaveConfig();
        }

        private void DeleteSize()
        {
            List<string> names = _display.GetNames(Config.Sizes.Select(x => x.Name).ToList());
            if (names.Count > 0)
            {
                Order = new();
                Directory.Delete(Config.OrdersPath, true);
            }
            Config.Sizes.RemoveAll(x => names.Contains(x.Name));
            _configProvider.SaveConfig();
        }
    }
}