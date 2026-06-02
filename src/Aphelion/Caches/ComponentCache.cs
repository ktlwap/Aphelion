using Aphelion.Core;

namespace Aphelion.Caches;

internal enum ComponentCacheEventType
{
    Added,
    Removed
}

internal struct ComponentCacheEvent
{
    internal BaseComponent BaseComponent;
    internal ComponentCacheEventType EventType;
}

internal class ComponentCache
{
    internal static ThreadLocal<ComponentCache> Instance = new (() => new ComponentCache());
    
    private readonly List<BaseComponent> _components = new();
    private readonly List<ComponentCacheEvent> _unitializedComponents = new();

    internal IReadOnlyList<BaseComponent> Components => _components;

    internal void Register(BaseComponent baseComponent)
    {
        _unitializedComponents.Add(new ComponentCacheEvent
        {
            BaseComponent = baseComponent,
            EventType = ComponentCacheEventType.Added
        });
    }

    internal void Unregister(BaseComponent baseComponent)
    {
        _unitializedComponents.Add(new ComponentCacheEvent
        {
            BaseComponent = baseComponent,
            EventType = ComponentCacheEventType.Removed
        });
    }

    internal void Update()
    {
        foreach (var @event in _unitializedComponents)
            if (ComponentCacheEventType.Added == @event.EventType)
                _components.Add(@event.BaseComponent);
            else
                _components.Remove(@event.BaseComponent);

        foreach (var @event in _unitializedComponents)
            if (ComponentCacheEventType.Added == @event.EventType)
                @event.BaseComponent.Start();
            else
                @event.BaseComponent.Stop();

        _unitializedComponents.Clear();
    }

    internal void Clear()
    {
        _components.Clear();
        _unitializedComponents.Clear();
    }
}