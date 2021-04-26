using Events;
using Godot;
using System;

public class BGM : IDispatcher<Events.Bootstrap>, IDispatcher<Events.FrameUpdate>
{
    AudioStreamPlayer player2D;

    public void OnDispatch(FrameUpdate args)
    {
        if (player2D.Playing == false)
            player2D.Play();
    }

    public void OnDispatch(Bootstrap args)
    {
        player2D = GD.Load<PackedScene>("res://Assets/BGM/BGM.tscn").Instance() as AudioStreamPlayer;
        args.AddChild(player2D);
    }
}
