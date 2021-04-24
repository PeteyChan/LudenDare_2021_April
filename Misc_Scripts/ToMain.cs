using ConsoleCommands;

class ToMain : ICommand
{
    public void OnCommand(ConsoleArgs args)
    {
        Scene.Load("res://Scenes/Main.tscn");
    }
}