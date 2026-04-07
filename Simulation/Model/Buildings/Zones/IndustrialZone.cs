namespace Simulation.Model.Buildings.Zones;

public class IndustrialZone : ZoneWithJobs
{
    private static readonly string[] Prefixes         = ["Apex", "Vortex", "Elevate", "Zenith", "Nova", "Synapse", "Pulse", "Enigma", "Catalyst", "Axiom"];
    private static readonly string[] Suffixes          = ["Dynamics", "Ventures", "Solutions", "Technologies", "Innovations", "Industries", "Enterprises", "Systems", "Mechanics", "Manufacturing"];
    private static readonly string[] BusinessSuffixes  = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];

    public IndustrialZone(Tile tile, string type) : base(tile, type)
    {
        Name = $"{Pick(Prefixes)} {Pick(Suffixes)} {Pick(BusinessSuffixes)}";
    }

    private static string Pick(string[] arr) => arr[Random.Shared.Next(arr.Length)];
}
