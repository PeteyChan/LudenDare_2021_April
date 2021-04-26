using Godot;
using System;

public class ShowScoreInput : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    Godot.TextEdit input;
    Godot.Button submit;

    // Called when the node enters the scene tree for the first time.
    public override void _EnterTree()
    {
        this.FindChild<Label>().Text = Player.Hardcore ? "New Hardcore Top Score " + Player.score.ToString() : "New Top Score " + Player.score.ToString();
        input = this.FindChild<TextEdit>();
        submit = this.FindChild<Button>();
        //input.Text = "";

        if (!Score.canEnterScore(Player.score))
            QueueFree();
    }

    bool test = true;
    public override void _Process(float delta)
    {
        if (input.Text == "" || input.Text.Contains(",") || input.Text.Contains("*"))
        {
            submit.Disabled = true;
        }
        else submit.Disabled = false;
    
        if (test && submit.Pressed)
        {
            test = false;
            Score.Submit(input.Text, Player.score);
            Scene.Load("res://Scenes/Main.tscn");
        }
    }

}
