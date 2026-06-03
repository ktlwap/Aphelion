namespace Aphelion.Rendering;

public class DrawCommandBuffer
{
    private readonly List<DrawCommand> _drawCommands = new();

    public IReadOnlyList<DrawCommand> DrawCommands => _drawCommands;

    public void DrawShape(DrawShapeCommand command)
    {
        _drawCommands.Add(command);
    }

    public void DrawTexture(DrawTextureCommand command)
    {
        _drawCommands.Add(command);
    }

    public void DrawText(DrawTextCommand command)
    {
        _drawCommands.Add(command);
    }

    internal void Clear()
    {
        _drawCommands.Clear();
    }
}