using Aphelion.Rendering;

namespace Aphelion.Core;

public abstract class BaseComponent
{
    public required Transform Transform { get; init; }

    public required GameObject GameObject { get; init; }

    public virtual void Start() { }

    public virtual void Update() { }
    
    public virtual void Render(DrawCommandBuffer buffer) { }
    
    public virtual void RenderUI(DrawCommandBuffer buffer) { }

    public virtual void Stop() { }
}