using CitySim.Simulation.Components;
using TypedArch;

namespace CitySim.Simulation.Entities;

interface IGridOccupant : IAbstractArcheType
{
    GridPosition GridPosition { get; }
    PowerConductor PowerConductor { get; }
}

interface IGridOccupantWithRoadAccess : IGridOccupant
{
    RoadAccessUser RoadAccessUser { get; }
}

interface IBuilding : IGridOccupantWithRoadAccess
{
    PowerConsumer PowerConsumer { get; }
    Development Development { get; }
}

interface IPowerLine : IGridOccupant, IArcheType
{
    PowerLine PowerLine { get; }
}

interface IRoad : IGridOccupant, IArcheType
{
    Road Road { get; }
}

interface IPowerPlant : IGridOccupantWithRoadAccess, IArcheType
{
    PowerPlant PowerPlant { get; }
}

interface IResidential : IGridOccupantWithRoadAccess, IArcheType
{
    Residence Residence { get; }
}

interface ICommercial : IGridOccupantWithRoadAccess, IArcheType
{
    Commercial Commercial { get; }
}

interface IIndustrial: IGridOccupantWithRoadAccess, IArcheType
{
    Industrial Industrial { get; }
}