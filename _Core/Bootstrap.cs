using Godot;

namespace Events
{
    public class Bootstrap : Node
    {
        public override void _EnterTree()
        {
            PauseMode = PauseModeEnum.Process;
            DispatchManager.Dispatch(this);
        }

        public override void _Process(float delta)
        {
            DispatchManager.Dispatch(new Debug());
            Profiler.Stop("Update");
            Profiler.Start("Update");
            DispatchManager.Dispatch(new Events.FrameUpdate(){delta_time = delta});
            DispatchManager.Dispatch(new Events.LateFrameUpdate(){delta_time = delta});
        }

        public override void _PhysicsProcess(float delta)
        {
            Profiler.Stop("Physics");
            Profiler.Start("Physics");
            DispatchManager.Dispatch(new Events.FixedUpdate(){delta_time = delta});
            DispatchManager.Dispatch(new Events.LateFixedUpdate(){delta_time = delta});
        }

        public override void _Input(InputEvent @event)
        {
            DispatchManager.Dispatch(@event);
        }
    }

    public struct FrameUpdate
    {
        public bool paused => Time.paused;
        public float delta_time;
    }

    public struct LateFrameUpdate
    {
        public bool paused => Time.paused;
        public float delta_time;
    }

    public struct FixedUpdate
    {
        public bool paused => Time.paused;
        public float delta_time;
    }

    public struct LateFixedUpdate
    {
        public bool paused => Time.paused;
        public float delta_time;
    }
}

