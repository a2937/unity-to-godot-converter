using Godot;

public partial class UseComponent : Node
{
    private Sprite2D sr;
    public override void _Ready()
    {
        sr = GetNode<Sprite2D>(".");
        sr.SetDisabled(false);
    }
}