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
    
    private readonly List<GameObject> _gameObjects = new();
    private readonly List<GameObjectCacheEvent> _unitializedGameObjects = new();

    internal IReadOnlyList<GameObject> GameObjects => _gameObjects;

    internal void Register(GameObject gameObject)
    {
        _unitializedGameObjects.Add(new GameObjectCacheEvent
        {
            GameObject = gameObject,
            EventType = GameObjectCacheEventType.Added
        });
    }

    internal void Unregister(GameObject gameObject)
    {
        _unitializedGameObjects.Add(new GameObjectCacheEvent
        {
            GameObject = gameObject,
            EventType = GameObjectCacheEventType.Removed
        });
    }

    internal bool IsNameAlreadyInUse(string name)
    {
        return _gameObjects.Any(go => go.Name == name);
    }

    internal void Update()
    {
        var events = _unitializedGameObjects.ToArray();
        _unitializedGameObjects.Clear();

        foreach (var @event in events)
            if (GameObjectCacheEventType.Added == @event.EventType)
                _gameObjects.Add(@event.GameObject);
            else
                _gameObjects.Remove(@event.GameObject);
    }

    internal void Clear()
    {
        _gameObjects.Clear();
        _unitializedGameObjects.Clear();
    }
}