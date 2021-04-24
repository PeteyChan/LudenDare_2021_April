using System.Collections;
using System.Collections.Generic;
using Events;


/// <summary>
/// implement on class to respond to console commands
/// ToString() method will return when using help on command
/// </summary>
public interface ICommand
{
    void OnCommand(ConsoleCommands.ConsoleArgs args);
}

namespace ConsoleCommands
{
    class FPS : ICommand, IUpdater
    {
        public void OnCommand(ConsoleArgs args)
        {
            show = !show;
            this.StartUpdates(true);
        }

        bool show;

        public bool Update(float delta)
        {
            Debug.Label("FPS: ", Time.frames_per_second);
            return show;
        }

        public override string ToString() => "Toggles FPS Counter";
    }

    class FrameCount : ICommand, IUpdater
    {
        bool show;
        public void OnCommand(ConsoleArgs args)
        {
            show = !show;
            this.StartUpdates(true);

        }

        public bool Update(float delta)
        {
            Debug.Label("Frame: ", Time.frame_count);
            return show;
        }

        public override string ToString() => "Toggles Frame Count";
    }

    class Quit : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Scene.Tree.Quit();
        }

        public override string ToString() => "Quits Application";
    }

    class Close : Quit { }

    class Exit : Quit { }

    class Maximize : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Godot.OS.WindowMaximized = !Godot.OS.WindowMaximized;
        }
        public override string ToString() => "Maximizes Window";
    }

    class Pause : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Scene.Tree.Paused = true;
        }

        public override string ToString() => "Pauses the Game";
    }


    class UnPause : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Scene.Tree.Paused = false;
        }

        public override string ToString() => "UnPause the Game";
    }

    class Resume : UnPause { };

    class Minimize : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Godot.OS.WindowMinimized = !Godot.OS.WindowMinimized;
        }

        public override string ToString() => "Minimizes the window";
    }

    class Borderless : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Godot.OS.WindowBorderless = !Godot.OS.WindowBorderless;
        }

        public override string ToString() => "Toggles borderless mode";
    }

    class Screen : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            if (value.TryParse(0, out int val))
            {
                if (val < Godot.OS.GetScreenCount() && val >= 0)
                    Godot.OS.CurrentScreen = val;
            }
        }

        public override string ToString() => "Moves window to specified screen";
    }

    class FullScreen : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Godot.OS.WindowFullscreen = !Godot.OS.WindowFullscreen;
        }

        public override string ToString() => "Toggles Fullscreen Mode";
    }

    class Log : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            if (value.original.Length < 5)
                return;
            string val = value.original.Remove(0, 4);
            _Core.Console.Log(val);
        }

        public override string ToString() => "Logs supplied message to console";
    }

    class Clear : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            _Core.Console.Clear();
        }

        public override string ToString() => "Clears Console";
    }

    public class ConsoleArgs
    {
        public string this[int index]
        {
            get
            {
                if (index < 0) return "";
                if (index >= args.Count) return "";
                return args[index];
            }
        }

        public ConsoleArgs(string original, List<string> args)
        {
            this.args = args;
            this.original = original;
        }

        public string original { get; private set; }
        List<string> args;

        public int arguements => args.Count-1;

        public bool TryParse(int arg, out int value)
            => int.TryParse(this[arg], out value);

        public bool TryParse(int arg, out float value)
            => float.TryParse(this[arg], out value);

        public bool TryParse(int arg, out bool value)
            => bool.TryParse(this[arg], out value);

        public bool TryParse(int arg, out string value)
        {
            value = this[arg];
            return value == "";
        }

        public bool ToBool(int arg)
        {
            TryParse(arg, out bool val);
            return val;
        }

        public int ToInt(int arg)
        {
            TryParse(arg, out int val);
            return val;
        }

        public string ToString(int arg)
            => this[arg];

        public float ToFloat(int arg)
        {
            TryParse(arg, out float val);
            return val;
        }
    }

    class Timescale : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            if (args.TryParse(0, out float val))
            {
                Time.scale = val.min(0);
            }
        }

        public override string ToString() => "Changes Games Timescale. 0 is paused, 1 is normal speed. 2 is double speed";
    }

    class ShowTime : ICommand, IUpdater
    {
        public void OnCommand(ConsoleArgs args)
        {
            show = !show;
            this.StartUpdates();
        }

        bool show;

        public bool Update(float delta)
        {
            var time = Godot.OS.GetTime();
            Debug.Label(time["hour"], time["minute"]);
            return show;
        }

        public override string ToString() => "Toggles showing current time";
    }

    class ShowMemory : ICommand, IUpdater
    {
        bool show;

        public void OnCommand(ConsoleArgs args)
        {
            show = !show;
            this.StartUpdates(true);
        }

        public bool Update(float delta)
        {
            var current = (double)System.GC.GetTotalMemory(false);
            string label;
            if (current < 1024)
                label = $"{current: 0} bytes";
            else if ((current /= 1024) < 1024)
                label = $"{current: 0.00} kb";
            else if ((current /= 1024) < 1024)
                label = $"{current: 0.00} mb";
            else
            {
                current /= 1024;
                label = $"{current: 0.00} gb";
            }
            Debug.Label($"Total Memory: {label}");
            return show;
        }

        public override string ToString() => "Toggle showing the current C# memory footprint";
    }

    class ShowDrawCalls : ICommand, IUpdater
    {
        bool show;
        public void OnCommand(ConsoleArgs args)
        {
            show = !show;
            if (show) this.StartUpdates();
        }

        public bool Update(float delta)
        {
            Debug.Label("2D Draw Calls:", Scene.Current.GetViewport().GetRenderInfo(Godot.Viewport.RenderInfo.Info2dDrawCallsInFrame));
            Debug.Label("3D DrawCalls:", Scene.Current.GetViewport().GetRenderInfo(Godot.Viewport.RenderInfo.DrawCallsInFrame));
            return show;
        }

        public override string ToString() => "Toggle Drawcall counts";
    }
}

