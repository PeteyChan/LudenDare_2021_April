using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    using ECS.Internal;

    class Archetype : Attribute
    {
        public Type[] components;
        public Archetype(params Type[] components)
        {
            this.components = components;
        }
    }

    public struct Entity : IEquatable<Entity>
    {
        public readonly ushort version;
        public readonly ushort archetype;
        public readonly int index;

        public Entity(int id, ushort version, ushort archetype)
        {
            this.index = id;
            this.version = version;
            this.archetype = archetype;
        }

        public bool Equals(Entity other)
        => index == other.index && version == other.version && archetype == other.archetype;

        public override int GetHashCode()
        {
            unchecked
            {
                return index * 53 + archetype * 6291469 + version * 6151;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Entity entity)
            {
                return Equals(entity);
            }
            return false;
        }

        public static Entity Spawn<Archetype>()
        {
            if (Archetype_Index<Archetype>.value > 0)
                return ArchetypeManager.archetypes[Archetype_Index<Archetype>.value].spawn_entity();
            
            Debug.Log($"{typeof(Archetype)} is not an archtype, failed to create entity");
            return new Entity();
        }

        public bool Has<Component>()
            => archetype > 0 ? ArchetypeManager.archetypes[archetype].has<Component>(): false;

        public Entity Set<Component>(Component component)
        {
            if (archetype > 0)
                ArchetypeManager.archetypes[archetype].try_set(this, component);
            return this;
        }

        public bool TryGet<Component>(out Component component)
        {
            if (archetype > 0)
                return ArchetypeManager.archetypes[archetype].try_get(this, out component);
            component = default;
            return false;
        }

        public void Destroy()
        {
            if (archetype > 0)
                ArchetypeManager.archetypes[archetype]?.destroy_entity(this);
        }

        public static IReadOnlyList<Entity> GetAll<Archetype>()
        {
            if (Archetype_Index<Archetype>.value > 0)
                return Archetype<Archetype>.Components<Entity>.components;
            return new ECSCollection<Entity>();
        }

        public static void Destroy<Archetype>()
        {
            if (Archetype_Index<Archetype>.value != 0)
                ArchetypeManager.archetypes[Archetype_Index<Archetype>.value]?.destory_all_entities();
        }

        public static void DestroyAll()
        {
            foreach (var archetype in ArchetypeManager.archetypes)
            {
                archetype?.destory_all_entities();
            }
            System.GC.Collect();
        }

        public override string ToString()
        {
            var archetypes = ArchetypeManager.archetypes;
            if (archetype > 0 && archetype < archetypes.Length)
            {
                return this ? $"{archetypes[archetype].type} {index}:{version}" :
                    $"Destroyed {archetypes[archetype].type} {index}:{version}";
            }
            return $"NULL Entity";
        }

        public static implicit operator bool(Entity entity)
            => entity.archetype > 0 ? ArchetypeManager.archetypes[entity.archetype].alive(entity) : false;
    }
}

namespace ECS.Internal
{
    class ArchetypeManager
    {
        static ArchetypeManager()
        {
            List<BaseArchetype> list_archetypes = new List<BaseArchetype>() { null };
            foreach (var type in typeof(Entity).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<ECS.Archetype>() != null)
                {
                    archetypeID[type] = (ushort)list_archetypes.Count;
                    var archetype = typeof(Archetype<>).MakeGenericType(new Type[] { type });
                    list_archetypes.Add(Activator.CreateInstance(archetype) as BaseArchetype);
                }
            }
            archetypes = list_archetypes.ToArray();
        }

        static Dictionary<Type, ushort> archetypeID = new Dictionary<Type, ushort>();

        public static ushort GetArchetypeID(Type type)
        {
            if (archetypeID.TryGetValue(type, out var value))
                return value;
            return 0;
        }

