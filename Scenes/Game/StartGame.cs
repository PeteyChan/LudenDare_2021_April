using Godot;
using System;

public class StartGame : Button
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    bool test = true;
    
    InputAction start = new InputAction(KeyList.Enter);

    public override void _Process(float delta)
    {
        if (test)
            Input.SetMouseMode(Input.MouseMode.Confined);
        if (test && (Pressed || start))
        {
            test = false;
            Scene.Load("res://Scenes/Game/Game.tscn");
        }

    }
}
