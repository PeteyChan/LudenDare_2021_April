using ConsoleCommands;
using Godot;
using System;

public class Shark : Prefab
{
    public Shark()
    {
        this.FindChild<AnimationPlayer>().Play("swim");
        this.FindChild<Area2D>().Connect("body_entered", this, nameof(OnEnterBody));
    }

    void OnEnterBody(Node body)
    {
        if (body.TryFindParent(out Player player))
        {
            player.DealDamage(3);
        }
    }

    protected override string path => "res://Assets/Shark/Shark.tscn";

    int minX, maxX, distance;

    float timer = Rand.Float01 * 5f, swimspeed = 5, prevx = 0;

    int last_y = 0;

    public override void _PhysicsProcess(float delta)
    {
        if (last_y != (int)GlobalPosition.y)
        {
            var pos = new int2((int)GlobalPosition.x, (int)GlobalPosition.y) / 128;
            var test = pos;
            while (Game.play_area.Contains(test))
            {
                test.x--;
            }
            minX = test.x;
            test = pos;
            while (Game.play_area.Contains(test))
            {
                test.x++;
            }
            maxX = test.x;
            distance = maxX - minX;
            minX *= 128;
            maxX *= 128;
            last_y = (int)GlobalPosition.y;
        }
        {


            timer += delta;
            var pos = GlobalPosition;

            var x = Mathf.Lerp(minX, maxX, Mathf.Sin(swimspeed * timer / distance) / 2 + .5f);

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

    class SpawnShark : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            var shark = new Shark();
            shark.GlobalPosition = new Vector2(args.ToInt(0), args.ToInt(1)) * 128;
        }
    }
}
