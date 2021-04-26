using Godot;
using System;
using System.Collections.Generic;

class Corals
{
    public static List<Func<Node2D>> Wall_Prefabs = new List<Func<Node2D>>
    {
        () => new Wall(),
        () => new Wall2(),
        () => new Wall3()
    };

    public static List<Func<Node2D>> Doodad_Prefabs = new List<Func<Node2D>>
    {
        () => new Weeds(),
        () => new Kelp(),
        () => new Branch_coral()
    };

    class Wall : Prefab
    {
        protected override string path => "res://Assets/Coral/Coral_Wall.tscn";
    }

    class Wall2 : Prefab
    {
        protected override string path => "res://Assets/Coral/Coral_Wall_2.tscn";
    }

    class Wall3 : Prefab
    {
        protected override string path => "res://Assets/Coral/Coral_Wall_3.tscn";
    }

    class Weeds : Prefab
    {
        public Weeds()
        {
            this.FindChild<AnimationPlayer>().Play("Sway");
        }
        protected override string path => "res://Assets/Coral/Weeds.tscn";
    }

    class Kelp: Prefab
    {
        public Kelp()
        {
            this.FindChild<AnimationPlayer>().Play("Sway");
        }
        protected override string path => "res://Assets/Coral/Weeds_2.tscn";
    }

    class Branch_coral : Prefab
    {
        protected override string path => "res://Assets/Coral/Branch_coral.tscn";
    }
}

public abstract class Prefab : Node2D
{
    public Prefab()
    {
        AddChild(GD.Load<PackedScene>(path).Instance());
        if (auto_add_to_scene) Scene.Current.AddChild(this);
    }

    protected virtual bool auto_add_to_scene => true;
    protected abstract string path {get;}

}