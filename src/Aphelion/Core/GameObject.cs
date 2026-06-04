using Aphelion.Caches;

namespace Aphelion.Core;

public sealed class GameObject
{
    private readonly List<BaseComponent> _components = new();
    public readonly Transform Transform;
    public readonly Input Input;

    public string Name { get; set; }
    
    private GameObject(string name)
    {
        Name = name;
        Transform = new Transform(this);
        Input = Input.Current;
    }

    /// <summary>
    /// Provides a read-only collection of components attached to the current GameObject.
    /// </summary>
    /// <remarks>
    /// This property grants access to all the components associated with a GameObject.
    /// It returns an immutable list of components, ensuring modifications cannot be
    /// made directly to the collection. Components can be added or removed using the
    /// respective methods provided by the GameObject class.
    /// <para />
    /// Note: Accessing this property is an expensive operation as it creates a copy of the internal list.
    /// </remarks>
    /// <value>
    /// A read-only list of <see cref="BaseComponent" /> instances.
    /// </value>
    public IReadOnlyList<BaseComponent> Components => _components.AsReadOnly();

    public static GameObject Instantiate(string name)
    {
        if (GameObjectCache.Instance.Value!.IsNameAlreadyInUse(name))
            throw new Exception("Name already in use: " + name);

        var gameObject = new GameObject(name);
        GameObjectCache.Instance.Value!.Register(gameObject);
        return gameObject;
    }

    public static GameObject Destroy(GameObject gameObject)
    {
        GameObjectCache.Instance.Value!.Unregister(gameObject);
        foreach (var component in gameObject.Components)
            ComponentCache.Instance.Value!.Unregister(component);
        return gameObject;
    }

    public TComponent AddComponent<TComponent>() where TComponent : BaseComponent
    {
        if (GetComponentNullSafe<TComponent>() is not null)
            throw new Exception("Component already exists: " + typeof(TComponent).Name + "");

        var component = (TComponent)Activator.CreateInstance(typeof(TComponent), true)!;

        var type = typeof(BaseComponent);
        type.GetProperty("GameObject")!.SetValue(component, this);
        type.GetProperty("Transform")!.SetValue(component, Transform);

        _components.Add(component);

        ComponentCache.Instance.Value!.Register(component);

        return component;
    }

    public void RemoveComponent<TComponent>() where TComponent : BaseComponent
    {
        BaseComponent component = GetComponent<TComponent>();
        ComponentCache.Instance.Value!.Unregister(component);
        _components.Remove(component);
    }

    public TComponent GetComponent<TComponent>() where TComponent : BaseComponent
    {
        var component = GetComponentNullSafe<TComponent>();
        if (component is null)
            throw new Exception("Component not found: " + typeof(TComponent).Name + "");

        return component;
    }

    public TComponent? GetComponentNullSafe<TComponent>() where TComponent : BaseComponent
    {
        return (TComponent?)_components.Find(c => typeof(TComponent) == c.GetType());
    }
}