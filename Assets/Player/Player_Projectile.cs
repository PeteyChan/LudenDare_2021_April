using Godot;
using System;

public class Player_Projectile : Prefab
{
    protected override string path => "res://Assets/Player/Projectile.tscn";
    public Player_Projectile(){}

    public Player_Projectile(Vector2 spawnPosition, Vector2 targetPosition)
    {
        this.Position = spawnPosition;
        this.FindChild<AnimationPlayer>().Play("Spray");

        var distance = targetPosition - spawnPosition;

        var angle = Vector2.Right.AngleTo(distance.Normalized());
        this.Rotation = angle;

        this.FindChild<Area2D>().Connect("area_entered", this, nameof(On_Enter_Area));
    }

    void On_Enter_Area(Area2D area)
    {
        if (area.TryFindParent(out Interfaces.IDamageable damageable))
            damageable.OnDamage();
    }

    float accumulated = 0;


    public override void _Process(float delta)
    {
        if (accumulated > .5f)
            QueueFree();
        else accumulated += delta;
    }
}