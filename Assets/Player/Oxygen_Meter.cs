using Godot;
using System;

public class Oxygen_Meter : Node2D
{
    Node2D GetOxygenPrefab()
    {
        return GD.Load<PackedScene>("res://Assets/Player/Oxygen.tscn").Instance() as Node2D;
    }
    
    Player player;

    public override void _EnterTree()
    {
        player = this.FindParent<Player>();
    }

    public override void _Process(float delta)
    {
        int oxygen = player.data.Get<Player.oxygen>();
        int count = GetChildCount();
        if (oxygen != GetChildCount())
        {
            if(oxygen < count)
            {
                for(int i = count-1; i >= oxygen; -- i)
                    GetChild(i).QueueFree();
            }

            if(oxygen > count)
            {
                for(int i = count; i < oxygen; ++ i)
                {
                    var node = GetOxygenPrefab();
                    AddChild(node);
                    node.Position = new Vector2(16f, 16f + 48f * i);
                    node.FindChild<AnimationPlayer>().Play("Bubble");
                }
            }
        }

    }
}
