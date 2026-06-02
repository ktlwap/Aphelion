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

internal static class ComponentCache
{
    private static readonly List<BaseComponent> _components = new();
    private static readonly List<ComponentCacheEvent> _unitializedComponents = new();

    internal static IReadOnlyList<BaseComponent> Components => _components;

    internal static void Register(BaseComponent baseComponent)
    {
        _unitializedComponents.Add(new ComponentCacheEvent
        {
            BaseComponent = baseComponent,
            EventType = ComponentCacheEventType.Added
        });
    }

    internal static void Unregister(BaseComponent baseComponent)
    {
        _unitializedComponents.Add(new ComponentCacheEvent
        {
            BaseComponent = baseComponent,
            EventType = ComponentCacheEventType.Removed
        });
    }

    internal static void Update()
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

    internal static void Clear()
    {
        _components.Clear();
        _unitializedComponents.Clear();
    }
}