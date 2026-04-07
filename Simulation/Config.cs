namespace Simulation;

public static class SimConfig
{
    public static class Modules
    {
        public static class Development
        {
            public const int    AbandonThreshold  = 10;
            public const double AbandonChance     = 0.25;
            public const int    ConstructionTime  = 3;
            public const double LevelUpChance     = 0.05;
            public const double RedevelopChance   = 0.25;
        }
        public static class Jobs
        {
            public const int    MaxWorkers       = 2;
            public const double ApplicantChance  = 0.5;
        }
        public static class Residents
        {
            public const int    MaxResidents          = 2;
            public const double ResidentMoveInChance  = 0.5;
        }
        public static class Commerce
        {
            public const int Capacity = 10;
        }
        public static class RoadAccess
        {
            public const int SearchDistance = 3;
        }
    }
    public static class Citizen
    {
        public const int MinWorkingAge       = 16;
        public const int RetirementAge       = 65;
        public const int MaxJobSearchDistance = 4;
    }
}
