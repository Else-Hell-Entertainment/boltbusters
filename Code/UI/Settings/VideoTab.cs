// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Config;
using EHE.BoltBusters.Data;
using Godot;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    ///  Controls the behavior of the video tab in the settings menu.
    /// </summary>
    public partial class VideoTab : Control
    {
        private Vector2I _baseResolution = VideoSettingsConfig.BaseResolution;
        private int _maxResolutionMultiplier = 1;
        private int ResolutionMultiplier => _resolutionDropdown.Selected + 1;
        private int WindowId => (int)DisplayServer.MainWindowId;
        private Vector2I MaxScreenSize => DisplayServer.ScreenGetUsableRect().Size;

        [Export]
        private OptionButton _resolutionDropdown;

        [Export]
        private CheckButton _fullscreenToggle;

        /// <summary>
        ///  Initializes the video settings tab with the given data.
        /// </summary>
        ///
        /// <param name="data">
        ///  Data object containing video settings.
        /// </param>
        public void Initialize(VideoSettingsData data)
        {
            _baseResolution = data.BaseResolution;
            SetupResolutionDropdown();
            SetupFullscreenToggle();
        }

        public override void _EnterTree()
        {
            _resolutionDropdown.ItemSelected += OnResolutionDropdownItemSelected;
            _fullscreenToggle.Toggled += OnFullscreenToggleToggled;
        }

        public override void _ExitTree()
        {
            _resolutionDropdown.ItemSelected -= OnResolutionDropdownItemSelected;
            _fullscreenToggle.Toggled -= OnFullscreenToggleToggled;
        }

        private void ListPossibleResolutions()
        {
            var maxWidth = MaxScreenSize.X;
            var maxHeight = MaxScreenSize.Y;
            var maxResolution = _maxResolutionMultiplier * _baseResolution;

            _resolutionDropdown.Clear();

            do
            {
                _resolutionDropdown.AddItem($"{maxResolution.X}x{maxResolution.Y}");
                _maxResolutionMultiplier++;
                maxResolution = _maxResolutionMultiplier * _baseResolution;
            } while (maxResolution.X < maxWidth && maxResolution.Y < maxHeight);
        }

        private int GetResolutionMultiplierIndex(int resolutionMultiplier)
        {
            var minMultiplierIndex = 0;
            var maxMultiplierIndex = _resolutionDropdown.ItemCount - 1;
            var multiplierIndex = resolutionMultiplier - 1;

            if (multiplierIndex < minMultiplierIndex)
            {
                GD.PrintErr(
                    $"Current multiplier index ({multiplierIndex}) is too low,"
                        + $"using default ({minMultiplierIndex})."
                );
                return minMultiplierIndex;
            }

            if (multiplierIndex > maxMultiplierIndex)
            {
                GD.PrintErr(
                    $"Current multiplier index ({multiplierIndex}) is too high,"
                        + $"using default ({minMultiplierIndex})."
                );
                return maxMultiplierIndex;
            }

            return multiplierIndex;
        }

        private void SetupResolutionDropdown()
        {
            ListPossibleResolutions();

            var currentResolution = DisplayServer.WindowGetSize();
            var currentMultiplier = currentResolution.X / _baseResolution.X;
            var currentMultiplierIndex = GetResolutionMultiplierIndex(currentMultiplier);
            _resolutionDropdown.Select(currentMultiplierIndex);
        }

        private bool SetupFullscreenToggle()
        {
            _fullscreenToggle.ButtonPressed =
                DisplayServer.WindowGetMode(WindowId) == DisplayServer.WindowMode.Fullscreen;
            return true;
        }

        private void ApplyResolution()
        {
            DisplayServer.WindowSetSize(ResolutionMultiplier * _baseResolution, WindowId);
        }

        private void ApplyFullscreenState(bool enabled)
        {
            var windowMode = enabled ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
            _resolutionDropdown.Disabled = enabled;
            DisplayServer.WindowSetMode(windowMode, WindowId);
        }

        private void OnResolutionDropdownItemSelected(long index)
        {
            ApplyResolution();
        }

        private void OnFullscreenToggleToggled(bool toggledOn)
        {
            ApplyFullscreenState(toggledOn);
        }
    }
}
