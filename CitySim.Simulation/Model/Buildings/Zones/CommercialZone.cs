namespace CitySim.Simulation.Model.Buildings.Zones;

public class CommerceState
{
    public int Capacity { get; set; }
}

public class CommercialZone : ZoneWithJobs
{
    public CommerceState Commerce { get; } = new();

    private static readonly string[] Prefixes         = ["Prime", "Elite", "Global", "Exquisite", "Vibrant", "Luxury", "Innovative", "Sleek", "Premium", "Dynamic"];
    private static readonly string[] Suffixes          = ["Commerce", "Trade", "Marketplace", "Ventures", "Enterprises", "Retail", "Group", "Emporium", "Boutique", "Mall"];
    private static readonly string[] BusinessSuffixes  = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];

    public CommercialZone(Tile tile, string type) : base(tile, type)
    {
        Name = $"{Pick(Prefixes)} {Pick(Suffixes)} {Pick(BusinessSuffixes)}";
    }

    private static string Pick(string[] arr) => arr[Random.Shared.Next(arr.Length)];
}
