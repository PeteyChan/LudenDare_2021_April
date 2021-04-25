using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;

public static partial class Extensions
{
    public static float clamp(this float target, float min, float max)
    {
        if (min > max)
        {
            var temp = max;
            max = min;
            min = temp;
        }
        return target < min ? min : target > max ? max : target;
    }

    public static int clamp(this int target, int min, int max)
    {
        if (min > max)
        {
            var temp = max;
            max = min;
            min = temp;
        }
        return target < min ? min : target > max ? max : target;
    }

    public static int abs(this int target) => target < 0 ? target * -1 : target;
    public static float abs(this float target) => target < 0 ? target * -1 : target;

    public static float min(this float target, float min)
        => target < min ? min : target;

    public static float max(this float target, float max)
        => target > max ? max : target;

    public static int min(this int target, int min)
        => target < min ? min : target;

    public static int max(this int target, int max)
        => target > max ? max : target;

    public static Color lerp(this Color current, Color target, float weight)
        => weight < 0 ? current : weight > 1 ? target : weight * (target - current) + current;

    public static float lerp(this float current, float target, float weight)
        => weight < 0 ? current : weight > 1 ? target : weight * (target - current) + current;

    public static Vector2 lerp(this Vector2 current, Vector2 target, float weight)
    {
        return weight < 0 ? current : weight > 1 ? target : weight * (target - current) + current;
    }

    public static Vector3 lerp(this Vector3 current, Vector3 target, float weight)
    {
        return weight < 0 ? current : weight > 1 ? target : weight * (target - current) + current;
    }

    static StringBuilder builder = new StringBuilder();
    public static string AddSpaceBeforeCaps(this string current)
    {
        builder.Clear();
        if (current.Length > 1)
        {
            builder.Append(current[0]);
            for (int i = 1; i < current.Length; ++i)
            {
                if (char.IsUpper(current[i]) && char.IsLower(current[i - 1]))
                    builder.Append(" ");
                builder.Append(current[i]);
            }
        }
        return builder.ToString();
    }

    public static Vector3 qlerp(this Vector3 current, Vector3 target, float weight)
    {
        var t1 = Transform.Identity.LookingAt(current, Vector3.Up);
        var t2 = Transform.Identity.LookingAt(target, Vector3.Up);
        var q = t1.basis.Quat().Slerp(t2.basis.Quat(), weight);
        return new Transform(q, Vector3.Zero).basis.z;
    }

    public static Vector3 qlerp(this Vector3 current, Vector3 target, float weight, Vector3 axis)
    {
        var t1 = Transform.Identity.LookingAt(current, axis);
        var t2 = Transform.Identity.LookingAt(target, axis);
        var q = t1.basis.Quat().Slerp(t2.basis.Quat(), weight);
        return new Transform(q, Vector3.Zero).basis.z;
    }

    public static Vector3 setX(this Vector3 current, float value)
    {
        current.x = value;
        return current;
    }

    public static Vector3 setY(this Vector3 current, float value)
    {
        current.y = value;
        return current;
    }

    public static Vector3 setZ(this Vector3 current, float value)
    {
        current.z = value;
        return current;
    }

    public static Vector2 setX(this Vector2 current, float value)
    {
        current.x = value;
        return current;
    }

    public static Vector2 setY(this Vector2 current, float value)
    {
        current.y = value;
        return current;
    }

    public static int toHash(this string value)
    {
        int p = 53;
        int power = 1;
        int hashval = 0;

        unchecked
        {
            for (int i = 0; i < value.Length; ++i)
            {
                power *= p;
                hashval = (hashval + value[i] * power);
            }
        }
        return hashval;
    }

    public static float tilt(this Vector2 vector2)
    {
        var val = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
        return val > 1 ? 1 : val;
    }

    public static void ForeachParallel<T>(this IReadOnlyList<T> list, Action<T> action)
    {
        System.Threading.Tasks.Parallel.For(0, list.Count, (int index) => action(list[index]));
    }

