using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private const float MIN_VOLUME_DB = -80.0f;
    private const float MAX_VOLUME_DB = 0.0f;

    private string CurrentSongFile => CurrentPlayer.Stream?.ResourcePath;

    public override void _Ready()
    {
        // Singleton pattern, prevents multiple instances from being created.
        if (Instance != null)
        {
            GD.PushWarning("[MusicManager] Music manager is already present. Deleting this instance.");
            QueueFree();
        }

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

    // public void PlayMusic(AudioStreamPlayer player, Song title)
    // {
    //     if (!_music.ContainsKey(title))
    //     {
    //         return;
    //     }
    //
    //     player.Stop();
    //     player.Stream = _music[title];
    //     player.Play();
    // }

    // public void StopMusic(AudioStreamPlayer player)
    // {
    //     player.Stop();
    // }

    // public void FadeOutPlayer(AudioStreamPlayer player)
    // {
    //     _fadeDuration = 5.0f;
    //
    //     _currentAudioTween?.Kill();
    //
    //     _currentAudioTween = CreateTween();
    //     _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
    //     _currentAudioTween.TweenProperty(player, "volume_db", -80f, _fadeDuration);
    // }

    // public void FadeInPlayer(AudioStreamPlayer player)
    // {
    //     _fadeDuration = 1.0f;
    //
    //     _currentAudioTween?.Kill();
    //
    //     _currentAudioTween = CreateTween();
    //     _currentAudioTween.SetTrans(Tween.TransitionType.Linear);
    //     _currentAudioTween.TweenProperty(player, "volume_db", 0f, _fadeDuration);
    // }

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

    /// <summary>
    ///  Starts playing the given <paramref name="song"/> immediately from the
    ///  beginning.
    /// </summary>
    ///
    /// <param name="song">The song to play.</param>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading the music in and out. This prevents the previously active
    ///  fading effect from changing the volume for the newly stated song.
    /// </remarks>
    public void PlaySong(Song song)
    {
        _currentAudioTween?.Kill();
        CurrentPlayer.Stream = _music[song];
        CurrentPlayer.VolumeDb = MAX_VOLUME_DB;
        CurrentPlayer.Play();
        GD.Print($"[MusicManager] Started playing '{CurrentSongFile}'.");
    }

    /// <summary>
    ///  Starts playing the given <paramref name="song"/> from the beginning
    ///  and fades it in.
    /// </summary>
    ///
    /// <param name="song">The song to play.</param>
    /// <param name="fadeDuration">
    ///  The duration of the fade in effect in seconds.
    /// </param>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading the music in and out. This prevents the previously active
    ///  fading effect from changing the volume for the newly stated song.
    /// </remarks>
    public async Task PlaySongWithFadeIn(Song song, float fadeDuration)
    {
        PlaySong(song);
        await FadeInCurrentSong(fadeDuration);
    }

    /// <summary>
    ///  Stops playing the current song immediately. This will also set the
    ///  playback head to the beginning of the song.
    /// </summary>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading the music in and out. This prevents executing fading
    ///  effects on the stopped player.
    /// </remarks>
    public void StopCurrentSong()
    {
        _currentAudioTween?.Kill();
        CurrentPlayer.Stop();
        GD.Print($"[MusicManager] Stopped playing '{CurrentSongFile}'.");
    }

    /// <summary>
    ///  Fades out the current song and stops playback. This will also set the
    ///  playback head back to the beginning of the song.
    /// </summary>
    ///
    /// <param name="fadeDuration">
    ///  The duration of the fade in effect in seconds.
    /// </param>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading the music in and out. This prevents executing fading
    ///  effects on the stopped player.
    /// </remarks>
    public async Task StopCurrentSongWithFadeOut(float fadeDuration)
    {
        await FadeOutCurrentSong(fadeDuration);
        StopCurrentSong();
    }

    /// <summary>
    ///  Pauses the playback of the current song.
    /// </summary>
    ///
    /// <remarks>
    ///  Calling this method will also pause the internal <see cref="Tween"/>
    ///  used by the current music player. I.e., this method will also pause
    ///  fade in/out effects.
    /// </remarks>
    public void PauseCurrentSong()
    {
        _currentAudioTween?.Pause();
        CurrentPlayer.StreamPaused = true;
        GD.Print($"[MusicManager] Pause '{CurrentSongFile}'.");
    }

    /// <summary>
    ///  Resumes the playback of the current song.
    /// </summary>
    public void ResumeCurrentSong()
    {
        _currentAudioTween?.Play();
        CurrentPlayer.StreamPaused = false;
        GD.Print($"[MusicManager] Resumed '{CurrentSongFile}'.");
    }

    /// <summary>
    ///  Fades in the currently played song. This can be used after
    /// </summary>
    ///
    /// <param name="fadeDuration">
    ///  The duration of the fade in effect in seconds.
    /// </param>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading in/out the currently playing music. I.e., if this method is
    ///  called while there already is fading in progress, that fade will be
    ///  canceled before the new fade in is started.
    /// </remarks>
    public async Task FadeInCurrentSong(float fadeDuration)
    {
        GD.Print($"[MusicManager] Fading IN '{CurrentSongFile}'.");
        _currentAudioTween?.Kill();
        _currentAudioTween = CurrentPlayer.CreateTween();
        await FadeIn(CurrentPlayer, fadeDuration, _currentAudioTween);
    }

    /// <summary>
    ///  Fades out the current song. This does not stop the playback.
    /// </summary>
    ///
    /// <param name="fadeDuration">
    ///  The duration of the fade out effect in seconds.
    /// </param>
    ///
    /// <remarks>
    ///  Calling this method will kill the internal <see cref="Tween"/> used
    ///  for fading in/out the currently playing music. I.e., if this method is
    ///  called while there already is fading in progress, that fade will be
    ///  canceled before the new fade out is started.
    /// </remarks>
    public async Task FadeOutCurrentSong(float fadeDuration)
    {
        GD.Print($"[MusicManager] Fading OUT '{CurrentSongFile}'.");
        _currentAudioTween?.Kill();
        _currentAudioTween = CurrentPlayer.CreateTween();
        await FadeOut(CurrentPlayer, fadeDuration, _currentAudioTween);
    }

    private async Task FadeIn(
        AudioStreamPlayer player,
        float duration,
        Tween tween,
        bool startFromCurrentVolume = false
    )
    {
        GD.Print($"[MusicManager] Fading IN player '{player.Name}' ({duration} s)");

        if (!startFromCurrentVolume)
        {
            player.VolumeDb = MIN_VOLUME_DB;
        }

        //var tween = player.CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(player, AudioStreamPlayer.PropertyName.VolumeDb.ToString(), MAX_VOLUME_DB, duration);
        await ToSignal(tween, Tween.SignalName.Finished);
        GD.Print("[MusicManager] Fade in finished.");
    }

    private async Task FadeOut(
        AudioStreamPlayer player,
        float duration,
        Tween tween,
        bool startFromCurrentVolume = false
    )
    {
        GD.Print($"[MusicManager] Fading OUT player '{player.Name}' ({duration} s)");

        if (!startFromCurrentVolume)
        {
            player.VolumeDb = MAX_VOLUME_DB;
        }

        //var tween = player.CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(player, AudioStreamPlayer.PropertyName.VolumeDb.ToString(), MIN_VOLUME_DB, duration);
        await ToSignal(tween, Tween.SignalName.Finished);
        //tween.Dispose();
        GD.Print("[MusicManager] Fade out finished.");
    }
}
