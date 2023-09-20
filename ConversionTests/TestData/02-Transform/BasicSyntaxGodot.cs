using Godot;
using System;

public class BasicSyntax : Node
{
    public override void _Ready()
    {
        GD.Print(Transform.Origin.X);
    }
}