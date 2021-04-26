using ConsoleCommands;
using Godot;
using System;

public class Pufferfish : Prefab, Interfaces.IDamageable
{
    protected override string path => "res://Assets/PufferFish/PufferFish.tscn";

    public Pufferfish()
    {
        this.FindChild<Area2D>().Connect("body_entered", this, nameof(OnEnterBody));
        this.FindChild<AnimationPlayer>().Play("Swim");
    }

    void OnEnterBody(Node body)
    {
        if (body.TryFindParent(out Player player))
        {
            player.DealDamage(1);
            this.FindChild<AnimationPlayer>().Play("Attack");
            state = puffer_state.attack;
            timer = 0;
        }
    }


    enum puffer_state
    {
        swim,
        attack,
        dead
    }

    puffer_state state = puffer_state.swim;

    float prevX, timer = 0;
    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case puffer_state.attack:
                {
                    timer += delta;
                    if (timer > .7f)
                    {
                        state = puffer_state.swim;
                        this.FindChild<AnimationPlayer>().Play("Swim");
                    }
                }
                break;
            case puffer_state.swim:
                {
                    if (Node.IsInstanceValid(Player.instance))
                    {
                        var pos = GlobalPosition;
                        if (prevX > pos.x)
                            Scale = new Vector2(1, 1);
                        else Scale = new Vector2(-1, 1);

                        prevX = pos.x;

                        var targetPos = Player.instance.GlobalPosition;
                        if (pos.DistanceTo(targetPos) < (128 * 5))
                        {
                            targetPos = (targetPos - pos).Normalized() * 32f * delta + pos;

                            var tile = new int2(Mathf.RoundToInt(targetPos.x / 128), Mathf.RoundToInt(targetPos.y / 128));
                            
                            if (Game.play_area.Contains(tile))
                                GlobalPosition = targetPos;
                        }
                    }

                }
                break;

            default:
                break;
        }
    }

    public void OnDamage()
    {
        if (state == puffer_state.dead)
            return;

        if (1f/((float)Player.instance.data.Get<Player.depth>().value.min(100)/100f) > Rand.Float01)
        {
            new Oxygen_Pickup().GlobalPosition = GlobalPosition;
        }


        var area = this.FindChild<Area2D>();
        area.Monitoring = false;
        area.Monitorable = false;
        this.FindChild<AnimationPlayer>().Play("Death");
        state = puffer_state.dead;
    }

    class SpawnPuffer : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            new Pufferfish().GlobalPosition = new Vector2(args.ToInt(0), args.ToInt(1)) * 128;
        }
    }
}
