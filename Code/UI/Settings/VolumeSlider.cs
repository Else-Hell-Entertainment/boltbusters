// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//
// Modified from original implementation by Sami Kojo under 3-Clause BSD License.
// Source: https://github.com/samikojo-tuni/GArkanoid-2025/blob/8ce89d9e5dea3cdaa829bfdf62549168046435db/Code/UI/UIAudioControl.cs
// License: https://github.com/samikojo-tuni/GArkanoid-2025/blob/8ce89d9e5dea3cdaa829bfdf62549168046435db/LICENSE

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class VolumeSlider : Control
    {
        private string _busName;

        /// <summary>
        ///  Emitted when the volume is changed.
        /// </summary>
        ///
        /// <param name="busName">
        ///  Name of the audio bus associated with the slider.
        /// </param>
        /// <param name="volumeDb">
        ///  New volume as a decibel value.
        /// </param>
        [Signal]
        public delegate void VolumeChangedEventHandler(string busName, float volumeDb);

        [Export]
        private Label _titleLabel;

        [Export]
        private Label _valueLabel;

        [Export]
        private Slider _slider;

        public override void _EnterTree()
        {
            _slider.ValueChanged += OnSliderValueChanged;
        }

        public override void _ExitTree()
        {
            _slider.ValueChanged -= OnSliderValueChanged;
        }

        /// <summary>
        ///  Initializes the volume slider.
        /// </summary>
        ///
        /// <param name="busName">Name of the associated audio bus.</param>
        /// <param name="displayName">The text visible in the UI.</param>
        /// <param name="volumeDb">Initial volume as decibel value.</param>
        public void Setup(string busName, string displayName, float volumeDb)
        {
            _busName = busName;
            _titleLabel.Text = displayName;
            SetVolume(volumeDb);
        }

        /// <summary>
        ///  Sets the value of the slider.
        /// </summary>
        ///
        /// <param name="volumeDb">Volume as decibel value.</param>
        private void SetVolume(float volumeDb)
        {
            var linearVolume = Mathf.DbToLinear(volumeDb);
            _slider.Value = linearVolume;
        }

        /// <summary>
        ///  Updates the value label text and emits a signal telling the volume
        ///  has changed.
        /// </summary>
        private void UpdateVolume()
        {
            var linearVolume = (float)_slider.Value;
            var volumeDb = Mathf.LinearToDb(linearVolume);
            _valueLabel.Text = $"{(int)(linearVolume * 100)} %";
            EmitSignal(SignalName.VolumeChanged, _busName, volumeDb);
        }

        /// <summary>
        ///  Updates the volume when the value of the slider changes.
        /// </summary>
        ///
        /// <param name="value">New slider value, not used.</param>
        private void OnSliderValueChanged(double value)
        {
            UpdateVolume();
        }
    }
}
