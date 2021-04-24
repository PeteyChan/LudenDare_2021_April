using Godot;
using System.Collections.Generic;
using Events;

public enum MouseList : byte
{
    left_click = 1,
    right_click = 2,
    middle_click = 3,
    wheel_up = 4,
    wheel_down = 5,
    right,
    up,
    left,
    down = 0,
}

public enum GamepadList : byte
{
    cross = 0,
    circle = 1,
    square = 2,
    triangle = 3,

    left_shoulder = 4,
    right_shoulder = 5,
    left_trigger = 6,
    right_trigger = 7,
    left_hat = 8,
    right_hat = 9,

    select = 10,
    start = 11,

    dpad_up = 12,
    dpad_down = 13,
    dpad_left = 14,
    dpad_right = 15,

    home = 16,

    leftstick_up,
    leftstick_down,
    leftstick_left,
    leftstick_right,

    rightstick_up,
    rightstick_down,
    rightstick_left,
    rightstick_right,
}

[DispatchOrder(int.MinValue)]
class InputEventManager :
    IDispatcher<Godot.InputEvent>,
    IDispatcher<Events.FrameUpdate>
{
    static List<Gamepad> gamepads = new List<Gamepad>() { new Gamepad(), new Gamepad() };
    static (float value, int frame)[] mouse_buttons = new (float, int)[Enum<MouseList>.Count];

    public static float GetKey(KeyList key) => Input.IsKeyPressed((int)key) ? 1 : 0;
    public static float GetGamepad(GamepadList gamepad, int device = 0)
    {
        float val;
        switch (gamepad)
        {
            case GamepadList.leftstick_left:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogLx);
                return val >= 0 ? 0 : -val;
            case GamepadList.leftstick_right:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogLx);
                return val <= 0 ? 0 : val;

            case GamepadList.leftstick_down:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogLy);
                return val <= 0 ? 0 : val;
            case GamepadList.leftstick_up:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogLy);
                return val >= 0 ? 0 : -val;
            
            case GamepadList.rightstick_left:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogRx);
                return val >= 0 ? 0 : -val;
            case GamepadList.rightstick_right:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogRx);
                return val <= 0 ? 0 : val;
            
            case GamepadList.rightstick_down:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogRy);
                return val <= 0 ? 0 : val;
            case GamepadList.rightstick_up:
                val = Input.GetJoyAxis(device, (int)JoystickList.AnalogRy);
                return val >= 0 ? 0 : -val;

            case GamepadList.left_trigger:
                return Input.GetJoyAxis(device, (int)JoystickList.AnalogL2);
            case GamepadList.right_trigger:
                return Input.GetJoyAxis(device, (int)JoystickList.AnalogR2);

            default:
                return Input.IsJoyButtonPressed(device, (int)gamepad) ? 1: 0;
        }
    }

    public static float GetMouse(MouseList mouse) => mouse_buttons[(int)mouse].value;

    public void OnDispatch(InputEvent args)
    {
        if (args is InputEventMouseButton mouseButton)
        {
            var frame = Time.frame_count;
            switch ((MouseList)mouseButton.ButtonIndex)
            {
                case MouseList.wheel_down:
                case MouseList.wheel_up:
                    mouse_buttons[mouseButton.ButtonIndex] = (1, frame);
                    break;
                default:
                    mouse_buttons[mouseButton.ButtonIndex] = mouseButton.Pressed ? (1, 0) : (0, 0);
                    break;
            }
        }
        else if (args is InputEventMouseMotion mouseMotion)
        {
            int frame = Time.frame_count;
            var x = mouseMotion.Relative.x * .1f;
            var y = mouseMotion.Relative.y * .1f;

            if (x > 0) mouse_buttons[(int)MouseList.right] = (x, frame);
            if (x < 0) mouse_buttons[(int)MouseList.left] = (-x, frame);
            if (y > 0) mouse_buttons[(int)MouseList.up] = (y, frame);
            if (y < 0) mouse_buttons[(int)MouseList.down] = (-y, frame);
        }
    }

    static Gamepad GetDevice(int device)
    {
        while (device + 1 >= gamepads.Count)
            gamepads.Add(new Gamepad());
        return gamepads[device + 1];
    }

    public void OnDispatch(FrameUpdate args)
    {
        int frame = Time.frame_count - 1;
        for (int i = 0; i < mouse_buttons.Length; ++i)
        {
            if (mouse_buttons[i].frame == frame)
            {
                mouse_buttons[i] = (0, 0);
            }
        }
    }

    class Gamepad
    {
        public float[] gamepad = new float[Enum<GamepadList>.Count];
    }
}

public class InputAction
{
    public InputAction(params IBinding[] default_bindings)
    {
        defaults.AddRange(default_bindings);
        ResetToDefaults();
    }

    public InputAction(params object[] default_bindings)
    {
        foreach (var item in default_bindings)
        {
            if (item is KeyList key)
                defaults.Add(new KeyboardBinding(key));
            if (item is MouseList mouse)
                defaults.Add(new MouseBinding(mouse));
            if (item is GamepadList pad)
                defaults.Add(new GamepadBinding(pad));
        }
        ResetToDefaults();
    }

    List<IBinding> defaults = new List<IBinding>();
    public List<IBinding> bindings = new List<IBinding>();
    public float value => rawValue < deadzone ? 0 : rawValue.max(1f);
    public float raw => rawValue < deadzone ? 0 : rawValue;

    float lastValue;
    float currentValue;

    public float rawValue
    {
        get
        {
            if (last_update != Time.frame_count)
            {
                lastValue = currentValue;
                last_update = Time.frame_count;
                currentValue = 0;
                foreach (var binding in bindings)
                    currentValue += binding.value;
            }
            return currentValue;
        }
    }

    public float deadzone = .2f;
    public bool pressed => rawValue > deadzone;
    public bool on_pressed => rawValue > deadzone && lastValue < deadzone;
    public bool on_released => rawValue < deadzone && lastValue > deadzone;
    int last_update;

    public void ResetToDefaults()
    {
        bindings.Clear();
        bindings.AddRange(defaults);
    }

    public InputAction AddBinding(KeyList key)
    {
        bindings.Add(new KeyboardBinding(key));
        return this;
    }

    public InputAction AddBinding(MouseList mouse)
    {
        bindings.Add(new MouseBinding(mouse));
        return this;
    }

    public InputAction AddBinding(GamepadList game, int device = 0)
    {
        bindings.Add(new GamepadBinding(game, device));
        return this;
    }

    public static implicit operator float(InputAction action) =>
        action == null ? 0 : action.value;

    public static implicit operator bool (InputAction action) =>
        action == null ? false : action.on_pressed;

    public interface IBinding
    {
        float value { get; }
    }

    public class KeyboardBinding : IBinding
    {
        public KeyboardBinding(KeyList key)
        {
            this.key = key;
        }

        public int device;
        public KeyList key;
        public float value => InputEventManager.GetKey(key);

        public override string ToString() => $"Key {key}";
    }

    public class MouseBinding : IBinding
    {
        public MouseBinding(MouseList key)
        {
            this.key = key;
        }

        public int device;
        public MouseList key;
        public float value => InputEventManager.GetMouse(key);
        public override string ToString() => $"Mouse {key}";
    }

    public class GamepadBinding : IBinding
    {
        public GamepadBinding(GamepadList key, int device = 0)
        {
            this.device = device;
            this.key = key;
        }

        public int device;
        public GamepadList key;
        public float value => InputEventManager.GetGamepad(key, device);

        public override string ToString() => $"{device} : Joy {key}";
    }
}