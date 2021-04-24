using System.Collections;
using System.Collections.Generic;
using ConsoleCommands;

class Profiler : ICommand, IUpdater
{
    static Dictionary<string, System.TimeSpan> startTimes = new Dictionary<string, System.TimeSpan>();
    static Dictionary<string, System.TimeSpan> executionTimes = new Dictionary<string, System.TimeSpan>();

    /// <summary>
    /// Starts a snapshot with identifier
    /// </summary>
    public static void Start(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return;

        startTimes[identifier] = Time.timespan_since_startup;
    }
    
    /// <summary>
    /// Stores the time diff between now and the when you started the snapshot
    /// </summary>
    public static void Stop(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return;

        if (startTimes.TryGetValue(identifier, out var span))
        {
            executionTimes[identifier] = Time.timespan_since_startup - span;
        }
    }
    
    bool show;
    void ICommand.OnCommand(ConsoleArgs args)
    {
        show = !show;
        this.StartUpdates(true);
    }

    public bool Update(float delta)
    {
        Debug.Label("");
            Debug.Label("### PROFILER ###");
            var to_sort = new List<KeyValuePair<string, System.TimeSpan>>();
            foreach(var item in executionTimes)
            {
                to_sort.Add(item);
            }
            to_sort.Sort((y,x) => x.Value.CompareTo(y.Value));
            foreach(var item in to_sort)
            {
                Debug.Label(item.Key, $"{item.Value.TotalMilliseconds: 0.000}ms");
            }
            Debug.Label("");
        return show;
    }
}