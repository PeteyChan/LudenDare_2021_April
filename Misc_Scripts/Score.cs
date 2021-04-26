using ConsoleCommands;
using Godot;
using System;
using System.Collections.Generic;


public static class Score
{
    static string path = "user://save.dat";
    static Score()
    {
        var file = new File();
        if (file.FileExists(path) && Error.Ok == file.Open(path, File.ModeFlags.Read))
        {
            var data = file.GetAsText();

            var items = data.Split(',');
            
            for(int i = 0; i< items.Length; i+=2)
            {
                var name = items[i];
                if (i+1 >= items.Length)
                    break;
                int.TryParse(items[i+1].Replace("*", ""), out int score);

                if (name.Contains("*"))
                    top_score_hardcore.Add((score, name.Replace("*", "")));
                else top_score_standard.Add((score, name));   
            }
            
        }
        file.Close();
    }

    public static int Current = 0;

    public static List<(int score, string name)> top_score_standard = new List<(int, string)>();
    public static List<(int score, string name)> top_score_hardcore = new List<(int, string)>();

    public static bool canEnterScore(int score)
    {
        if (Player.Hardcore)
        {
            if (top_score_hardcore.Count < 10) return true;
            foreach(var value in top_score_hardcore)
            {
                if (score > value.score)
                    return true;
            }
            return false;
        }
        else
        {
            if (top_score_standard.Count < 10) return true;
            foreach(var value in top_score_standard)
            {
                if (score > value.score)
                    return true;
            }
            return false;
        }
    }

    public static void Submit(string name, int score)
    {
        List<(int score, string name)> table = Player.Hardcore ? top_score_hardcore : top_score_standard;

        table.Add((score, name));
        table.Sort( (x,y) => y.score.CompareTo(x.score));

        if (table.Count > 10)
            table.RemoveAt(table.Count-1);

        var file = new File();
        if (Error.Ok == file.Open(path, File.ModeFlags.Write))
        {
            string value = "";

            foreach(var item in top_score_standard)
            {
                value += item.name;
                value += ",";
                value += item.score.ToString();
                value += ",";
            }
            foreach(var item in top_score_hardcore)
            {
                value += "*" + item.name;
                value += ",";
                value += "*" + item.score;
                value += ",";
            }
            file.StoreString(value);
        }
        file.Close();
    }

    class SubmitScore : ICommand
    {
        public void OnCommand(ConsoleArgs args)
        {
            string name = args.ToString(0);
            int score = args.ToInt(1);

            if (name == "")
                name = "No Name";
            Submit(name, score);
        }
    }
}
