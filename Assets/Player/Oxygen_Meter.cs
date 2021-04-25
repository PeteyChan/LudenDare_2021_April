using Godot;
using System.Collections.Generic;

public class Oxygen_Meter : Node2D
{
    class Oxygen_Symbol : Node2D
    {
        public Oxygen_Symbol()
        {
            this.AddChild(GD.Load<PackedScene>("res://Assets/Player/Oxygen.tscn").Instance());
            complete = Rand.Float01 + .7f;
        }

        public float timer = 0, complete;
    }
    
    Player player;

    public override void _EnterTree()
    {
        player = this.FindParent<Player>();
    }

    List<Oxygen_Symbol> pending = new List<Oxygen_Symbol>();

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
                    var node = new Oxygen_Symbol();
                    pending.Add(node);
                    AddChild(node);
                    node.Position = new Vector2(16f, 16f + 48f * i);
                    node.FindChild<AnimationPlayer>().Play("Spawn");
                }
            }
        }

        for(int i = pending.Count-1; i >= 0; --i)
        {
            var node = pending[i];
            if (Node.IsInstanceValid(node))
            {
                node.timer += delta;
                if (node.timer > node.complete)
                {
                    node.FindChild<AnimationPlayer>().Play("Bubble");
                    pending.RemoveAt(i);
                }
            }
            else pending.RemoveAt(i);
        }
    }
}


public class Oxygen_Pickup : Node2D
{
    public Oxygen_Pickup()
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/OxygenPickup/Oxygen Pickup.tscn").Instance());
        Scene.Current.AddChild(this);
    }
}