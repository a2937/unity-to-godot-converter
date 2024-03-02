using Godot;

public partial class HelloWorld : Node
{
    public override void _EnterTree()
    {
        GD.Print("I'm just been accessed!");
    }

    public override void _Ready()
    {
        GD.Print("I'm ready!");
    }

    public override void _Process(double deltaTime)
    {
        GD.Print("I'm doing work!");
    }
}