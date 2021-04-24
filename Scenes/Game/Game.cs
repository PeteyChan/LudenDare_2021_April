using Godot;
using System;
using System.Collections.Generic;

class Rock : Node2D
{
    public Rock()
    {
        Scene.Current.AddChild(this);
        this.AddChild(GD.Load<PackedScene>("res://Assets/Rocks/Rock.tscn").Instance());
    }
}

public class Game : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    static float block_width = 128f;
    // Called when the node enters the scene tree for the first time.


    public override void _Ready()
    {
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

        foreach (var position in positions)
        {
            for (int x = position.x - 5; x < position.x + 6; ++x)
                for(int y = -1; y < 2; ++ y)
            {
                var ypos = position.y + y;
                if (blocked.Contains(new int2(x, ypos))) continue;
                new Rock().GlobalPosition = new Vector2(x, -ypos) * block_width;
                blocked.Add(new int2(x, ypos));
            }
        }

        Player.Spawn(new Vector2());
    }
}
