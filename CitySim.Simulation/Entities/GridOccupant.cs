using CitySim.Simulation.Components;
using TypedArch;

namespace CitySim.Simulation.Entities;

interface IGridOccupant : IAbstractArcheType
{
    GridPosition GridPosition { get; }
    PowerConductor PowerConductor { get; }
    BuildingState BuildingState { get; }
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

interface IEmployer : IBuilding
{
    Employer Employer { get; }
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

interface IResidential : IBuilding, IArcheType
{
    Residence Residence { get; }
    Vacancies? Vacancies { get; }
}

interface ICommercial : IEmployer, IArcheType
{
    Commercial Commercial { get; }
}

interface IIndustrial: IEmployer, IArcheType
{
    Industrial Industrial { get; }
}
