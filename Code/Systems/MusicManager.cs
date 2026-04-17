// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miko Reinholm <miko.reinholm@tuni.fi>

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

    public AudioStreamPlayer MainThemePlayer { get; private set; }
    public AudioStreamPlayer EndThemePlayer { get; private set; }
    public AudioStreamPlayer StageThemePlayer1 { get; private set; }
    public AudioStreamPlayer StageThemePlayer2 { get; private set; }
    public AudioStreamPlayer StageThemePlayer3 { get; private set; }
    public AudioStreamPlayer StageThemePlayer4 { get; private set; }
    public AudioStreamPlayer NutCollectSFX;
    public AudioStreamPlayer BoltCollectSFX;
    public AudioStreamPlayer WrenchCollectSFX;

    private float _fadeDuration;
    private Tween _currentAudioTween;

    public override void _Ready()
    {
        Instance = this;
        MainThemePlayer = new AudioStreamPlayer();
        EndThemePlayer = new AudioStreamPlayer();
        StageThemePlayer1 = new AudioStreamPlayer();
        StageThemePlayer2 = new AudioStreamPlayer();
        StageThemePlayer3 = new AudioStreamPlayer();
        StageThemePlayer4 = new AudioStreamPlayer();
        NutCollectSFX = new AudioStreamPlayer();
        BoltCollectSFX = new AudioStreamPlayer();
        WrenchCollectSFX = new AudioStreamPlayer();
        AddChild(MainThemePlayer);
        AddChild(EndThemePlayer);
        AddChild(StageThemePlayer1);
        AddChild(StageThemePlayer2);
        AddChild(StageThemePlayer3);
        AddChild(StageThemePlayer4);
        AddChild(NutCollectSFX);
        AddChild(BoltCollectSFX);
        AddChild(WrenchCollectSFX);

        _music[Song.MainTheme] = GD.Load<AudioStream>("res://Assets/Music/MainTheme.ogg");
        _music[Song.EndTheme] = GD.Load<AudioStream>("res://Assets/Music/EndTheme.ogg");
        _music[Song.StageTheme1] = GD.Load<AudioStream>("res://Assets/Music/StageTheme1.ogg");
        _music[Song.StageTheme2] = GD.Load<AudioStream>("res://Assets/Music/StageTheme2.ogg");
        _music[Song.StageTheme3] = GD.Load<AudioStream>("res://Assets/Music/StageTheme3.ogg");
        _music[Song.StageTheme4] = GD.Load<AudioStream>("res://Assets/Music/StageTheme4.ogg");

        NutCollectSFX.Stream = GD.Load<AudioStream>("res://Assets/SFX/CollectibleSound1.ogg");
        BoltCollectSFX.Stream = GD.Load<AudioStream>("res://Assets/SFX/CollectibleSound2.ogg");
        WrenchCollectSFX.Stream = GD.Load<AudioStream>("res://Assets/SFX/CollectibleSound3.ogg");

        NutCollectSFX.VolumeDb = -14f;
        BoltCollectSFX.VolumeDb = -14f;
        WrenchCollectSFX.VolumeDb = -14f;
    }

    public void PlayMusic(AudioStreamPlayer player, Song title)
    {
        if (!_music.ContainsKey(title))
        {
            return;
        }

        player.Stop();
        player.VolumeDb = 0f;
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
        _currentAudioTween.TweenCallback(Callable.From(() => player.Stop()));
    }

    public void FadeInPlayer(AudioStreamPlayer player)
    {
        _fadeDuration = 1.0f;

        _currentAudioTween?.Kill();

        _currentAudioTween = CreateTween();
        _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
        _currentAudioTween.TweenProperty(player, "volume_db", 0f, _fadeDuration);
    }

    public void FadeToBackgroundLevel(AudioStreamPlayer player)
    {
        _fadeDuration = 2.0f;

        _currentAudioTween?.Kill();

        _currentAudioTween = CreateTween();
        _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
        _currentAudioTween.TweenProperty(player, "volume_db", -12f, _fadeDuration);
    }

    public void QuickFadeInPlayer(AudioStreamPlayer player)
    {
        _fadeDuration = 0.2f;

        _currentAudioTween?.Kill();

        _currentAudioTween = CreateTween();
        _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
        _currentAudioTween.TweenProperty(player, "volume_db", 0f, _fadeDuration);
    }

    public void KillAllMusic()
    {
        MainThemePlayer.Stop();
        EndThemePlayer.Stop();
        StageThemePlayer1.Stop();
        StageThemePlayer2.Stop();
        StageThemePlayer3.Stop();
        StageThemePlayer4.Stop();
    }

    public void PlayNutCollectibleSound()
    {
        NutCollectSFX.Play();
    }

    public void PlayBoltCollectibleSound()
    {
        BoltCollectSFX.Play();
    }

    public void PlayWrenchCollectibleSound()
    {
        WrenchCollectSFX.Play();
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
