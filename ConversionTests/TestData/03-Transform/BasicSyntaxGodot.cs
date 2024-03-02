using Godot;
using System;

public partial class BasicSyntax : Node
{
    public override void _Ready()
    {
        GD.Print(Transform.Origin.X);
        GD.Print(Transform.Origin.Y);
    }
}