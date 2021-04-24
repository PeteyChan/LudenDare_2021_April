using Godot;
using System;
using System.Collections.Generic;
using ConsoleCommands;
using Events;

namespace _Core
{
    [DispatchOrder(int.MinValue)]
    class Console : IDispatcher<Events.FrameUpdate>
    {
        static Console_GUI node;
        InputAction toggle_console = new InputAction(KeyList.Quoteleft);

        public void OnDispatch(FrameUpdate args)
        {
            if (toggle_console.on_pressed)
            {
                if (!Node.IsInstanceValid(node))
                {
                    node = new Console_GUI();
                    Scene.Tree.CurrentScene.AddChild(node);
                    node.PauseMode = Node.PauseModeEnum.Process;
                    update_log = true;
                }
                else
                {
                    node.QueueFree();
                    node = null;
                }
            }
        }

        public static void Clear()
        {
            log.Clear();
            update_log = true;
        }

        static bool update_log;
        static List<string> log = new List<string>();
        public static void Log(string message)
        {
            log.Insert(0, message);
            if (log.Count > 10000)
                log.RemoveAt(log.Count - 1);
            update_log = true;
        }

        class Console_GUI : CanvasLayer
        {
            InputAction previous_command = new InputAction(KeyList.Up);
            InputAction next_command = new InputAction(KeyList.Down);
            InputAction execute_command = new InputAction(KeyList.Enter, KeyList.KpEnter);

            static List<string> previousCommands = new List<string>();
            static Dictionary<string, ICommand> Console_Commands = new Dictionary<string, ICommand>();
            static Console_GUI()
            {
                foreach (var type in typeof(Console_GUI).Assembly.GetTypes())
                {
                    if (typeof(ICommand).IsAssignableFrom(type) && Singleton.TryGet(type, out var singleton))
                    {
                        Console_Commands.Add(type.Name.ToLower(), singleton as ICommand);
                    }
                }
            }

            LineEdit edit = new LineEdit();
            Label console_log = new Label();
            Panel panel = new Panel();
            VScrollBar scroll = new VScrollBar();

            int command_index = -1;
            public override void _Process(float delta)
            {
                ExecuteConsoleInput();
                CyclePreviousCommands();
                DrawConsoleLog();
            }

            void CyclePreviousCommands()
            {
                if (previousCommands.Count > 0)
                {
                    if (next_command.on_pressed)
                    {
                        command_index--;
                        if (command_index < 0)
                            command_index = previousCommands.Count - 1;
                        edit.Text = previousCommands[command_index];
                    }

                    if (previous_command.on_pressed)
                    {
                        command_index = (command_index + 1) % previousCommands.Count;
                        command_index = command_index.min(0);
                        edit.Text = previousCommands[command_index];
                    }
                }
            }

            void ExecuteConsoleInput()
            {
                if (execute_command.on_pressed && !string.IsNullOrEmpty(edit.Text))
                {
                    command_index = -1;
                    previousCommands.Insert(0, edit.Text);
                    if (previousCommands.Count > 50)
                        previousCommands.RemoveAt(previousCommands.Count - 1);
                    List<string> items = new List<string>(edit.Text.ToLower().Split(' '));
                    items.Add("");
                    string command = "";
                    for (int i = 0; i < items.Count - 1; ++i)
                    {
                        command += items[i];
                        if (Console_Commands.TryGetValue(command, out var execute))
                        {
                            execute.OnCommand(new ConsoleArgs(edit.Text, items.GetRange(i + 1, items.Count - (i + 1))));
                            edit.Text = "";
                            return;
                        }
                    }
                    edit.Text = "";
                }
            }

            public override void _EnterTree()
            {
                AddChildNodes();
                SetupPanels(20f);
                CallDeferred(nameof(GrabFocus));
                mode = Input.GetMouseMode();
                Input.SetMouseMode(Input.MouseMode.Visible);
            }

            Input.MouseMode mode;
            public override void _ExitTree()
            {
                Input.SetMouseMode(mode);
            }

            void AddChildNodes()
            {
                this.AddChild(edit);
                this.AddChild(scroll);
                this.AddChild(panel);
                panel.AddChild(console_log);
            }

            void SetupPanels(float TextSize)
            {
                edit.AnchorRight = .3f;
                edit.MarginBottom = TextSize;

                scroll.MarginRight = 12;
                scroll.AnchorBottom = 1;
                scroll.MarginTop = TextSize + 4;

                panel.MarginTop = TextSize + 5;
                panel.MarginBottom = -1;
                panel.MarginLeft = scroll.MarginRight;
                panel.AnchorBottom = 1;
                panel.AnchorRight = .3f;
                panel.Modulate = new Color(1, 1, 1, .9f);

                console_log.AnchorRight = 1;
                console_log.AnchorBottom = 1;
                console_log.MarginLeft = 2;
                console_log.MarginRight = -2;
                console_log.Autowrap = true;
            }

            int last_scroll_index;
            static System.Text.StringBuilder builder = new System.Text.StringBuilder();
            void DrawConsoleLog()
            {
                int index = ((int)scroll.Value).clamp(0, log.Count - 1);
                if (last_scroll_index != index)
                    update_log = true;

                scroll.MaxValue = log.Count.min(1);

                if (update_log)
                {
                    int maxLines = (int)(panel.RectSize.y / (edit.RectSize.y - 7));

                    for (int i = index; i < log.Count && i < index + maxLines; ++i)
                    {
                        builder.Append(log[i]);
                        builder.Append("\n");
                    }
                    console_log.Text = builder.ToString();
                    builder.Clear();
                }
                last_scroll_index = index;
                update_log = false;
            }

            void GrabFocus()
            {
                edit.GrabFocus();
                edit.Text = "";
            }

            class Help : ICommand
            {
                public void OnCommand(ConsoleArgs value)
                {
                    if (value.arguements == 0)
                    {
                        foreach (var item in Console_Commands)
                        {
                            Console.Log(item.Value.GetType().Name);
                        }
                        return;
                    }

                    var command = "";
                    for (int i = 0; i < value.arguements; ++i)
                    {
                        command += value[i];
                        if (Console_Commands.TryGetValue(command, out var execute))
                        {
                            Debug.Log(execute.ToString());
                            return;
                        }
                    }
                }

                public override string ToString() => "Returns command's ToString() function";
            }

            class Commands : ICommand
            {
                public void OnCommand(ConsoleArgs args)
                {
                    foreach (var item in Console_Commands)
                    {
                        Console.Log(item.Value.GetType().Name);
                    }
                }

                public override string ToString() => "Lists all commands";
            }

            class Filter : ICommand
            {
                public void OnCommand(ConsoleArgs args)
                {
                    for (int i = log.Count - 1; i >= 0; --i)
                    {
                        var text = log[i].ToLower();

                        for (int index = 0; index < args.arguements - 1; ++index)
                        {
                            if (text.Contains(args.ToString(index)))
                                goto Continue;
                        }
                        log.RemoveAt(i);
                        update_log = true;

                    Continue:
                        continue;
                    }
                }

                public override string ToString() => "Filters command console to show only entries containing supplied input";
            }
        }
    }
}