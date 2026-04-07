namespace Simulation.Model.Buildings;

public class PowerConsumer
{
    public int Required { get; set; }
    public int Supplied { get; set; }

    public PowerConsumer(int required)
    {
        Required = required;
        Supplied = 0;
    }

    public PowerConsumer Copy() => new(Required) { Supplied = Supplied };

    public bool IsEqual(PowerConsumer other) =>
        Required == other.Required && Supplied == other.Supplied;
}