        public readonly static BaseArchetype[] archetypes;
    }

    public class Archetype_Index<Archetype>
    {
        public static readonly int value = ArchetypeManager.GetArchetypeID(typeof(Archetype));
    }

    abstract class BaseArchetype
    {
        public abstract Type type { get; }

        public abstract int entity_count { get; }

        public abstract bool has<Component>();

        public abstract bool try_set<Component>(Entity entity, in Component component);

        public abstract bool try_get<Component>(Entity entity, out Component component);

        public abstract bool try_get_component_array<Component>(out Component[] components);

        public abstract bool alive(Entity entity);

        public abstract Entity spawn_entity();

        public abstract void destory_all_entities();

        public abstract void destroy_entity(Entity entity);

        public Action<BaseArchetype> on_spawn_intial_entity = delegate { };

        public abstract Type[] components { get; }
    }

    class Archetype<Type> : BaseArchetype
    {
        static Archetype()
        {
            Components<Entity>.Initialize();
            foreach (var type in typeof(Type).GetCustomAttribute<ECS.Archetype>().components)
            {
                typeof(Components<>).MakeGenericType(typeof(Type), type)
                    .GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            }
            increment_versions();
        }

        ///     VARIABLES           ///
        static BaseArchetype archetype_instance => ArchetypeManager.archetypes[archetype_id];
        static readonly ushort archetype_id = ArchetypeManager.GetArchetypeID(typeof(Type));
        static (ushort version, int component_index)[] entity_map = new (ushort version, int component_index)[8];
        static Stack<int> free_entity_indexes = new Stack<int>();
        static List<System.Type> archtype_components = new List<System.Type>();

        ///     ARCHETYPE EVENTS    ///

        static Action create_entity_components = delegate { };
        static Action destroy_all_entities_event = delegate { };
        static Action<int> remove_components_from_entity = delegate { };

        ///     COMPONENT POOL      ///

        public class Components<C>
        {
            public static bool initialized = false;
            public static ECSCollection<C> components;

            public static void Initialize()
            {
                if (initialized) return;
                initialized = true;
                components = new ECSCollection<C>();
                archtype_components.Add(typeof(C));
                create_entity_components += () => components.Add(default);
                destroy_all_entities_event += () => components = new ECSCollection<C>();
                remove_components_from_entity += (int index) => components.RemoveAndSwapLast(index);
            }
        }

        ///     ARCHTYPE METHODS    ///

        static void increment_versions()
        {
            for (int i = 0; i < entity_map.Length; ++i)
                entity_map[i].version++;
        }

        public static Entity SpawnEntity()
        {
            int component_index = Components<Entity>.components.count;

            if (component_index == 0)
                archetype_instance.on_spawn_intial_entity(archetype_instance);

            if (component_index == entity_map.Length)
                Array.Resize(ref entity_map, entity_map.Length * 2);

            int entity_index;
            if (free_entity_indexes.Count > 0)
                entity_index = free_entity_indexes.Pop();
            else entity_index = component_index;

            create_entity_components();
            entity_map[entity_index].component_index = component_index;

            var entity = new Entity(entity_index, entity_map[entity_index].version, archetype_id);
            Components<Entity>.components.items[component_index] = entity;
            return entity;
        }

        ///     OVERRIDES            ///

        public override System.Type type => typeof(Type);

        public override System.Type[] components => archtype_components.ToArray();

        public override int entity_count => Components<Entity>.components.count;

        public override Entity spawn_entity()
            => SpawnEntity();

        public override void destroy_entity(Entity entity)
        {
            ref var map = ref entity_map[entity.index];
            if (map.version == entity.version)
            {
                map.version++;
                remove_components_from_entity(map.component_index);

                free_entity_indexes.Push(entity.index);

                if (map.component_index < Components<Entity>.components.count)
                {
                    var e = Components<Entity>.components.items[map.component_index]; // get entity now in component index
                    entity_map[e.index].component_index = map.component_index; // assign his new component index
                }
            }
        }

        public override void destory_all_entities()
        {
            destroy_all_entities_event();
            free_entity_indexes.Clear();
            increment_versions();
        }

        public override bool has<Component>() => Components<Component>.initialized;

        public override bool try_get<Component>(Entity entity, out Component component)
        {
            ref var map = ref entity_map[entity.index];
            if (Components<Component>.initialized && map.version == entity.version)
            {
                component = Components<Component>.components.items[map.component_index];
                return true;
            }
            component = default;
            return false;
        }

        public override bool try_get_component_array<Component>(out Component[] components)
        {
            if (Components<Component>.initialized)
            {
                components = Components<Component>.components.items;
                return true;
            }
            components = default;
            return false;
        }

        public override bool try_set<Component>(Entity entity, in Component component)
        {
            ref var map = ref entity_map[entity.index];
            if (Components<Component>.initialized && map.version == entity.version)
            {
                Components<Component>.components.items[map.component_index] = component;
                return true;
            }
            Debug.Log("failed set", typeof(Component));
            return false;
        }

        public override bool alive(Entity entity)
            => entity.version == entity_map[entity.index].version;

        public override string ToString()
        => $"{type} Archetype";
    }

    /// <summary>
    /// custom collection built for purpose 
    /// </summary>
    class ECSCollection<T> : IReadOnlyList<T>
    {
        public ECSCollection()
        {
            items = new T[8];
        }

        public T[] items;
        public int count;

        //public ref T last => ref items[count - 1];

        int IReadOnlyCollection<T>.Count => count;

        public void Resize()
        {
            var newSize = 8;
            while (newSize < count)
                newSize *= 2;
            if (items.Length > newSize)
                Array.Resize(ref items, newSize);
        }

        /// <summary>
        /// returns index of added item
        /// </summary>
        public int Add(T item)
        {
            if (count == items.Length)
                Array.Resize(ref items, count * 2);
            items[count] = item;
            count++;
            return count - 1;
        }

        public int Add()
        {
            if (count == items.Length)
                Array.Resize(ref items, count * 2);
            count++;
            return count - 1;
        }

        // swapping with back ensures arrays are packed
        public void RemoveAndSwapLast(int index)
        {
            count--;
            items[index] = items[count];
            items[count] = default;
        }

        T IReadOnlyList<T>.this[int index] => items[index];

        Iterator iterator = new Iterator();

        IEnumerator IEnumerable.GetEnumerator()
        => iterator.Init(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => iterator.Init(this);

        class Iterator : IEnumerator<T>
        {
            public Iterator Init(ECSCollection<T> list)
            {
                this.list = list;
                position = list.count;
                return this;
            }

            ECSCollection<T> list;
            int position;

            T IEnumerator<T>.Current => list.items[position];

            object IEnumerator.Current => list.items[position];

            void IDisposable.Dispose() { }

            // iterating backwards means we can remove entities without invalidating iterators
            bool IEnumerator.MoveNext()
            {
                position--;
                return position >= 0;
            }

            void IEnumerator.Reset()
            {
                position = list.count;
                Debug.Log("Reset", typeof(T).Name);
            }
        }
    }
}

