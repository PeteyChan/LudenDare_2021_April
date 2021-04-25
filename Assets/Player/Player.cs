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

    public static Player instance { get; private set; }

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
        if (data.Get<bool>()) return;
        data.Get<bool>() = true;

        instance = this;
        Input.SetMouseMode(Input.MouseMode.Captured);
        statemachine.Change<Player_States.Move>();

        data.Set(this as RigidBody2D)
            .Set(this.FindChild<AnimationPlayer>())
            .Set(this.FindChild<Crosshair>())
            .Set(this.FindChild<GunBarrel>())
            .Set(this.FindChild<Arm>())
            .Set(new input())
            .Set<movespeed>(200)
            .Set<oxygen>(5)
            ;

        data.Get<Crosshair>().Position= new Vector2(128, 0);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Debug.Label("Depth:", ((int)Position.y) / 128 + data.Get<depth>());
        statemachine.Update(data, delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        LinearVelocity = data.Get<move_velocity>().value + data.Get<environment_forces>();
        data.Get<environment_forces>() = new Vector2(0, 64f);
    }

    void On_Enter_Player(Body body)
    {
        var pickup = body.FindParent<Oxygen_Pickup>();
        if (pickup != null)
        {
            pickup.QueueFree();
            if (data.Get<Player.oxygen>() < 12)
                data.Get<Player.oxygen>() ++;
        }
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

        public Vector2 Move => new Vector2(right - left, down - up);
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
            => new movespeed { value = value };
    }

    public struct move_velocity
    {
        public Vector2 value;

        public static implicit operator Vector2(move_velocity value)
            => value.value;

        public static implicit operator move_velocity(Vector2 value)
            => new move_velocity { value = value };
    }

    public struct environment_forces
    {
        public Vector2 value;

        public static implicit operator Vector2(environment_forces value)
            => value.value;

        public static implicit operator environment_forces(Vector2 value)
            => new environment_forces { value = value };
    }

    public struct oxygen
    {
        public int value;

        public static implicit operator int(oxygen value)
            => value.value;

        public static implicit operator oxygen(int value)
            => new oxygen { value = value };
    }

    public struct depth
    {
        public int value;

        public static implicit operator int(depth value)
            => value.value;

        public static implicit operator depth(int value)
            => new depth { value = value };
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
            var crosshair = data.Get<Crosshair>();
            var body = data.Get<Body>();

            ref var vel = ref data.Get<Player.move_velocity>();
            vel = vel.value.lerp(inputs.Move * data.Get<Player.movespeed>(), delta);

            var env = data.Get<Player.environment_forces>();

            var rot = (vel.value ).Normalized().y * 20f;
            var model = body.GetChild(0) as Node2D;
            model.Rotation = model.Rotation.lerp(Mathf.Deg2Rad(rot), delta * 2f);


            crosshair.Translate(inputs.Look);
            var crosspos = crosshair.Position;
            if (crosspos.x < -500) crosspos.x = -500;
            if (crosspos.x > 500) crosspos.x = 500;
            if (crosspos.y < -300) crosspos.y = -300;
            if (crosspos.y > 300) crosspos.y = 300;
            crosshair.Position = crosspos;

            if (crosshair.Transform.origin.x < 0)
                body.Scale = new Vector2(-1, 1);
            else body.Scale = new Vector2(1, 1);

            if (inputs.Fire)
            {
                var offset = data.Get<GunBarrel>().GlobalPosition - data.Get<Arm>().GlobalPosition;
                new Player_Projectile(data.Get<GunBarrel>().GlobalPosition, data.Get<Crosshair>().GlobalPosition + offset);
                data.Get<Player.oxygen>() --;
                //Debug.Log(data.Get<Crosshair>().Position, data.Get<GunBarrel>().Position);
            }

            data.Get<Arm>().LookAt(crosshair.GlobalPosition);


            if (data.Get<Player.oxygen>() == 0)
                stateMachine.Change<Death>();
        }
    }

    class Death : State
    {
        public override void OnEnter(StateMachine stateMachine, TypeMap data)
        {
            data.Get<Crosshair>().QueueFree();
            data.Get<AnimationPlayer>().Play("Death");
        }

        public override void OnUpdate(StateMachine stateMachine, TypeMap data, float delta, float state_time)
        {
            data.Get<Player.move_velocity>() = data.Get<Player.move_velocity>().value.lerp(Vector2.Zero, state_time);
            if (state_time > 3)
                Scene.Load("res://Scenes/Main.tscn");
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
            QueueFree();
        else accumulated += delta;
    }
}