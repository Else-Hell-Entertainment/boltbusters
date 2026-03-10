using System;
using System.Collections.Generic;
using Godot;

public partial class MusicManager : Node
{
    public static MusicManager Instance { get; private set; }

    // Used in place of lengthy file paths when handling music.
    public enum Song
    {
        MainTheme,
        EndTheme,
        StageTheme1,
        StageTheme2,
        StageTheme3,
        StageTheme4,
    }

    private Dictionary<Song, AudioStream> music = new();

    public AudioStreamPlayer CurrentPlayer;
    public AudioStreamPlayer NextPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        CurrentPlayer = new AudioStreamPlayer();
        NextPlayer = new AudioStreamPlayer();
        AddChild(CurrentPlayer);
        AddChild(NextPlayer);

        music[Song.MainTheme] = GD.Load<AudioStream>("res://Assets/Music/MainTheme.wav");
        music[Song.EndTheme] = GD.Load<AudioStream>("res://Assets/Music/EndTheme.wav");
        music[Song.StageTheme1] = GD.Load<AudioStream>("res://Assets/Music/StageTheme1.wav");
        music[Song.StageTheme2] = GD.Load<AudioStream>("res://Assets/Music/StageTheme2.wav");
        music[Song.StageTheme3] = GD.Load<AudioStream>("res://Assets/Music/StageTheme3.wav");
        music[Song.StageTheme4] = GD.Load<AudioStream>("res://Assets/Music/StageTheme4.wav");
    }

    public void PlayMusic(AudioStreamPlayer player, Song title)
    {
        if (!music.ContainsKey(title))
            return;

        player.Stop();
        player.Stream = music[title];
        player.Play();
    }

    public void StopMusic(AudioStreamPlayer player)
    {
        player.Stop();
    }

    public async void LinearCrossFade(AudioStreamPlayer from, AudioStreamPlayer to) { }
}
