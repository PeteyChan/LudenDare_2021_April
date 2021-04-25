using System;

public struct int2: IEquatable<int2>
{
    public int2(int x, int y)
    {
        this.x = x; this.y = y;
    }

    public int x;
    public int y;

    public override bool Equals(object obj)
    {
        if (obj is int2 int2)
            return x == int2.x && y == int2.y;
        return false;
    }

    public override int GetHashCode()
        => unchecked(x * 37) ^ (y * 1543);

    public static bool operator ==(int2 a, int2 b)
        => a.x == b.x && a.y == b.y;

    public static bool operator !=(int2 a, int2 b)
        => !(a == b);

    public static int2 operator +(int2 a, int2 b)
        => new int2(a.x + b.x, a.y - b.y);

    public static int2 operator -(int2 a, int2 b)
        => new int2(a.x - b.x, a.y - b.y);

    public static int2 operator *(int2 a, int b)
        => new int2(a.x * b, a.y * b);
    
    public static int2 operator *(int b, int2 a)
        => a*b;
    
    public static int2 operator /(int2 a, int b)
        => new int2(a.x/b, a.y/b);

    public override string ToString()
        => $"int2( {x}x, {y}y)";

    public bool Equals(int2 other)
    => x == other.x && y == other.y;

    public static implicit operator Godot.Vector2(int2 value) => new Godot.Vector2(value.x, value.y);
}

public struct int3: IEquatable<int3>
{
    public int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int x, y, z;

    public override bool Equals(object obj)
    {
        if (obj is int3 int3)
            return x == int3.x && y == int3.y && z == int3.z;
        return false;
    }

    public override int GetHashCode()
        => unchecked(x * 37) ^ (y * 1543) ^ (z * 196613);

    public static bool operator ==(int3 a, int3 b)
        => a.x == b.x && a.y == b.y && a.z == b.z;

    public static bool operator !=(int3 a, int3 b)
        => !(a == b);

    public static int3 operator +(int3 a, int3 b)
        => new int3(a.x + b.x, a.y + b.y, a.z + b.z);

    public static int3 operator -(int3 a, int3 b)
        => new int3(a.x - b.x, a.y - b.y, a.z - b.z);

    public static int3 operator *(int3 a, int b)
        => new int3(a.x * b, a.y * b, a.z * b);
    
    public static int3 operator *(int b, int3 a)
        => a*b;

    public static int3 operator /(int3 a, int b)
        => new int3(a.x / b, a.y /b, a.z /b);

    public override string ToString()
    => $"int3( {x}x, {y}y, {z}z)";

    public bool Equals(int3 other)
    => x == other.x && y == other.y && z == other.z;
}