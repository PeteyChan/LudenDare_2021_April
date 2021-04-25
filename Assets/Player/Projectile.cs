using Godot;
using System;

public class Projectile : Node2D
{
    void On_Enter_Area(Area2D area)
    {
        if (Node.IsInstanceValid(area))
            area.FindParent<Interfaces.IDamageable>()?.OnDamage();
    }
}
