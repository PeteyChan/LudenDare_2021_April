using ConsoleCommands;
using Godot;
using System;

public class Player : RigidBody2D
{
    class sethealth : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            if (instance != null)
                instance.data.Get<oxygen>() = args.ToInt(0);
        }
    }

    public static Player instance {get; private set;}

    public static Player Spawn(Vector2 position)
    {
        var player = GD.Load<PackedScene>("res://Assets/Player/Player.tscn").Instance() as Player;
        Scene.Current.AddChild(player);
        player.GlobalPosition = position;
        return player;        
    }

    StateMachine statemachine = new StateMachine();
    public TypeMap data = new TypeMap();

    public override void _EnterTree()
    {
        instance = this;
        Input.SetMouseMode(Input.MouseMode.Captured);
        statemachine.Change<Player_States.Move>();

        data.Set(this as RigidBody2D)
            .Set(this.FindChild<AnimationPlayer>())
            .Set(new input())
            .Set(new movespeed{value = 200})
            .Set(this.FindChild<GunBarrel>())
            .Set(this.FindChild<Arm>())
            .Set(new oxygen{value = 5})
            ;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Debug.Label("player oxygen", data.Get<oxygen>().value);
        statemachine.Update(data, delta);

        var crosshair = data.Get<Crosshair>();
        var body = data.Get<Body>();
        var inputs = data.Get<input>();

        crosshair.Translate(inputs.Look);

        if (crosshair.Transform.origin.x < 0)
            body.Scale = new Vector2(-1, 1);
        else body.Scale = new Vector2(1, 1);

        if (inputs.Fire)
        {
            var offset = data.Get<GunBarrel>().GlobalPosition - data.Get<Arm>().GlobalPosition;
            new Player_Projectile(data.Get<GunBarrel>().GlobalPosition, data.Get<Crosshair>().GlobalPosition + offset);
            //Debug.Log(data.Get<Crosshair>().Position, data.Get<GunBarrel>().Position);
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        LinearVelocity = data.Get<move_velocity>().value + data.Get<environment_forces>();
        data.Get<environment_forces>() = new Vector2(0, 64f);
    }


    public class input
    {
        InputAction up = new InputAction(KeyList.W, KeyList.Space);
        InputAction down = new InputAction(KeyList.S);
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);

        InputAction look_up = new InputAction(MouseList.up);
        InputAction look_down = new InputAction(MouseList.down);
        InputAction look_left = new InputAction(MouseList.left);
        InputAction look_right = new InputAction(MouseList.right);

        InputAction fire = new InputAction(MouseList.left_click, MouseList.right_click);

        public Vector2 Move => new Vector2(right - left,  down - up);
        public Vector2 Look => new Vector2(look_right.raw - look_left.raw, look_up.raw - look_down.raw) * look_sensitivity;

        public bool Fire => fire.on_pressed;

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

    public struct move_velocity
    {
        public Vector2 value;
    
        public static implicit operator Vector2(move_velocity value)
            => value.value;

        public static implicit operator move_velocity(Vector2 value)
            => new move_velocity{value = value};
    }

    public struct environment_forces
    {
        public Vector2 value;
    
        public static implicit operator Vector2(environment_forces value)
            => value.value;

        public static implicit operator environment_forces(Vector2 value)
            => new environment_forces{value= value};
    }

    public struct oxygen
    {
        public int value;

        public static implicit operator int(oxygen value)
            => value.value;

        public static implicit operator oxygen(int value)
            => new oxygen{value = value};
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
            var inputs = data.Get<Player.input>();
            ref var vel = ref data.Get<Player.move_velocity>();
            vel = vel.value.lerp(inputs.Move * data.Get<Player.movespeed>(), delta);
        }
    }

}


public class Player_Projectile : Node2D
{
    public Player_Projectile(Vector2 spawnPosition, Vector2 targetPosition)
    {
        this.AddChild(GD.Load<PackedScene>("res://Assets/Player/Projectile.tscn").Instance());
        this.Position = spawnPosition;
        Scene.Current.AddChild(this);
        this.FindChild<AnimationPlayer>().Play("Spray");

        var distance = targetPosition - spawnPosition;

        var angle = Vector2.Right.AngleTo(distance.Normalized());
        this.Rotation = angle;
    }

    float accumulated = 0;
    public override void _Process(float delta)
    {
        if (accumulated > .5f)
            this.QueueFree();
        else accumulated += delta;

    }
}