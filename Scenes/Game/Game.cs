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

class Corpse : Node2D, Interfaces.IDamageable
{
    public Corpse()
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/Corpse/Corpse.tscn").Instance());
        Scene.Current.AddChild(this);
    }

    bool damagable = true;
    public void OnDamage()
    {
        if (damagable)
        {
            this.FindChild<AnimationPlayer>().Play("Death");
            damagable = false;

            var count = Rand.Int(3) + 2;
            for(int i = 0; i < count; ++ i)
            {
                var gpos = GlobalPosition;
                gpos.x -= 32;
                var pos = new Vector2(Rand.Float01 * 64, Rand.Float01*32) +gpos;
                new Oxygen_Pickup().GlobalPosition = pos;
            }
        }
    }
}

public class Game : Node2D
{
    static int block_width = 128;
    // Called when the node enters the scene tree for the first time.

    static Player player;

    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(Colors.Black);
        HashSet<int2> play_area = new HashSet<int2>();
        HashSet<int2> rocks = new HashSet<int2>();
        int2 start = new int2();
        
        play_area.Add(start);
        rocks.Add(start);
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
                    start.y += 1;
                    break;
            }

            if (!play_area.Contains(start))
            {
                play_area.Add(start);
                rocks.Add(start);
            }

            if (start.y == 100)
            {
                loop++;
                start = new int2();
            }
        }

        int minX = 0, maxX = 0;
        foreach (var position in play_area)
        {
            for (int x = position.x - 5; x < position.x + 6; ++x)
                for(int y = position.y -1; y < position.y + 2; ++ y)
            {
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;

                if (rocks.Contains(new int2(x, y))) continue;
                new Rock().GlobalPosition = new Vector2(x, y) * block_width;
                rocks.Add(new int2(x, y));
            }
        }

        List<int2> caves = new List<int2>(), sides = new List<int2>(), rooms = new List<int2>(), grounded = new List<int2>();
        foreach(var position in play_area)
        {
            int obstruction_count = 0;
            int cave_count = 0;
            int side_count  = 0;
            bool is_grounded = false;

            if (position.y < 5)
                continue;

            for(int x = position.x -1; x < position.x + 2; ++ x)
            {
                for (int y = position.y-1; y < position.y + 2; ++ y)
                {
                    bool obstructed = !play_area.Contains(new int2(x, y));
                    if (obstructed)
                    {
                        obstruction_count ++;
                        if (y == position.y-1 && x == position.x)
                            is_grounded = true;    
                        if (y == position.y || x == position.x)
                        {
                            cave_count ++;
                        }
                        if (y == position.x)
                            side_count ++;
                    }
                }
            }

            if (is_grounded) grounded.Add(position);
            if (obstruction_count == 0) rooms.Add(position);
            if (side_count == 0) sides.Add(position);
            if (cave_count == 3 && is_grounded) caves.Add(position);
        }


        spawn_corpses(caves);
        spawn_edge_boundaries(minX, maxX);        
        spawn_player();
    }

    void spawn_player()
    {
        if (!Node.IsInstanceValid(player))
            player = Player.Spawn(new Vector2());
        else
        {
            this.AddChild(player);
            player.Position = new Vector2();
            player.data.Get<Player.depth>() += 100;
        }
    }

    void spawn_edge_boundaries(int minX, int maxX)
    {
        for(int x = minX; x < maxX; ++ x)
        {
            new Depths().GlobalPosition = new Vector2(x, 100)*block_width;
            var depth = new Depths();
            depth.GlobalPosition = new Vector2(x, 0)*block_width;
            depth.Rotation = Mathf.Pi;
        }
    }

    void spawn_corpses(List<int2> caves)
    {
        int coprses = caves.Count.max(Rand.Int(10, 20))/2;
        HashSet<int2> spawned = new HashSet<int2>();
        for(int i = 0; i < coprses; ++ i)
        {
            var pos = caves[Rand.Int(caves.Count)];            
            if (spawned.Contains(pos))
                i--;
            else
            {
                new Corpse().GlobalPosition = pos*block_width;
            } 
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
