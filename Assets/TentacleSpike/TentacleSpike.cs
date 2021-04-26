using ConsoleCommands;
using Godot;
using System.Collections;

public class TentacleSpike : Prefab
{
    protected override string path => "res://Assets/TentacleSpike/TentacleSpike.tscn";

    public TentacleSpike()
    {
        this.FindChild<Area2D>().Connect("body_entered", this, nameof(On_Enter_Area));
        offset().StartCoroutine();
    }

    IEnumerator offset()
    {
        float timer = 0, complete = Rand.Float01 * 5;

        while (timer < complete)
        {
            timer += Time.frame_delta;
            yield return null;
        }
        this.FindChild<AnimationPlayer>().Play("Attack");
    }

    void On_Enter_Area(Node body)
    {
        if (body.TryFindParent(out Player player))
        {
            player.DealDamage(1);
        }
    }

    class SpawnSpike : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            new TentacleSpike().GlobalPosition = new Vector2(args.ToInt(0), args.ToInt(1)) * 128;
        }
    }
}
