using Godot;
using System;

public class DisplayScores : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var table = Player.Hardcore ? Score.top_score_hardcore : Score.top_score_standard;

        var children = this.GetChildren();
        for(int i = 0;i <  children.Count; ++ i)
        {
            var label = children[i] as Label;
            
            if (i >= table.Count)
                label.Text = "";
            else
                label.Text = table[i].score + "    " + table[i].name;
            label.AnchorLeft = 0;
            label.AnchorRight = 1;
            label.AnchorBottom = 0;
            label.AnchorTop = .1f * i;
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
