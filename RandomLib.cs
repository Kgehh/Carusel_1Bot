
namespace Carusel_1Bot
{
    public class RandomLib
    {
        public List<string> Cars = new()
        {
            "Cresta",
            "Calinvagen",
            "Accord",
            "Gnivic"
        };

        public List<string> Coffee = new()
        {
            "BabaRoma",
            "Ёлки",
            "Ярче",
            "RocketCoffee",
            "Kafedra",
            "Территория кофе",
        };

        public Dictionary<string, int> CoffeePercent = new()
        {
            {"RocketCoffee"     ,20},
            {"Ярче"             ,20},
            {"Абрикос/Spar"     ,20},
            {"BabaRoma"         ,15},
            {"Ёлки"             ,10},
            {"Kafedra"          ,5},
            {"Paradox"          ,5},
            {"Территория кофе"  ,5}
        };

        public string GetCarImagePath(string carName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Images", "Cars", $"{carName}.jpg");
        }
    }
}
