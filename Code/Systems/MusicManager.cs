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

    private Dictionary<Song, AudioStream> _music = new();

    public AudioStreamPlayer CurrentPlayer { get; private set; }
    public AudioStreamPlayer NextPlayer { get; private set; }

    private float _fadeDuration;
    private Tween _currentAudioTween;

    public override void _Ready()
    {
        Instance = this;
        CurrentPlayer = new AudioStreamPlayer();
        NextPlayer = new AudioStreamPlayer();
        AddChild(CurrentPlayer);
        AddChild(NextPlayer);

        _music[Song.MainTheme] = GD.Load<AudioStream>("res://Assets/Music/MainTheme.ogg");
        _music[Song.EndTheme] = GD.Load<AudioStream>("res://Assets/Music/EndTheme.ogg");
        _music[Song.StageTheme1] = GD.Load<AudioStream>("res://Assets/Music/StageTheme1.ogg");
        _music[Song.StageTheme2] = GD.Load<AudioStream>("res://Assets/Music/StageTheme2.ogg");
        _music[Song.StageTheme3] = GD.Load<AudioStream>("res://Assets/Music/StageTheme3.ogg");
        _music[Song.StageTheme4] = GD.Load<AudioStream>("res://Assets/Music/StageTheme4.ogg");
    }

    public void PlayMusic(AudioStreamPlayer player, Song title)
    {
        if (!_music.ContainsKey(title))
        {
            return;
        }

        player.Stop();
        player.Stream = _music[title];
        player.Play();
    }

    public void StopMusic(AudioStreamPlayer player)
    {
        player.Stop();
    }

    public void FadeOutPlayer(AudioStreamPlayer player)
    {
        _fadeDuration = 5.0f;

        _currentAudioTween?.Kill();

        _currentAudioTween = CreateTween();
        _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
        _currentAudioTween.TweenProperty(player, "volume_db", -80f, _fadeDuration);
    }

    public void FadeInPlayer(AudioStreamPlayer player)
    {
        _fadeDuration = 1.0f;

        _currentAudioTween?.Kill();

        _currentAudioTween = CreateTween();
        _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
        _currentAudioTween.TweenProperty(player, "volume_db", 0f, _fadeDuration);
    }

    /* public async void LinearCrossFade(AudioStreamPlayer from, AudioStreamPlayer to)
    {
        float fadeDuration = 2.0f;

        var tweenFrom = CreateTween();
        tweenFrom.SetTrans(Tween.TransitionType.Linear);
        tweenFrom.TweenProperty(from, "volume_db", -80f, fadeDuration);

        var tweenTo = CreateTween();
        tweenTo.SetTrans(Tween.TransitionType.Linear);
        tweenTo.TweenProperty(to, "volume_db", 0f, fadeDuration);

        await ToSignal(tweenFrom, Tween.SignalName.Finished);
        await ToSignal(tweenTo, Tween.SignalName.Finished);

        from.Stop();
    } */
}
