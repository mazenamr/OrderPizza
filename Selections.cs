using System.Collections.Generic;

namespace OrderPizza
{
    public static class Selections
    {
        #region OkBackSelections
        public const string Back = "Back";
        public const string Ok = "Ok";
        public static List<string> OkBackSelections = new List<string> { Ok, Back };
        #endregion

        #region MainMenuSelections
        public const string AddPizza = "Add Pizza";
        public const string RemovePizza = "Remove Pizza";
        public const string NewOrder = "New Order";
        public const string SaveOrder = "Save Order";
        public const string LoadSavedOrder = "Load Saved Order";
        public const string DeleteSavedOrder = "Delete Saved Orders";
        public const string Settings = "Settings";
        public const string Quit = "Quit";
        public static List<string> MainMenuSelections = new List<string>
            { AddPizza, RemovePizza, NewOrder, SaveOrder, LoadSavedOrder, DeleteSavedOrder, Settings, Quit };
        #endregion

        #region SettingsMenuSelections
        public const string EditPizzas = "Edit Pizzas";
        public const string EditToppings = "Edit Toppings";
        public const string EditSizes = "Edit Sizes";

        public static List<string> SettingsMenuSelections = new List<string>
            { EditPizzas, EditToppings, EditSizes, Back };
        #endregion

        #region AddRemoveSelections
        public const string Add = "Add";
        public const string Remove = "Remove";
        public static List<string> AddRemoveSelections = new List<string>
            { Add, Remove, Back };
        #endregion
    }
}