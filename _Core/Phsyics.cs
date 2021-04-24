using Godot;

public class Physics: IDispatcher<Events.Bootstrap>
{
    static SceneTree tree;
    static Viewport viewport => tree.CurrentScene.GetViewport();
    static PhysicsDirectSpaceState space => tree.CurrentScene.GetViewport().World.DirectSpaceState;
    static Physics2DDirectSpaceState space2D => tree.CurrentScene.GetViewport().World2d.DirectSpaceState;
    static Camera cam => viewport.GetCamera();

    static CircleShape2D circle = new CircleShape2D();
    static Physics2DShapeQueryParameters shapeParams = new Physics2DShapeQueryParameters();

    static SphereShape sphere = new SphereShape();
    static PhysicsShapeQueryParameters volumeParams = new PhysicsShapeQueryParameters();
    
    public static bool TrySphereCast(Vector3 global_position, Vector3 direction, float radius, out RayCast3DResult result, uint mask = 1, bool body = true, bool area = false, bool debug = false)
    {
        result = new RayCast3DResult();
        sphere.Radius = radius;
        volumeParams.CollideWithBodies = body;
        volumeParams.CollideWithAreas = area;
        volumeParams.SetShape(sphere);
        volumeParams.CollisionMask = (int)mask;
        var transform = Transform.Identity;
        transform.origin = global_position;
        volumeParams.Transform = transform;

        var cast = space.CastMotion(volumeParams, direction);
        if (cast.Count == 0)
        {
            return false;
        }
        transform.origin = global_position+direction * (float)cast[1];
        volumeParams.Transform = transform;
        var info = space.GetRestInfo(volumeParams);                       
        
        var hit = info.Count > 0;
        if (hit)
        {
            result.normal = (Vector3)info["normal"];
            result.point = (Vector3)info["point"];
            var col = (Godot.Collections.Dictionary)space.IntersectShape(volumeParams, 1)[0];
            result.collider = (Node)col["collider"];
        }

        if (debug)
        {
            var color = Colors.Green;
            if (hit)
            {
                color = Colors.Red;
                Debug.DrawLine3D(result.point, transform.origin, Colors.Cyan);
                Debug.DrawCircle3D(transform.origin, cam.GlobalTransform.basis.z, radius, color);
            }
            else
            {
                Debug.DrawCircle3D(global_position + direction, cam.GlobalTransform.basis.z, radius, color);
            }
            Debug.DrawLine3D(global_position, global_position + direction, color);
        }
        return hit;
    }

    public static bool TryRayCast(Vector3 global_position, Vector3 direction, out RayCast3DResult result, uint mask = 1, bool body = true, bool area = false, bool debug = false)
    {
        var outcome = space.IntersectRay(global_position, global_position + direction, null, mask, body, area);
        result = new RayCast3DResult();
        var hit = outcome.Count > 0;
        if (hit)
        {
            result.normal = (Vector3)outcome["normal"];
            result.point = (Vector3)outcome["position"];
            result.collider = (Node)outcome["collider"];
        }
        if (debug)
        {
            if (hit)
            {
                Debug.DrawLine3D(global_position, result.point, Colors.Red);
                Debug.DrawLine3D(result.point, result.point + result.normal, Colors.Cyan);
            }
            else Debug.DrawLine3D(global_position, global_position + direction, Colors.Green);   
        }
        return hit;
    }

    public static bool TryRayCast2D(Vector2 global_position, Vector2 direction, out Raycast2DResult result, uint mask = 1, bool body = true, bool area = false, bool debug = false)
    {
        var outcome = space2D.IntersectRay(global_position,global_position + direction, null, mask, body, area);
        result = new Raycast2DResult();
        var hit = outcome.Count > 0;
        if (hit)
        {
            result.normal = (Vector2)outcome["normal"];
            result.point = (Vector2)outcome["position"];
            result.collider = (Node)outcome["collider"];
        }
        if (debug)
        {
            if (hit)
            {
                Debug.DrawLine2D(global_position, result.point, Colors.Red);
                Debug.DrawLine2D(result.point, result.point + result.normal, Colors.Cyan);
            }
            else Debug.DrawLine2D(global_position, global_position+ direction, Colors.Green);
        }
        return hit;
    }

    public static bool TryCircleCast2D(Vector2 global_position, Vector2 direction, float radius, out Raycast2DResult result, uint mask = 1, bool body = true, bool area = false, bool debug = false)
    {
        result = new Raycast2DResult();
        circle.Radius = radius;
        shapeParams.CollideWithAreas = area;
        shapeParams.CollideWithBodies = body;
        shapeParams.CollisionLayer = (int)mask;
        shapeParams.SetShape(circle);
        shapeParams.Motion = direction;
        shapeParams.Transform = new Transform2D(0, global_position);
        
        var cast = space2D.CastMotion(shapeParams);
        if (cast.Count == 0)
        {
            return false;
        }
        var origin = global_position + (direction * (float)cast[1]);
        shapeParams.Transform = new Transform2D(0, origin);
        shapeParams.Motion = Vector2.Zero;
        var info = space2D.GetRestInfo(shapeParams); 

        var hit = info.Count > 0;
        if (hit)
        {
            result.normal = (Vector2)info["normal"];
            result.point = (Vector2)info["point"];
            var col = (Godot.Collections.Dictionary)space2D.IntersectShape(shapeParams, 1)[0];
            result.collider = (Node)col["collider"];
        }

        if (debug)
        {
            var color = Colors.Green;
            if (hit)
            {
                color = Colors.Red;
                Debug.DrawLine2D(global_position, global_position + direction, color, 1);
                Debug.DrawCircle2D(origin, radius, color);
                Debug.DrawLine2D(origin, result.point, Colors.Cyan, 1);
            }
            else
            {
                Debug.DrawLine2D(global_position, global_position + direction, color, 1);
                Debug.DrawCircle2D(global_position + direction, radius, color);
            }
        }
        return hit;
    }

    public void OnDispatch(Events.Bootstrap args)
    {
        tree = args.GetTree();
    }

    public struct Raycast2DResult
    {
        public Vector2 normal;
        public Vector2 point;
        public Node collider;
    }

    public struct RayCast3DResult
    {
        public Vector3 normal;
        public Vector3 point;
        public Node collider;
    }
}

