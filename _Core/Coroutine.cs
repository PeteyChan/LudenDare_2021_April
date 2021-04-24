using Events;
using System.Collections;
using GameSystems;

namespace GameSystems
{
    class CoroutineManager :
        IDispatcher<Events.FrameUpdate>
    {
        static (IUpdater updater, bool continue_paused)[] updaters = new (IUpdater updater, bool continue_paused)[8];
        static int updater_count = 0;

        static (IEnumerator coroutine, bool continue_while_paused)[] coroutines = new (IEnumerator, bool continue_while_paused)[8];
        static int coroutine_count = 0;

        public static void StopAll()
        {
            coroutine_count = 0; 
        }

        public void OnDispatch(FrameUpdate args)
        {
            var paused = args.paused;
            
            {// updaters
                for (int i = updater_count - 1; i >= 0; --i)
                {
                    if (paused && !updaters[i].continue_paused)
                        continue;
                    if (!updaters[i].updater.Update(args.delta_time))
                    {
                        updater_count--;
                        updaters[i] = updaters[updater_count];
                        updaters[updater_count] = default;
                    }
                }
            }
            {// coroutine
                for (int i = coroutine_count - 1; i >= 0; --i)
                {
                    if (paused && !coroutines[i].continue_while_paused)
                        continue;
                    if (!coroutines[i].coroutine.MoveNext())
                    {
                        coroutine_count--;
                        coroutines[i] = coroutines[coroutine_count];
                        coroutines[coroutine_count] = default;
                    }
                }
            }
        }

        public static void StartCoroutine(IEnumerator coroutine, bool continue_while_paused)
        {
            coroutines[coroutine_count] = (coroutine, continue_while_paused);
            coroutine_count++;
            if (coroutine_count == coroutines.Length)
                System.Array.Resize(ref coroutines, coroutines.Length * 2);
        }

        public static void StartUpdater(IUpdater updater, bool continue_while_paused)
        {
            updaters[updater_count] = (updater, continue_while_paused);
            updater_count++;
            if (updater_count == updaters.Length)
                System.Array.Resize(ref updaters, updaters.Length * 2);
        }
    }
}

/// <summary>
/// Allows upating per a frame
/// </summary>
public interface IUpdater
{
    /// <summary>
    /// Return value is whether to keep updating
    /// </summary>
    bool Update(float delta);
}

public static partial class Extensions
{
    public static void StartUpdates(this IUpdater updater, bool continue_while_paused = false)
        => CoroutineManager.StartUpdater(updater, continue_while_paused);

    public static void StartCoroutine(this IEnumerator coroutine, bool continue_while_paused = false)
        => CoroutineManager.StartCoroutine(coroutine, continue_while_paused);
}