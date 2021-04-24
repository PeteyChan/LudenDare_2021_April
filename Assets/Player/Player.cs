using Godot;
using System;

public class Player : RigidBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    StateMachine statemachine = new StateMachine();
    public TypeMap data = new TypeMap();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
        statemachine.Change<Player_States.Move>();

        data.Set(this as RigidBody2D)
            .Set(this.FindChild<AnimationPlayer>())
            .Set(new input())
            .Set(new movespeed{value = 200});
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        statemachine.Update(data, delta);

        var crosshair = data.Get<Crosshair>();
        var body = data.Get<Body>();
        var inputs = data.Get<input>();

        crosshair.Translate(data.Get<input>().Look);

        if (crosshair.Transform.origin.x < 0)
            body.Scale = new Vector2(-1, 1);
        else body.Scale = new Vector2(1, 1);
    }


    public class input
    {
        InputAction up = new InputAction(KeyList.W);
        InputAction down = new InputAction(KeyList.S);
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);

        InputAction look_up = new InputAction(MouseList.up);
        InputAction look_down = new InputAction(MouseList.down);
        InputAction look_left = new InputAction(MouseList.left);
        InputAction look_right = new InputAction(MouseList.right);

        public Vector2 Move => new Vector2(right - left,  down - up);
        public Vector2 Look => new Vector2(look_right.raw - look_left.raw, look_up.raw - look_down.raw) * look_sensitivity;

        public float look_sensitivity = 10f;

    }

    public struct movespeed
    {
        public float value;

        public static implicit operator float(movespeed value)
            => value.value;

        public static implicit operator movespeed(float value)
            => new movespeed{value= value};
    }

}


namespace Player_States
{
    class Move : State
    {
        public override void OnEnter(StateMachine stateMachine, TypeMap data)
        {
            data.Get<AnimationPlayer>().Play("Swim");
        }

        public override void OnUpdate(StateMachine stateMachine, TypeMap data, float delta, float state_time)
        {
            var rb = data.Get<RigidBody2D>();
            var inputs = data.Get<Player.input>();

            rb.LinearVelocity = inputs.Move * data.Get<Player.movespeed>();
        }
    }

}