using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public interface IDispatcher<Event> : DispatchManager.IDipatcher
{
    void OnDispatch(Event args);
}

[AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DispatchOrder : Attribute
{
    public int execOrder = 0;

    public DispatchOrder(int executionOrder = 0)
    {
        this.execOrder = executionOrder;
    }

    public static implicit operator int(DispatchOrder eventOrder)
        => eventOrder == null ? 0 : eventOrder.execOrder;
}

public static class DispatchManager
{
    public interface IDipatcher { }

    static DispatchManager()
    {
        List<(Type system, int order)> systems = new List<(Type, int order)>();
        foreach (var system in typeof(DispatchManager).Assembly.GetTypes())
        {
            if (typeof(IDipatcher).IsAssignableFrom(system))
            {
                systems.Add((system, system.GetCustomAttribute<DispatchOrder>()));
            }
        }
        systems.Sort((x, y) => x.order.CompareTo(y.order));
        foreach (var system in systems)
        {
            if (Singleton.TryGet(system.system, out var singleton))
                RegisterSystem(singleton);
        }
    }

    static Dictionary<Type, IEventHandler> Events = new Dictionary<Type, IEventHandler>();

    static void RegisterSystem(object obj)
    {
        foreach (var inf in obj.GetType().GetInterfaces())
        {
            if (inf.IsGenericType)
            {
                if (inf.GetGenericTypeDefinition() == typeof(IDispatcher<>))
                {
                    var arg = inf.GetGenericArguments()[0];
                    if (!Events.TryGetValue(arg, out IEventHandler handler))
                    {
                        var type = typeof(EventHandle<>).MakeGenericType(arg);
                        Events[arg] = handler = (IEventHandler)Activator.CreateInstance(type);
                    }
                    handler.Register(obj);
                }
            }
        }
    }

    interface IEventHandler
    {
        void Register(object obj);
        Type EventType { get; }
        System.Collections.IList GetEvents();
    }

    class EventHandle<E> : IEventHandler
    {
        public static List<IDispatcher<E>> events = new List<IDispatcher<E>>();

        public Type EventType => typeof(E);

        public IList GetEvents() => events;

        public void Register(object obj)
        {
            events.Add((IDispatcher<E>)obj);
        }
    }

    public static void Dispatch<E>(E args)
    {
        var events = EventHandle<E>.events;
        for (int i = 0; i < events.Count; ++i)
            events[i].OnDispatch(args);
    }

    class ShowEvents : ICommand, IUpdater
    {
        public bool show;

        public void OnCommand(ConsoleCommands.ConsoleArgs args)
        {
            show = !show;
            this.StartUpdates();
        }

        public bool Update(float delta)
        {
            foreach (var Event in Events)
            {
                string name = "";
                var args = Event.Key.GetGenericArguments();
                if (args.Length > 0)
                {
                    name = Event.Key.Name.Substring(0, Event.Key.Name.IndexOf('`'));
                    foreach (var type in args)
                    {
                        name += type.Name + " ";
                        name = name.AddSpaceBeforeCaps().ToUpper();
                    }
                }
                else
                {
                    name = Event.Key.Name.AddSpaceBeforeCaps().ToUpper();
                }

                Debug.Label(name);
                foreach (var dispatcher in Event.Value.GetEvents())
                    Debug.Label(dispatcher.GetType().Name.AddSpaceBeforeCaps());
                Debug.Label("");
            }
            return show;
        }
    }
}