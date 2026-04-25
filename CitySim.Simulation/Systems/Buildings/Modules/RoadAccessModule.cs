using CitySim.Simulation.Model.Buildings;

namespace CitySim.Simulation.Systems.Buildings.Modules;

using CitySim.Simulation.Model;

public class RoadAccessModule : SimModule
{
    public override void Simulate(World world, Building building)
    {
        var road = TileTools.FindTile(
            world,
            building.Tile.X,
            building.Tile.Y,
            t => t.Building?.Type == BuildingType.Road,
            SimConfig.Modules.RoadAccess.SearchDistance);

        var has = road is not null;
        if (building.HasRoadAccess != has)
        {
            building.HasRoadAccess = has;
            building.Updated = building.Tile.Updated = true;
        }
    }
}
