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

internal static class GameObjectCache
{
    private static readonly List<GameObject> _gameObjects = new();
    private static readonly List<GameObjectCacheEvent> _unitializedGameObjects = new();

    internal static IReadOnlyList<GameObject> GameObjects => _gameObjects;

    internal static void Register(GameObject gameObject)
    {
        _unitializedGameObjects.Add(new GameObjectCacheEvent
        {
            GameObject = gameObject,
            EventType = GameObjectCacheEventType.Added
        });
    }

    internal static void Unregister(GameObject gameObject)
    {
        _unitializedGameObjects.Add(new GameObjectCacheEvent
        {
            GameObject = gameObject,
            EventType = GameObjectCacheEventType.Removed
        });
    }

    internal static bool IsNameAlreadyInUse(string name)
    {
        return _gameObjects.Any(go => go.Name == name);
    }

    internal static void Update()
    {
        var events = _unitializedGameObjects.ToArray();
        _unitializedGameObjects.Clear();

        foreach (var @event in events)
            if (GameObjectCacheEventType.Added == @event.EventType)
                _gameObjects.Add(@event.GameObject);
            else
                _gameObjects.Remove(@event.GameObject);
    }

    internal static void Clear()
    {
        _gameObjects.Clear();
        _unitializedGameObjects.Clear();
    }
}