using Events;

[DispatchOrder(int.MinValue)]
public class Scene : IDispatcher<Events.Bootstrap>
{
    public static Godot.SceneTree Tree {get; private set;}
        
    static bool locked = false;

    public static Godot.Node Current {
        get => Tree.CurrentScene;
        set
        {
            if (locked)
            {
                Debug.Log("CAN'T CHANGE CHANGE SCENES WHILE CHANGING SCENES");
                return;
            }

            if (Godot.Node.IsInstanceValid(value))
            {
                locked = true;
                DispatchManager.Dispatch(new Events.BeforeSceneChange());
                Tree.Root.AddChild(value);
                Tree.CurrentScene.QueueFree();
                Tree.CurrentScene = value;
                locked = false;
                DispatchManager.Dispatch(new Events.AfterSceneChange());
            }
        }
    }

    public static void Load(string path)
    {
        var res = Godot.GD.Load(path) as Godot.PackedScene;
        if (res == null)
            return;
        Current = res.Instance();
    }
    
    public void OnDispatch(Bootstrap args)
    {
        Tree = args.GetTree();
    }
}

namespace Events
{
    struct BeforeSceneChange{}
    struct AfterSceneChange{}
}