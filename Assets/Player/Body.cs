using Godot;
using System;

public class Body : Node2D
{    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.FindParent<Player>().data.Set(this);
    }
}
