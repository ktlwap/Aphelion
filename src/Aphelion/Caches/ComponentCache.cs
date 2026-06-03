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

    private readonly object _lock = new();
    private readonly List<BaseComponent> _components = new();
    private readonly List<ComponentCacheEvent> _unitializedComponents = new();
    private BaseComponent[] _snapshot = Array.Empty<BaseComponent>();

    internal IReadOnlyList<BaseComponent> Components => _snapshot;

    internal void Register(BaseComponent baseComponent)
    {
        lock (_lock)
        {
            _unitializedComponents.Add(new ComponentCacheEvent
            {
                BaseComponent = baseComponent,
                EventType = ComponentCacheEventType.Added
            });
        }
    }

    internal void Unregister(BaseComponent baseComponent)
    {
        lock (_lock)
        {
            _unitializedComponents.Add(new ComponentCacheEvent
            {
                BaseComponent = baseComponent,
                EventType = ComponentCacheEventType.Removed
            });
        }
    }

    internal void Update()
    {
        ComponentCacheEvent[] events;
        lock (_lock)
        {
            if (_unitializedComponents.Count == 0)
                return;

            events = _unitializedComponents.ToArray();
            _unitializedComponents.Clear();

            foreach (var @event in events)
                if (ComponentCacheEventType.Added == @event.EventType)
                    _components.Add(@event.BaseComponent);
                else
                    _components.Remove(@event.BaseComponent);

            _snapshot = _components.ToArray();
        }

        foreach (var @event in events)
            if (ComponentCacheEventType.Added == @event.EventType)
                @event.BaseComponent.Start();
            else
                @event.BaseComponent.Stop();
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _components.Clear();
            _unitializedComponents.Clear();
            _snapshot = Array.Empty<BaseComponent>();
        }
    }
}