namespace ECS.Internal
{
    interface IQuery
    {
        /// <summary>
        /// Query will return archetypes with all components
        /// </summary>
        IQuery Has<C>();

        /// <summary>
        /// Query will return archetypes which do not have component
        /// </summary>
        IQuery Not<C>();

        /// <summary>
        /// Query will return archetypes which contains at least one of these components
        /// </summary>
        IQuery Any<C>();

        Query Complete { get; }
    }

    public delegate void query<C1>(ref C1 c1);
    public delegate void query<C1, C2>(ref C1 c1, ref C2 c2);
    public delegate void query<C1, C2, C3>(ref C1 c1, ref C2 c2, ref C3 c3);
    public delegate void query<C1, C2, C3, C4>(ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);
    public delegate void query<C1, C2, C3, C4, C5>(ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ref C5 c5);
    public delegate void query<C1, C2, C3, C4, C5, C6>(ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ref C5 c5, ref C6 c6);
    public delegate void query<C1, C2, C3, C4, C5, C6, C7>(ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ref C5 c5, ref C6 c6, ref C7 c7);
    public delegate void query<C1, C2, C3, C4, C5, C6, C7, C8>(ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ref C5 c5, ref C6 c6, ref C7 c7, ref C8 c8);
}

namespace ECS
{
    using Internal;

