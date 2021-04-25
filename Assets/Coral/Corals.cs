using Godot;

namespace Corals
{
    class Wall : Prefab
    {
        public override string path => "res://Assets/Coral/Coral_Wall.tscn";
    }


    class Weeds : Prefab
    {
        public override void _Ready()
        {
            this.FindChild<AnimationPlayer>().Play("Sway");
        }

        public override string path => "res://Assets/Coral/Weeds.tscn";
    }
}

public abstract class Prefab : Node2D
{
    public Prefab()
    {
        AddChild(GD.Load<PackedScene>(path).Instance());
        Scene.Current.AddChild(this);
    }

    public abstract string path {get;}

}