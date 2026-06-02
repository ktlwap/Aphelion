namespace Aphelion.Core;

public abstract class BaseComponent
{
    public required Transform Transform { get; init; }

    public required GameObject GameObject { get; init; }

    public virtual void Start()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void Stop()
    {
    }
}