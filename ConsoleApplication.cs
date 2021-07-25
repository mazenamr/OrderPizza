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
                        Order = new();
                        break;
                    case Selections.SaveOrder:
                        SaveOrder();
                        break;
                    case Selections.LoadSavedOrder:
                        LoadOrder();
                        break;
                    case Selections.DeleteSavedOrder:
                        DeleteSavedOrders();
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
                _display.Error("No pizzas available\nAdd new pizzas from settings");
                return;
            }
            if (Config.Sizes.Count == 0)
            {
                _display.Error("No sizes available\nAdd new sizes from settings");
                return;
            }

            string pizzaChoice = _display.ChooseWithPrice(
                Config.Pizzas.Select(x => (x.Name, x.Price)));
            Pizza? pizza = Config.GetPizza(pizzaChoice);

            if (pizza is null)
            {
                return;
            }

            List<string> toppingChoices = _display.ChooseMultipleWithPrice(
                Config.Toppings.Select(x => (x.Name, x.Price)));
            List<Topping> toppings = Config.GetToppings(toppingChoices);

            if (Config.Toppings.Count == 0)
            {
                _display.Error("No toppings available\nAdd new toppings from settings");
            }

            string sizeChoice = _display.ChooseWithPrice(
                Config.Sizes.Select(x => (x.Name, Models.GetPrice(pizza, toppings, x))));
            Size? size = Config.GetSize(sizeChoice);

            if (size is null)
            {
                return;
            }

            Order.Items.Add(new(pizza, toppings, size));
        }

        private void RemovePizza()
        {
            if (Order.Items.Count == 0)
            {
                _display.Error("The cart is empty");
            }
            else
            {
                Order.Items.RemoveAt(_display.GetItem(Order.Items));
            }
        }

        private void SaveOrder()
        {
            if (!Directory.Exists(Config.OrdersPath))
            {
                Directory.CreateDirectory(Config.OrdersPath);
            }

            string name = $"{_display.GetName("order", Order.GetSavedOrders(Config.OrdersPath))}.json";
            Order.SaveOrder(Path.Combine(Config.OrdersPath, name));
            Order = new();
        }

        private void LoadOrder()
        {
            List<string> orders = Order.GetSavedOrders(Config.OrdersPath);
            if (orders.Count == 0)
            {
                _display.Error("No saved orders found");
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

        private void DeleteSavedOrders()
        {
            List<string> orders = Order.GetSavedOrders(Config.OrdersPath);
            if (orders.Count == 0)
            {
                _display.Error("No saved orders found");
            }
            else
            {
                string? selected = _display.GetOrder(orders);
                selected = orders
                    .Where(x => x.StartsWith(selected)).FirstOrDefault();
                if (!_display.Confirm("You can't recover the saved order once it's deleted"))
                {
                    return;
                }
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
                switch (_display.Choose(Selections.SettingsMenuSelections))
                {
                    case Selections.EditPizzas:
                        Config.Pizzas = EditList("Pizza", "Price", Config.Pizzas.ToList<Component>())
                            .Select(x => new Pizza(x.Name, x.Price)).ToList();
                        break;
                    case Selections.EditToppings:
                        Config.Toppings = EditList("Topping", "Price", Config.Toppings.ToList<Component>())
                            .Select(x => new Topping(x.Name, x.Price)).ToList();
                        break;
                    case Selections.EditSizes:
                        Config.Sizes = EditList("Size", "Multiplier", Config.Sizes.ToList<Component>())
                            .Select(x => new Size(x.Name, x.Price)).ToList();
                        break;
                    default:
                        return;
                }
                _configProvider.SaveConfig();
            }
        }

        private List<Component> EditList(string itemName, string priceName, List<Component> list)
        {
            while (true)
            {
                switch (_display.EditComponent(list.Select(x => (x.Name, x.Price)), itemName, priceName))
                {
                    case Selections.Add:
                        AddToList(itemName, priceName, list);
                        break;
                    case Selections.Remove:
                        RemoveFromList(list);
                        break;
                    default:
                        return list;
                }
            }
        }

        private void AddToList(string itemName, string priceName, List<Component> list)
        {
            string name = _display.GetName(itemName, list.Select(x => x.Name));
            double price = _display.GetNumber(itemName, priceName);
            list.Add(new Component(name, price));
            _configProvider.SaveConfig();
        }

        private void RemoveFromList(List<Component> list)
        {
            List<string> names = _display.GetNames(list.Select(x => x.Name).ToList());
            if (names.Count > 0)
            {
                Order = new();
                if (Directory.Exists(Config.OrdersPath))
                {
                    Directory.Delete(Config.OrdersPath, true);
                }
            }
            list.RemoveAll(x => names.Contains(x.Name));
            _configProvider.SaveConfig();
        }
    }
}