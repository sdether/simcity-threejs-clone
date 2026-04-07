namespace Simulation.Model.Buildings.Zones;

public class ResidentialZone : Zone
{
    public List<Citizen> Residents    { get; set; } = [];
    public int           MaxResidents { get; set; }

    public int Vacancies => MaxResidents - Residents.Count;

    private static readonly string[] Prefixes = ["Emerald", "Ivory", "Crimson", "Opulent", "Celestial", "Enchanted", "Serene", "Whispering", "Stellar", "Tranquil"];
    private static readonly string[] Suffixes  = ["Tower", "Residence", "Manor", "Court", "Plaza", "House", "Mansion", "Place", "Villa", "Gardens"];

    public ResidentialZone(Tile tile, string type) : base(tile, type)
    {
        Name = $"{Pick(Prefixes)} {Pick(Suffixes)}";
    }

    private static string Pick(string[] arr) => arr[Random.Shared.Next(arr.Length)];
}