    class Query : IQuery
    {
        Query() { }

        bool QueryArchetype(BaseArchetype archetype)
        {
            bool any = false, any_sucess = false;

            foreach (var test in items)
            {
                switch (test.query_type)
                {
                    case Item.type.all:
                        if (!test.Pass(archetype))
                            return false;
                        break;
                    case Item.type.not:
                        if (test.Pass(archetype))
                            return false;
                        break;
                    case Item.type.any:
                        any = true;
                        if (!any_sucess) any_sucess = test.Pass(archetype);
                        break;
                }
            }
            return any ? any_sucess : true;
        }

        List<Item> items = new List<Item>();

        static Dictionary<Query, Query> queries = new Dictionary<Query, Query>();

        abstract class Item
        {
            public enum type
            {
                all = 53,
                not = 1543,
                any = 196613
            }

            public type query_type;

            public abstract bool Pass(BaseArchetype archetype);
        }

        class QueryItem<C> : Item
        {
            public QueryItem(type query_type)
            {
                this.query_type = query_type;
            }

            public override string ToString()
            => query_type.ToString() + " " + typeof(C).Name + " ";

            public override bool Equals(object obj)
            {
                if (obj is QueryItem<C> q)
                    return q.query_type == query_type;
                return false;
            }

            public override int GetHashCode()
                => TypeID.Get<C>() * (int)query_type;

            public override bool Pass(BaseArchetype archetype)
                => archetype.has<C>();
        }

        static Query singleton = new Query();

        IQuery Clear()
        {
            singleton.items.Clear();
            return singleton;
        }

        public static IQuery Start => singleton.Clear();
        public static Query[] All => queries.Values.ToArray();

        IQuery IQuery.Has<C>()
        {
            items.Add(new QueryItem<C>(Item.type.all));
            return this;
        }

        IQuery IQuery.Not<C>()
        {
            items.Add(new QueryItem<C>(Item.type.not));
            return this;
        }

        IQuery IQuery.Any<C>()
        {
            items.Add(new QueryItem<C>(Item.type.any));
            return this;
        }

        string GetQueryName()
        {
            var builder = Singleton.Get<System.Text.StringBuilder>();
            builder.Clear();
            builder.Append("Query: ");
            foreach (var item in items)
            {
                builder.Append(item.ToString());
            }
            return builder.ToString();
        }

        Query IQuery.Complete
        {
            get
            {
                items = items.Distinct().ToList();
                items.Sort((x, y) =>
                {
                    int sort = x.query_type.CompareTo(y.query_type);
                    if (sort == 0)
                        return x.GetHashCode().CompareTo(y.GetHashCode());
                    return sort;
                });

                hash = 0;
                for (int i = 0; i < items.Count; ++i)
                {
                    unchecked
                    {
                        hash += items[0].GetHashCode();
                    }
                }

                if (!queries.TryGetValue(this, out var stored_query))
                {
                    queries[this] = stored_query = this;
                    query_name = GetQueryName();
                    singleton = new Query();

                    for (int i = 1; i < ArchetypeManager.archetypes.Length; ++i)
                    {
                        var archetype = ArchetypeManager.archetypes[i];
                        if (QueryArchetype(archetype))
                        {
                            query_active_archetypes.Add(archetype);
                        }
                    }
                }
                return stored_query;
            }
        }

        ECSCollection<BaseArchetype> query_active_archetypes = new ECSCollection<BaseArchetype>();

