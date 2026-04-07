// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using EHE.BoltBusters.Config;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters.Data
{
    /// <summary>
    ///  Settings resource that holds video-related settings such as
    ///  resolution and fullscreen mode. Requires the
    ///  <see cref="SettingsConfig.Video"/> config module.
    /// </summary>
    [GlobalClass]
    public partial class VideoSettingsData : SettingsResource<VideoSettingsData>
    {
        [Export]
        private int _resolutionMultiplier = 1;

        [Export]
        private bool _isFullscreen = false;

        /// <summary>
        ///  Base resolution of the game as defined in the project settings.
        ///  Equivalent to <c>Video.BaseResolution</c>.
        /// </summary>
        public Vector2I BaseResolution => SettingsConfig.Video.BaseResolution;

        /// <summary>
        ///  Maximum allowed resolution multiplier based on the current
        ///  screen size. Determined by the largest integer multiplier
        ///  that keeps both width and height within the screen dimensions.
        /// </summary>
        public int MaxResolutionMultiplier =>
            Math.Min(
                DisplayServer.ScreenGetSize().X / BaseResolution.X,
                DisplayServer.ScreenGetSize().Y / BaseResolution.Y
            );

        /// <summary>
        ///  Actual rendering resolution, calculated by multiplying the base
        ///  resolution by the <see cref="ResolutionMultiplier"/>.
        /// </summary>
        public Vector2I Resolution => ResolutionMultiplier * BaseResolution;

        /// <summary>
        ///  The resolution multiplier applied to the base resolution to
        ///  determine the actual rendering <see cref="Resolution"/>.
        /// </summary>
        public int ResolutionMultiplier
        {
            get => _resolutionMultiplier;
            set => _resolutionMultiplier = Math.Clamp(value, 1, MaxResolutionMultiplier);
        }

        /// <summary>
        ///  Indicates whether the game is in fullscreen mode (borderless
        ///  window) or not.
        /// </summary>
        public bool IsFullscreen
        {
            get => _isFullscreen;
            set => _isFullscreen = value;
        }

        public override void Load(Dictionary data, VideoSettingsData defaults = null)
        {
            GD.Print("VideoSettingsData] Loading from data dictionary.");

            if (defaults == null)
            {
                GD.Print("VideoSettingsData] No defaults provided, using new instance as fallback.");
                defaults = new VideoSettingsData();
            }

            ResolutionMultiplier = data.TryGetValue(SettingsConfig.Video.KeyResolutionMultiplier, out var multiplier)
                ? (int)multiplier
                : defaults.ResolutionMultiplier;

            IsFullscreen = data.TryGetValue(SettingsConfig.Video.KeyIsFullscreen, out var isFullscreen)
                ? (bool)isFullscreen
                : defaults.IsFullscreen;
        }

        /// <summary>
        ///  Retrieves the current video settings from the
        ///  <see cref="DisplayServer"/> and stores them in this instance.
        /// </summary>
        public override void StoreValues()
        {
            // Written with GitHub Copilot auto-complete assistance.
            const int windowId = (int)DisplayServer.MainWindowId;
            ResolutionMultiplier = DisplayServer.WindowGetSize(windowId).X / BaseResolution.X;
            IsFullscreen = DisplayServer.WindowGetMode(windowId) == DisplayServer.WindowMode.Fullscreen;
        }

        /// <summary>
        ///  Applies the video settings stored in this instance to the
        ///  <see cref="DisplayServer"/>.
        /// </summary>
        public override void ApplyValues()
        {
            const int windowId = (int)DisplayServer.MainWindowId;
            var windowMode = IsFullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
            DisplayServer.WindowSetSize(Resolution, windowId);
            DisplayServer.WindowSetMode(windowMode, windowId);
        }

        /// <summary>
        ///  Resets the video settings to the default values specified in the
        ///  config module <see cref="SettingsConfig.Video"/>.
        /// </summary>
        public override void ResetValues()
        {
            ResolutionMultiplier = SettingsConfig.Video.DefaultResolutionMultiplier;
            IsFullscreen = SettingsConfig.Video.DefaultIsFullscreen;
            ApplyValues();
        }

        public override Dictionary Serialize()
        {
            Dictionary saveDict = new();
            saveDict.Add("ResolutionMultiplier", _resolutionMultiplier);
            saveDict.Add("FullscreenMode", _isFullscreen);
            return saveDict;
        }
    }
}
