using System.Reflection;
using Events;
using _Core;
using Godot;

public class Time :
    IDispatcher<Events.Bootstrap>,
    IDispatcher<Events.FrameUpdate>,
    IDispatcher<Events.FixedUpdate>
{
    static System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    public static System.TimeSpan timespan_since_startup => timer.Elapsed;
    public static float seconds_since_startup => (float)timer.Elapsed.TotalSeconds;
    public static float frame_delta { get; private set; }
    public static float fixed_delta { get; private set; }
    public static float scale
    {
        get => Engine.TimeScale;
        set => Engine.TimeScale = value.min(0);
    }
    
    public static int frame_count { get; private set; }
    public static int fixed_count { get; private set; }

    public static float frames_per_second => Engine.GetFramesPerSecond();
    public static bool isPhysicsStep => Engine.IsInPhysicsFrame();
    public static bool paused
    {
        get => Scene.Tree.Paused; set => Scene.Tree.Paused = value;
    }

    public void OnDispatch(Bootstrap args)
    {
        timer.Start();
    }

    public void OnDispatch(FrameUpdate args)
    {
        frame_count++;
        frame_delta = args.delta_time;
    }

    public void OnDispatch(FixedUpdate args)
    {
        fixed_count++;
        fixed_delta = args.delta_time;

    }
}