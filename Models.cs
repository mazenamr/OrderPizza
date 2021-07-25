using System.Collections.Generic;
using System.Linq;

namespace OrderPizza
{
    public record Pizza(string Name, double Price);
    public record Topping(string Name, double Price);
    public record Size(string Name, double Multiplier);

    class Models
    {
        public static double GetPrice(Pizza pizza, List<Topping> toppings) =>
            pizza.Price + toppings.Sum(t => t.Price);
        public static double GetPrice(Pizza pizza, List<Topping> toppings, Size size) =>
            GetPrice(pizza, toppings) * size.Multiplier;
    }
}