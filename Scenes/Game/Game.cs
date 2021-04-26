using ConsoleCommands;
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
            damagable = false;
            this.FindChild<AnimationPlayer>().Play("Death");

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

    class SpawnCorpse : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            new Corpse().GlobalPosition = new Vector2(args.ToInt(0), args.ToInt(1)) * 128;
        }
    }
}

public class Game : Node2D
{
    static int block_width = 128;
    // Called when the node enters the scene tree for the first time.

    static Player player;

    public static HashSet<int2> play_area = new HashSet<int2>();

    public static List<Func<Node2D>> wall_prefabs = new List<Func<Node2D>>
    {
        () => new Corals.Wall()
    };

    public static List<Func<Node2D>> doodad_prefabs = new List<Func<Node2D>>
    {
        () => new Corals.Weeds()
    };

    public override void _Ready()
    {
        play_area.Clear();
        VisualServer.SetDefaultClearColor(Colors.Black);
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
                rocks.Add(new int2(x, y));
                if (y > 100)
                    continue;
                wall_prefabs.GetRandom()().GlobalPosition = new Vector2(x, y) * block_width;
            }
        }

        List<int2> caves = new List<int2>(), sides = new List<int2>(), open_areas = new List<int2>(), grounded = new List<int2>();
        foreach(var position in play_area)
        {
            int obstruction_count = 0;
            int cave_count = 0;
            int side_count  = 0;
            bool is_grounded = false;

            for(int x = position.x -1; x < position.x + 2; ++ x)
            {
                for (int y = position.y-1; y < position.y + 2; ++ y)
                {
                    if (y >= 100) continue;
                    bool obstructed = !play_area.Contains(new int2(x, y));
                    if (obstructed)
                    {
                        obstruction_count ++;
                        if (y == position.y+1 && x == position.x)
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

            if (position.y < 5) continue;
            if (obstruction_count == 0) open_areas.Add(position);
            if (side_count == 0) sides.Add(position);
            if (cave_count == 3 && is_grounded) caves.Add(position);
        }

        spawn_sharks(open_areas, play_area);
        spawn_corpses(caves, play_area);
        spawn_edge_boundaries(minX, maxX);
        spawn_weeds(grounded);
    }

    void spawn_sharks(List<int2> open, HashSet<int2> play_area)
    {
        int sharks = open.Count.max(Rand.Int(10, 16))/2 + (Node.IsInstanceValid(player) ? player.data.Get<Player.depth>()/25 : 0);

        HashSet<int2> spawned = new HashSet<int2>();

        for (int i = 0;i < sharks; ++ i)
        {
            var pos = open[Rand.Int(open.Count)];
            int2 test = pos;
            while(play_area.Contains(test))
            {
                test.x --;
                if (spawned.Contains(test))
                    goto skip;
                spawned.Add(test);
            }
            test = pos;
            while(play_area.Contains(test))
            {
                test.x ++;
                if (spawned.Contains(test))
                    goto skip;
                spawned.Add(test);
            }
            new Shark().GlobalPosition = pos * block_width;
            skip:
                continue;
        }                
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
        player.FindChild<Camera2D>().Current = true;
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

    void spawn_corpses(List<int2> caves, HashSet<int2> play_area)
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
                var corpse = new Corpse();
                corpse.GlobalPosition = pos*block_width;
                spawned.Add(pos);
                if (play_area.Contains(new int2(pos.x - 1, pos.y)))
                    corpse.Scale = new Vector2(-1, 1);
            } 
        }
    }

    void spawn_weeds(List<int2> grounded)
    {
        for(int i = 0; i < grounded.Count; ++ i)
        {
            if (Rand.Int(2) == 0)
                doodad_prefabs.GetRandom()().GlobalPosition = grounded[i].vector2 * block_width;
        }
    }

    float timer = 0;
    bool spawned = false;
    public override void _Process(float delta)
    {
            timer += delta;
        if (!spawned && timer > .2f)
        {
            spawn_player();
            spawned = true;
        }
        if (spawned && player.Position.y > 12800)
        {
            this.RemoveChild(player);
            Scene.Load("res://Scenes/Game/Game.tscn");
        }
    }
}
