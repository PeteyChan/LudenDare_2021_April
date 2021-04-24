using System.Collections;
using System.Collections.Generic;

public sealed class TypeMap : IEnumerable
{
    static int newID = 0;

    interface IGetObject { object obj { get; } }

    class item<T> : IGetObject
    {
        public T data;
        public static readonly int ID = ++newID;
        object IGetObject.obj => data;
    }

    int[] indexes;
    object[] values;
    int factor;
    int steps;
    int data_offset;

    public TypeMap()
    {
        Clear();
    }

    public void Clear()
    {
        Count = 0;
        data_offset = 0;
        factor = mapping_data[data_offset].factor;
        steps = mapping_data[data_offset].steps;
        indexes = new int[factor + steps];
        values = new object[factor + steps];
    }

    public int Count
    {
        get; private set;
    }

    public bool Has<T>()
    {
        var index = item<T>.ID;
        var place = index % factor;
        for (int i = 0; i < steps; ++i)
        {
            var pos = place + i;
            var current_index = indexes[pos];
            if (index == 0)
                return false;
            if (index == current_index)
                return true;
        }
        return false;
    }

    public void Set(System.Type type, object value)
    {
        var method = typeof(TypeMap).GetMethod(nameof(Set), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method = method.MakeGenericMethod(type);
        method.Invoke(this, new object[] { value });
    }

    void Set<T>(object value)
    {
        Get<T>() = (T)System.Convert.ChangeType(value, typeof(T));
    }

    public TypeMap Set<T>(T value)
    {
        Get<T>() = value;
        return this;
    }

    public ref T Get<T>()
    {
        var index = item<T>.ID;
        var place = index % factor;
        for (int i = 0; i < steps; ++i)
        {
            var pos = place + i;
            var current_index = indexes[pos];
            if (index == current_index)
                return ref ((item<T>)values[pos]).data;
            if (current_index == 0)
            {
                Count++;
                item<T> value = new item<T>();
                indexes[pos] = index;
                values[pos] = value;
                return ref value.data;
            }
        }
        Resize();
        return ref Get<T>();
    }

    public ref T GetOrCreate<T>() where T : new()
    {
        var index = item<T>.ID;
        var place = index % factor;
        for (int i = 0; i < steps; ++i)
        {
            var pos = place + i;
            var current_index = indexes[pos];
            if (index == current_index)
                return ref ((item<T>)values[pos]).data;
        }

        ref var item = ref Get<T>();
        item = new T();
        return ref item;
    }

    void Resize()
    {
        
        data_offset++;
        factor = mapping_data[data_offset].factor;
        steps = mapping_data[data_offset].steps;

        int size = factor + steps;
        var new_indexes = new int[size];
        var new_values = new object[size];

        for (int position = 0; position < indexes.Length; ++position)
        {
            var index = indexes[position];
            if (index == 0)
                continue;
            var value = values[position];

            var target_position = index % factor;
            for (int offset = 0; offset < steps; ++offset)
            {
                var place = target_position + offset;
                if (new_indexes[place] == 0)
                {
                    new_indexes[place] = index;
                    new_values[place] = value;
                    goto Success;
                }
            }

            Resize(); // if too many hash collisions go to next size up
            return;

        Success:
            continue;
        }

        indexes = new_indexes;
        values = new_values;
    }

    public void Remove<T>()
    {
        var index = item<T>.ID;
        var pos = index % factor;
        int next = 0;

        while (next < steps)
        {
            var gap = pos + next;
            next++;
            var current_index = indexes[pos];
            if (index == current_index)
            {
                indexes[gap] = default;
                values[gap] = default;
                Count--;
                break;
            }
        }

        while (next < steps)
        {
            var gap = pos + next;
            var current_index = indexes[gap];
            if (current_index % factor == pos)
            {
                indexes[gap] = current_index;
                values[gap] = default;
            }
            next++;
        }
    }

    static (int factor, int steps)[] mapping_data = new (int, int)[]
    {
        (5, 3),
        (13, 3),
        (31, 3),
        (53, 3),
        (97, 3),
        (193, 3),
        (389, 3),
        (769, 3),
        (1543, 3),
        (3079, 3),
        (6151, 3),
        (12289, 3)
    };

    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < indexes.Length; ++i)
        {
            if (indexes[i] == 0)
                continue;
            else
                yield return ((IGetObject)values[i]).obj;
        }
    }
}