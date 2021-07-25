using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;

namespace OrderPizza
{
    class Display
    {
        public string MainMenu(Order order)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[underline]Order Summary[/]")
                .Expand();

            if (order.Items.Count > 0)
            {
                orderTable
                    .AddColumn(new TableColumn("Pizza")
                        .Footer("Total"))
                    .AddColumn(new TableColumn("Price")
                        .Footer(new Markup($"[bold lime]${order.TotalPrice}[/]"))
                        .Centered());

                foreach (var item in order.Items)
                {
                    string itemMarkup = $"[bold lightsteelblue]{item.Pizza.Name}[/]";
                    item.Toppings.ForEach(x => itemMarkup += $"\n  [italic lightcoral]{x.Name}[/]");
                    string priceMarkup = $"[bold lime]${Order.GetPrice(item)}[/]";
                    orderTable.AddRow(new Markup(itemMarkup), new Markup(priceMarkup));
                }
            }
            else
            {
                orderTable
                    .AddColumn(new TableColumn(Text.Empty).Centered())
                    .AddRow(new Rule("[italic silver]No Items Added[/]").Alignment(Justify.Left))
                    .HideHeaders();
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.MainMenuSelections));
        }

        public string GetPizza(List<Pizza> pizzas)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .AddChoices(pizzas.Select(p => $"{p.Name} [lime](${p.Price})[/]"));

            AnsiConsole.Clear();
            string[] selection = AnsiConsole
                .Prompt(prompt).Split(' ');

            return string.Join(' ', selection.Take(selection.Length - 1));
        }

        public List<string> GetToppings(List<Topping> toppings)
        {
            MultiSelectionPrompt<string> prompt = new MultiSelectionPrompt<string>()
                .AddChoices(toppings.Select(p => $"{p.Name} [lime](${p.Price})[/]"))
                .NotRequired();

            AnsiConsole.Clear();
            List<string[]> selections = AnsiConsole
                .Prompt(prompt).Select(x => x.Split(' ')).ToList();

            return selections.Select(x => string.Join(' ', x.Take(x.Length - 1))).ToList();
        }

        public string GetSize(List<Size> sizes, double price)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .AddChoices(sizes.Select(p => $"{p.Name} [lime](${price * p.Multiplier})[/]"));

            AnsiConsole.Clear();
            string[] selection = AnsiConsole
                .Prompt(prompt).Split(' ');

            return string.Join(' ', selection.Take(selection.Length - 1));
        }

        public void NotAvailable(string name, string error)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .Title($"[red bold]No {name} {error}![/]")
                .AddChoices("Press any key to continue...");
            prompt.HighlightStyle("dim italic silver");

            AnsiConsole.Clear();
            AnsiConsole.Prompt(prompt);
        }

        public int GetItem(List<Order.Item> items)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[underline]Added Pizzas[/]")
                .AddColumn(new TableColumn("# Index").Centered())
                .AddColumn(new TableColumn("Pizza"))
                .Expand();

            int i = 0;
            foreach (var item in items)
            {
                string indexMarkup = $"[bold silver]{++i}[/]";
                string itemMarkup = $"[bold lightsteelblue]{item.Pizza.Name}[/]";
                item.Toppings.ForEach(x => itemMarkup += $"\n  [italic lightcoral]{x.Name}[/]");
                orderTable.AddRow(new Markup(indexMarkup), new Markup(itemMarkup));
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(
                new TextPrompt<int>("Enter the number of the pizza you want to remove")
                    .DefaultValue(1)
                    .Validate(index => (index > 0 && index <= i) ?
                        ValidationResult.Success() :
                        ValidationResult.Error($"[red]Invalid selection {index}[/]"))) - 1;
        }

        public string GetOrderName(string OrdersLocation)
        {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter a name for the order")
                    .Validate(name => File.Exists(Path.Combine(OrdersLocation, name + ".json")) ?
                        ValidationResult.Error($"[red]A file with the same name already exists[/]") :
                        ValidationResult.Success()));
        }

        public string GetOrder(List<string> orders)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .AddChoices(orders.Select(x => x.Remove(x.Length - 5)));

            AnsiConsole.Clear();
            return AnsiConsole.Prompt(prompt);
        }

        public string SettingsMenu()
        {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.SettingsMenuSelections));
        }

        public string EditPizzas(List<Pizza> pizzas)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[underline]Edit Pizzas[/]")
                .AddColumn(new TableColumn("Pizza"))
                .AddColumn(new TableColumn("Price").Centered())
                .Expand();

            foreach (var pizza in pizzas)
            {
                string nameMarkup = $"[bold lightsteelblue]{pizza.Name}[/]";
                string priceMarkup = $"[bold lime]${pizza.Price}[/]";
                orderTable.AddRow(new Markup(nameMarkup), new Markup(priceMarkup));
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.AddRemoveSelections));
        }

        public string EditToppings(List<Topping> toppings)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[underline]Edit Toppings[/]")
                .AddColumn(new TableColumn("Topping"))
                .AddColumn(new TableColumn("Price").Centered())
                .Expand();

            foreach (var topping in toppings)
            {
                string nameMarkup = $"[bold lightsteelblue]{topping.Name}[/]";
                string priceMarkup = $"[bold lime]${topping.Price}[/]";
                orderTable.AddRow(new Markup(nameMarkup), new Markup(priceMarkup));
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.AddRemoveSelections));
        }

        public string EditSizes(List<Size> sizes)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[underline]Edit Sizes[/]")
                .AddColumn(new TableColumn("Size"))
                .AddColumn(new TableColumn("Multiplier").Centered())
                .Expand();

            foreach (var size in sizes)
            {
                string nameMarkup = $"[bold lightsteelblue]{size.Name}[/]";
                string multiplierMarkup = $"[bold silver]x{size.Multiplier}[/]";
                orderTable.AddRow(new Markup(nameMarkup), new Markup(multiplierMarkup));
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.AddRemoveSelections));
        }

        public string GetName(string name, List<string> existing)
        {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter a name for the {name}")
                    .Validate(name => existing.Contains(name) ?
                        ValidationResult.Error($"[red]A {name} with the same name already exists[/]") :
                        ValidationResult.Success()));
        }

        public double GetNum(string name, string intName) {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new TextPrompt<double>($"Enter a {intName} for the {name}")
                    .Validate(num => num <= 0 ?
                        ValidationResult.Error($"[red]The value must be greater than zero[/]") :
                        ValidationResult.Success()));
        }

        public List<string> GetNames(List<string> names)
        {
            MultiSelectionPrompt<string> prompt = new MultiSelectionPrompt<string>()
                .AddChoices(names)
                .NotRequired();

            AnsiConsole.Clear();
            List<string> selections = AnsiConsole.Prompt(prompt).ToList();
            if (selections.Count > 0)
            {
                SelectionPrompt<string> confirmPrompt = new SelectionPrompt<string>()
                    .Title("[bold red]Removing an item will result in deleting all the saved orders[/]")
                    .AddChoices(Selections.OkBackSelections);
                if (AnsiConsole.Prompt(confirmPrompt) == Selections.Back)
                {
                    return new();
                }
            }
            return selections;
        }
    }
}