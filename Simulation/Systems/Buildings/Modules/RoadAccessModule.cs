using Simulation.Model.Buildings;

namespace Simulation.Systems.Buildings.Modules;

using Simulation.Model;

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
