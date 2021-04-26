using Godot;
using System;

public class Button3 : Button
{
    public override void _Process(float delta)
    {
        if (Pressed)
            GetTree().Quit();
    }
}
