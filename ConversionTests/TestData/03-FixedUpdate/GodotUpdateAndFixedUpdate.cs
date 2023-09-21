using Godot;
using System.Collections;

public class UpdateAndFixedUpdate : Node
{
    public override void _PhysicsProcess(double delta)
    {
        Debug.Log("FixedUpdate time :" + delta);
    }

    public override void _Process(double delta)
    {
        Debug.Log("Update time :" + delta);
    }
}