using System;
using System.IO;


[Serializable]
public class Playlist
{
    public string Filepath { get; set; }
    public string DisplayName { get; set; }


    public Playlist() { }

    public Playlist(string filepath)
    {
        Filepath = filepath;
        DisplayName = Path.GetFileName(filepath);
    }

    public override string ToString()
    {
        return DisplayName;
    }
}