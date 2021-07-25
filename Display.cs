using System.Collections.Generic;
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

        public string Choose(IEnumerable<string> choices)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .AddChoices(choices);

            AnsiConsole.Clear();
            return AnsiConsole.Prompt(prompt);
        }

        public List<string> ChooseMultiple(IEnumerable<string> choices)
        {
            MultiSelectionPrompt<string> prompt = new MultiSelectionPrompt<string>()
                .AddChoices(choices)
                .NotRequired();

            AnsiConsole.Clear();
            return AnsiConsole.Prompt(prompt);
        }

        public string ChooseWithPrice(IEnumerable<(string name, double price)> choices)
        {
            string[] selection = Choose(choices.Select(x => $"{x.name} [lime](${x.price})[/]")).Split(' ');
            return string.Join(' ', selection.Take(selection.Length - 1));
        }

        public List<string> ChooseMultipleWithPrice(IEnumerable<(string name, double price)> choices)
        {
            List<string[]> selections = ChooseMultiple(choices.Select(x => $"{x.name} [lime](${x.price})[/]"))
                .Select(x => x.Split(' ')).ToList();
            return selections.Select(x => string.Join(' ', x.Take(x.Length - 1))).ToList();
        }

        public void Error(string message)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .Title($"[red bold]{message}[/]")
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

        public string GetOrder(List<string> orders)
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .AddChoices(orders.Select(x => x.Remove(x.Length - 5)));

            AnsiConsole.Clear();
            return AnsiConsole.Prompt(prompt);
        }

        public string EditComponent(IEnumerable<(string name, double price)> available, string name, string priceName)
        {
            Table orderTable = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[underline]Edit {name}[/]")
                .AddColumn(new TableColumn(name))
                .AddColumn(new TableColumn(priceName).Centered())
                .Expand();

            foreach (var item in available)
            {
                string nameMarkup = $"[bold lightsteelblue]{item.name}[/]";
                string priceMarkup = $"[bold silver]{item.price}[/]";
                orderTable.AddRow(new Markup(nameMarkup), new Markup(priceMarkup));
            }

            AnsiConsole.Clear();
            AnsiConsole.Render(orderTable);
            return AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(Selections.AddRemoveSelections));
        }

        public bool Confirm(string message)
        {
            SelectionPrompt<string> confirmPrompt = new SelectionPrompt<string>()
                .Title($"[bold red]{message}[/]")
                .AddChoices(Selections.OkBackSelections);

            AnsiConsole.Clear();
            return (AnsiConsole.Prompt(confirmPrompt) == Selections.Ok);
        }

        public string GetName(string name, IEnumerable<string> existing)
        {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter a name for the {name}")
                    .Validate(name => existing.Contains(name) ?
                        ValidationResult.Error($"[red]A {name} with the same name already exists[/]") :
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
                if (Confirm("Removing an item will result in deleting all the saved orders"))
                {
                    return selections;
                }
            }
            return new();
        }

        public double GetNumber(string name, string intName) {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new TextPrompt<double>($"Enter a {intName} for the {name}")
                    .Validate(num => num <= 0 ?
                        ValidationResult.Error($"[red]The value must be greater than zero[/]") :
                        ValidationResult.Success()));
        }

    }
}