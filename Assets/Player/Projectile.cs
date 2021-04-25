using Godot;
using System;

public class Projectile : Area2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    void On_Enter_Area(Area2D area)
    {
        area.FindParent<Interfaces.IDamageable>()?.OnDamage();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
