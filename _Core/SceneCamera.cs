using ConsoleCommands;
using System.Collections;
using Godot;
using Events;

namespace _Core
{
    class Scenecamera2D : ICommand, IUpdater
    {
        Camera2D camera, previous;
        public void OnCommand(ConsoleArgs args)
        {
            if (Node.IsInstanceValid(camera))
            {
                if (Node.IsInstanceValid(previous))
                    previous.Current= true;
                camera.QueueFree();
            }
            else
            {
                foreach(var cam in Scene.Current.GetAll<Camera2D>())
                    if (cam.Current) previous = cam;
                
                camera = new Camera2D();
                Scene.Current.AddChild(camera);
                this.StartUpdates();
                camera.Current = true;
            }
        }

        InputAction up = new InputAction(KeyList.W);
        InputAction down = new InputAction(KeyList.S);
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);
        InputAction zoom_in = new InputAction(KeyList.E);
        InputAction zoom_out = new InputAction(KeyList.Q);
        InputAction slow = new InputAction(KeyList.Shift);
        InputAction fast = new InputAction(KeyList.Space);

        Vector2 velocity = Vector2.Zero;

        public bool Update(float delta)
        {
            float speed = 128;
            if (fast.pressed)
                speed *= 20f;
            if (slow.pressed)
                speed *= .2f;
            if (zoom_in.pressed)
                camera.Zoom -= Vector2.One * delta * (speed / 100f);
            if (zoom_out.pressed)
                camera.Zoom += Vector2.One * delta * (speed / 100f);

            Vector2 moveVector = new Vector2(left - right, up - down) * speed;        
            velocity = velocity.lerp(moveVector * Time.frame_delta, Time.frame_delta * 10f);

            camera.Translate(velocity);
            return Node.IsInstanceValid(camera);
        }
    }

    class SceneCamera3D : ICommand, IUpdater
    {
        public void OnCommand(ConsoleArgs args)
        {
            if (Node.IsInstanceValid(camera))
                camera.QueueFree();
            else
            {
                camera = new Camera();
                Scene.Current.AddChild(camera);
                this.StartUpdates();
                camera.Current = true;
            }
        }

        Camera camera;
        InputAction up = new InputAction(KeyList.Q);
        InputAction down = new InputAction(KeyList.E);
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);
        InputAction forward = new InputAction(KeyList.W);
        InputAction back = new InputAction(KeyList.S);
        InputAction slow = new InputAction(KeyList.Shift);
        InputAction fast = new InputAction(KeyList.Space);
        InputAction look_left = new InputAction(MouseList.left, KeyList.Kp4);
        InputAction look_right = new InputAction(MouseList.right, KeyList.Kp6);
        InputAction look_up = new InputAction(MouseList.down, KeyList.Kp8);
        InputAction look_down = new InputAction(MouseList.up, KeyList.Kp5);

        Vector3 velocity = Vector3.Zero;
        float yaw = 0, pitch = 0;

        public bool Update(float delta)
        {
            float speed = 5;
            if (fast.pressed)
                speed *= 10f;
            if (slow.pressed)
                speed *= .2f;
            Vector3 moveVector = new Vector3(left - right, up - down, forward - back) * speed;
            yaw += (look_left - look_right) * 6f * Time.frame_delta;
            pitch += (look_up - look_down) * 6f * Time.frame_delta;

            camera.Transform = new Transform(new Basis(new Vector3(pitch, yaw, 0)), camera.Transform.origin);
            velocity = velocity.lerp(-moveVector * Time.frame_delta, Time.frame_delta * 10f);
            camera.Translate(velocity);
            return Node.IsInstanceValid(camera);
        }
    }
}