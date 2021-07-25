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
                        LoadSavedOrder();
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
                int selection = _display.GetItem(Order.Items);
                if (_display.Confirm("You can't recover the pizza once it's deleted"))
                {
                    Order.Items.RemoveAt(selection);
                }
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

        private void LoadSavedOrder()
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
            if (!_display.Confirm("Entering the settings will result in deleting all the saved orders"))
            {
                return;
            }

            Order = new();
            if (Directory.Exists(Config.OrdersPath))
            {
                Directory.Delete(Config.OrdersPath, true);
            }

            while (true)
            {
                switch (_display.Choose(Selections.SettingsMenuSelections))
                {
                    case Selections.EditPizzas:
                        Config.Pizzas = EditList(Config.Pizzas.ToList<Component>(), "Pizza", "Price")
                            .Select(x => new Pizza(x.Name, x.Price)).ToList();
                        break;
                    case Selections.EditToppings:
                        Config.Toppings = EditList(Config.Toppings.ToList<Component>(), "Topping", "Price")
                            .Select(x => new Topping(x.Name, x.Price)).ToList();
                        break;
                    case Selections.EditSizes:
                        Config.Sizes = EditList(Config.Sizes.ToList<Component>(), "Size", "Multiplier")
                            .Select(x => new Size(x.Name, x.Price)).ToList();
                        break;
                    case Selections.RestoreDefaults:
                        _configProvider.Config = Config.Default;
                        break;
                    default:
                        return;
                }
                _configProvider.SaveConfig();
            }
        }

        private List<Component> EditList(List<Component> list, string stringName, string intName)
        {
            while (true)
            {
                switch (_display.EditComponent(list.Select(x => (x.Name, x.Price)), stringName, intName))
                {
                    case Selections.Add:
                        string name = _display.GetName(stringName, list.Select(x => x.Name));
                        double price = _display.GetNumber(stringName, intName);
                        list.Add(new Component(name, price));
                        break;
                    case Selections.Remove:
                        List<string> names = _display.GetNames(list.Select(x => x.Name));
                        if (_display.Confirm("You can't recover the " + stringName.ToLower() + " once it's deleted"))
                        {
                            list.RemoveAll(x => names.Contains(x.Name));
                        }
                        break;
                    default:
                        return list;
                }
            }
        }
    }
}