    public static void ForeachParallel<T>(this IList<T> list, Action<T> action)
    {        
        System.Threading.Tasks.Parallel.For(0, list.Count, (int index) => action(list[index]));
    }
}

public static class Enum<T> where T : System.Enum
{
    public static readonly string[] Names = System.Enum.GetNames(typeof(T));
    public static readonly int Count = Names.Length;
    public static readonly T[] Values = System.Enum.GetValues(typeof(T)).Cast<T>().ToArray();
}

/// <summary>
/// Random number generator
/// </summary>
public static class Rand
{
    static System.Random rand = new System.Random();
    public static void SetSeed(int seed)
    {
        rand = new Random(seed);
    }

    public static bool Bool => (rand.Next() & 1) == 0;

    public static float Float
        => Float01 * 2f - 1f;
    public static float Float01
        => (float)rand.NextDouble();
    public static float FloatMax(float max)
        => Float01 * max;
    public static float FloatRange(float min, float max)
        => Float01 * (max - min) + min;
    public static int Int(int max)
        => rand.Next(max);
    public static int Int(int min, int max)
        => rand.Next(min, max);
}

public static class NodeExtensions
{
    public static T FindParent<T>(this Node node) where T: class
    {
        if (node is T value)
            return value;
        return node?.GetParent()?.FindParent<T>();
    }

    public static bool TryFindParent<T>(this Node node, out T obj) where T: class
    {
        obj = node.FindParent<T>();
        return Node.IsInstanceValid(obj as Node); 
    }

    public static T FindChild<T>(this Node node) where T: class
    {
        if (node is T value)
            return value;
        
        foreach(Node child in node.GetChildren())
        {
            var childtype = FindChild<T>(child);
            if (childtype != null)
                return childtype;
        }
        return null;
    }

    public static bool TryFindChild<T>(this Node node, out T obj) where T : class
    {
        obj = node.FindChild<T>();
        return Node.IsInstanceValid(obj as Node);
    }

    public static List<T> GetAll<T>(this Node node, bool include_self = true) where T : class
    {
        var source = new List<T>();

        if (include_self)
        {
            if (node is T target)
                source.Add(target);
        }
        FindNodes(node);
        return source;

        void FindNodes(Node current)
        {
            foreach (Node child in current.GetChildren())
            {
                if (child is T target)
                {
                    source.Add(target);
                }
                FindNodes(child);
            }
        }
    }

    public static void LookAt(this Spatial spatial, Vector3 direction, float weight = 1)
    {
        direction = direction.Normalized();
        if (direction == Vector3.Up || direction == Vector3.Zero)
            return;
        var t = spatial.GlobalTransform;
        var t2 = Transform.Identity.LookingAt(direction, Vector3.Up);
        spatial.GlobalTransform = new Transform(t.basis.Slerp(t2.basis, weight.clamp(0, 1)), t.origin);
    }

    public static void LookAt(this Spatial spatial, Vector3 direction, Vector3 axis, float weight = 1)
    {
        direction = direction.Normalized();
        axis = axis.Normalized();
        if (direction == axis || direction == Vector3.Zero || axis == Vector3.Zero)
            return;
        var t = spatial.GlobalTransform;
        var t2 = Transform.Identity.LookingAt(direction, axis);
        spatial.GlobalTransform = new Transform(t.basis.Slerp(t2.basis, weight.clamp(0, 1)), t.origin);
    }

    public static bool TryLoadAsChild(this Node parent, out Node node, string path)
    {
        node = GD.Load<PackedScene>(path).Instance();
        if (Node.IsInstanceValid(node))
        {
            parent.AddChild(node);
            return true;
        }
        return false;
    }
}

public static class TypeID
{
    static Dictionary<Type, int> newIDs = new Dictionary<Type, int>();
    public static int Get(Type type)
    {
        if (!newIDs.TryGetValue(type, out var id))
            newIDs[type] = id = newIDs.Count;
        return id;
    }

    public static int Get<T>() => ID<T>.value;

    class ID<T>
    {
        public static int value = Get(typeof(T));
    }
}