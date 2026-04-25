using Arch.Core;
using Arch.Core.Extensions;

namespace CitySim.Simulation.Extensions;

public static class EntityExtensions
{

    public static void SafeAdd<T>(this Entity entity) where T : struct
    {
        if (!entity.Has<T>()) entity.Add<T>();
    }
    
    public static void SafeAdd<T>(this Entity entity, T component) where T : struct
    {
        if (!entity.Has<T>()) entity.Add<T>(component);
    }
}
