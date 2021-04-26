using Godot;
using System;

public class Button2 : Button
{
    public override void _Ready()
    {
        Player.Hardcore =  false;
    }

    bool test = true;
    public override void _Pressed()
    {
        if (Pressed && test)
        {
            test = false;
            Player.Hardcore = true;
            Scene.Load("res://Scenes/Game/Game.tscn");
        }

    }
}
