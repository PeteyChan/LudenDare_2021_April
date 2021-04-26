using Godot;
using System;

public class BackToMain : Button
{
    bool test = true;
    public override void _Process(float delta)
    {
        Input.SetMouseMode(Input.MouseMode.Visible);
        if (test && Pressed)
        {
            Scene.Load("res://Scenes/Main.tscn");
            test = false;
        }
    }
}
