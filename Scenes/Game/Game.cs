using Godot;
using System;
using System.Collections.Generic;

class Rock : Node2D
{
    public Rock()
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/Rocks/Rock.tscn").Instance());
        Scene.Current.AddChild(this);
    }
}

class Depths : Node2D
{
    public Depths()
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/Depths/Depths.tscn").Instance());
        Scene.Current.AddChild(this);
    }
}

public class Game : Node2D
{
    static float block_width = 128f;
    // Called when the node enters the scene tree for the first time.

    static Player player;

    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(Colors.Black);
        HashSet<int2> positions = new HashSet<int2>();
        HashSet<int2> blocked = new HashSet<int2>();
        int2 start = new int2();
        
        positions.Add(start);
        blocked.Add(start);
        int loop = 0;

        while (loop < 4)// && loop < 100)
        {
            //loop++;
            switch (Rand.Int(3))
            {
                case 0:
                    start.x += 1;
                    break;
                case 1:
                    start.x -= 1;
                    break;
                default:
                    start.y -= 1;
                    break;
            }

            if (!positions.Contains(start))
            {
                positions.Add(start);
                blocked.Add(start);
            }

            if (start.y == -100)
            {
                loop++;
                start = new int2();
            }
        }

        int minX = 0, maxX = 0;
        foreach (var position in positions)
        {
            for (int x = position.x - 5; x < position.x + 6; ++x)
                for(int y = -1; y < 2; ++ y)
            {
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;
                var ypos = position.y + y;
                if (blocked.Contains(new int2(x, ypos))) continue;
                new Rock().GlobalPosition = new Vector2(x, -ypos) * block_width;
                blocked.Add(new int2(x, ypos));
            }
        }

        for(int x = minX; x < maxX; ++ x)
        {
            new Depths().GlobalPosition = new Vector2(x, 100)*block_width;
            var depth = new Depths();
            depth.GlobalPosition = new Vector2(x, 0)*block_width;
            depth.Rotation = Mathf.Pi;
        }

        if (!Node.IsInstanceValid(player))
            player = Player.Spawn(new Vector2());
        else
        {
            this.AddChild(player);
            player.Position = new Vector2();
            player.data.Get<Player.depth>() += 100;
        }
    }

    public override void _Process(float delta)
    {
        if (player.Position.y > 12800)
        {
            this.RemoveChild(player);
            Scene.Load("res://Scenes/Game/Game.tscn");
        }
    }
}
