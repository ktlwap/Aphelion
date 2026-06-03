using Aphelion.Core;

namespace Aphelion.Caches;

internal enum GameObjectCacheEventType
{
    Added,
    Removed
}

internal struct GameObjectCacheEvent
{
    internal GameObject GameObject;
    internal GameObjectCacheEventType EventType;
}

internal class GameObjectCache
{
    internal static ThreadLocal<GameObjectCache> Instance = new (() => new GameObjectCache());

    private readonly object _lock = new();
    private readonly List<GameObject> _gameObjects = new();
    private readonly List<GameObjectCacheEvent> _unitializedGameObjects = new();

    internal IReadOnlyList<GameObject> GameObjects
    {
        get { lock (_lock) return _gameObjects.ToArray(); }
    }

    internal void Register(GameObject gameObject)
    {
        lock (_lock)
        {
            _unitializedGameObjects.Add(new GameObjectCacheEvent
            {
                GameObject = gameObject,
                EventType = GameObjectCacheEventType.Added
            });
        }
    }

    internal void Unregister(GameObject gameObject)
    {
        lock (_lock)
        {
            _unitializedGameObjects.Add(new GameObjectCacheEvent
            {
                GameObject = gameObject,
                EventType = GameObjectCacheEventType.Removed
            });
        }
    }

    internal bool IsNameAlreadyInUse(string name)
    {
        lock (_lock)
            return _gameObjects.Any(go => go.Name == name);
    }

    internal void Update()
    {
        lock (_lock)
        {
            if (_unitializedGameObjects.Count == 0)
                return;

            foreach (var @event in _unitializedGameObjects)
                if (GameObjectCacheEventType.Added == @event.EventType)
                    _gameObjects.Add(@event.GameObject);
                else
                    _gameObjects.Remove(@event.GameObject);

            _unitializedGameObjects.Clear();
        }
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _gameObjects.Clear();
            _unitializedGameObjects.Clear();
        }
    }
}
