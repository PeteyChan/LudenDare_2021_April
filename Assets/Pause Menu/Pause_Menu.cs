using Godot;
using System;

public class Pause_Menu : Control
{
    public Pause_Menu()
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/Pause Menu/PauseMenu.tscn").Instance());
    }

    Button Resume, Menu, Desktop;
    InputAction resume = new InputAction(KeyList.Escape);
    Input.MouseMode mode;

    public override void _EnterTree()
    {
        mode = Godot.Input.GetMouseMode();
        Godot.Input.SetMouseMode(Input.MouseMode.Visible);
        var menu = GetChild(0);
        Resume = menu.GetChild(0) as Button;
        Menu = menu.GetChild(1) as Button;
        Desktop = menu.GetChild(2) as Button;
        this.PauseMode = PauseModeEnum.Process;
        GetTree().Paused = true;

        MarginLeft = 0;
        MarginRight = 0;
        MarginBottom = 0;
        MarginTop  = 0;
        AnchorLeft = 0;
        AnchorRight = 1;
        AnchorTop = 0;
        AnchorBottom = 1;
    }

    bool test= true;

    float timer = 0;
    public override void _Process(float delta)
    {
        timer += delta;
        if (timer > .5f && resume)
        {
            QueueFree();
            return;
        }

        if (test)
        {
            if (Resume.Pressed)
            {
                test = false;
                QueueFree();
                return;
            }
            if (Menu.Pressed)
            {
                test = false;
                Scene.Load("res://Scenes/Main.tscn");
                return;
            }
            if (Desktop.Pressed)
            {
                GetTree().Quit();
                return;
            }
        }

    }

    public override void _ExitTree()
    {
        GetTree().Paused = false;
        Godot.Input.SetMouseMode(mode);
    }
}
