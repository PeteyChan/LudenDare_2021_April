using Godot;
using System.Collections;
using System.Collections.Generic;

public class Oxygen_Meter : Node2D
{
    class Oxygen_Symbol : Prefab
    {
        public Oxygen_Symbol()
        {
            OnSpawn().StartCoroutine();
        }

        protected override string path => "res://Assets/Player/Oxygen.tscn";
        protected override bool auto_add_to_scene => false;

        float timer = 0, complete = Rand.Float01 + .7f;
        
        IEnumerator OnSpawn()
        {
            this.FindChild<AnimationPlayer>().Play("Spawn");
            while (timer < complete)
            {                
                timer += Time.frame_delta;
                yield return null;
            }
            
            if (Node.IsInstanceValid(this))
            {
                this.FindChild<AnimationPlayer>().Play("Bubble");
            }
        }
    }
    
    Player player;

    public override void _EnterTree()
    {
        player = this.FindParent<Player>();
    }

    public override void _Process(float delta)
    {
        int player_oxygen = player.data.Get<Player.oxygen>();
        int count = GetChildCount();
        if (player_oxygen != GetChildCount())
        {
            if(player_oxygen < count)
            {
                for(int i = count-1; i >= 0; -- i)
                    GetChild(i).QueueFree();
            }

            if(player_oxygen > count)
            {
                for(int i = count; i < player_oxygen; ++ i)
                {
                    var oxygen_symbol = new Oxygen_Symbol();
                    this.AddChild(oxygen_symbol);
                    oxygen_symbol.Position = new Vector2(16f, 16f + 48f * i);
                }
            }
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