        /// <summary>
        /// Allows manual data iteration
        /// </summary>
        public IReadOnlyList<BaseArchetype> ActiveArchetypes => query_active_archetypes;

        void on_archetype_activate(BaseArchetype archetype)
        {
            archetype.on_spawn_intial_entity -= on_archetype_activate;
            query_active_archetypes.Add(archetype);
        }

        public override bool Equals(object obj)
        {
            if (obj is Query query)
            {
                if (items.Count != query.items.Count)
                    return false;

                for (int i = 0; i < items.Count; ++i)
                {
                    if (!items[i].Equals(query.items[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        int hash;
        public override int GetHashCode() => hash;

        string query_name = "";
        public override string ToString() => query_name;

        public int entity_count
        {
            get
            {
                int count = 0;
                foreach (var archetype in query_active_archetypes)
                    count += archetype.entity_count;
                return count;
            }
        }

        public void Foreach<C1>(query<C1> action, bool multithread = false)
        {
            for (int i = query_active_archetypes.count - 1; i >= 0; --i)
            {
                var archetype = query_active_archetypes.items[i];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += on_archetype_activate;
                    query_active_archetypes.RemoveAndSwapLast(i);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1))
                    {
                        if (multithread)
                        {
                            System.Threading.Tasks.Parallel.For(0, count, index => {
                                action(ref c1[index]);
                            });
                            continue;
                        }

                        for (int i2 = count - 1; i2 >= 0; --i2)
                        {
                            action(ref c1[i2]);
                        }
                    }

                }
            }
        }

        public void Foreach<C1, C2>(query<C1, C2> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3>(query<C1, C2, C3> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3, C4>(query<C1, C2, C3, C4> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3)
                        && archetype.try_get_component_array(out C4[] c4))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i], ref c4[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3, C4, C5>(query<C1, C2, C3, C4, C5> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3)
                        && archetype.try_get_component_array(out C4[] c4)
                        && archetype.try_get_component_array(out C5[] c5))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3, C4, C5, C6>(query<C1, C2, C3, C4, C5, C6> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3)
                        && archetype.try_get_component_array(out C4[] c4)
                        && archetype.try_get_component_array(out C5[] c5)
                        && archetype.try_get_component_array(out C6[] c6))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i], ref c6[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3, C4, C5, C6, C7>(query<C1, C2, C3, C4, C5, C6, C7> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3)
                        && archetype.try_get_component_array(out C4[] c4)
                        && archetype.try_get_component_array(out C5[] c5)
                        && archetype.try_get_component_array(out C6[] c6)
                        && archetype.try_get_component_array(out C7[] c7))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i], ref c6[i], ref c7[i]);
                        }
                    }
                }
            }
        }

        public void Foreach<C1, C2, C3, C4, C5, C6, C7, C8>(query<C1, C2, C3, C4, C5, C6, C7, C8> action)
        {
            for (int itr = query_active_archetypes.count - 1; itr >= 0; --itr)
            {
                var archetype = query_active_archetypes.items[itr];
                var count = archetype.entity_count;

                if (count == 0)
                {
                    archetype.on_spawn_intial_entity += (on_archetype_activate);
                    query_active_archetypes.RemoveAndSwapLast(itr);
                }
                else
                {
                    if (archetype.try_get_component_array(out C1[] c1)
                        && archetype.try_get_component_array(out C2[] c2)
                        && archetype.try_get_component_array(out C3[] c3)
                        && archetype.try_get_component_array(out C4[] c4)
                        && archetype.try_get_component_array(out C5[] c5)
                        && archetype.try_get_component_array(out C6[] c6)
                        && archetype.try_get_component_array(out C7[] c7)
                        && archetype.try_get_component_array(out C8[] c8))
                    {
                        for (int i = count - 1; i >= 0; --i)
                        {
                            action(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i], ref c6[i], ref c7[i], ref c8[i]);
                        }
                    }
                }
            }
        }
    }
}