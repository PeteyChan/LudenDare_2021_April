using ConsoleCommands;
using Godot;
using System;

public class Shark : Prefab
{
    public Shark()
    {
        this.FindChild<AnimationPlayer>().Play("swim");
    }

    public override string path => "res://Assets/Shark/Shark.tscn";

    public int minX, maxX, distance;

    class SpawnShark : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            var shark = new Shark();
            shark.GlobalPosition = new Vector2(args.ToInt(0), args.ToInt(1)) * 128;
            shark.minX = (int)shark.GlobalPosition.x - 128;
            shark.maxX = (int) shark.GlobalPosition.x + 128;
            shark.distance = 2;
        }
    }

    float timer = 0, swimspeed = 5, prevx = 0;

    public override void _PhysicsProcess(float delta)
    {
        timer += delta;
        var pos = GlobalPosition;

        var x = Mathf.Lerp(minX, maxX, Mathf.Sin(swimspeed * timer / distance)/2 + .5f);
        
        if (prevx > x)
        {
            Scale = new Vector2(1, 1);
        }
        else
        {
            Scale = new Vector2(-1, 1);
        }
        prevx = x;

        GlobalPosition = new Vector2(x, pos.y);
    }
}
