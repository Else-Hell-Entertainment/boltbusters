// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Config;
using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuSettings : Menu
    {
        private AudioTab _audioTab;
        private VideoTab _videoTab;

        [Export]
        private Button _btnBack;

        /// <inheritdoc />
        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                OnBtnBackPressed();
            }
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            _btnBack.Pressed += OnBtnBackPressed;
        }

        public override void _ExitTree()
        {
            _btnBack.Pressed -= OnBtnBackPressed;
        }

        public override void _Ready()
        {
            base._Ready();

            _audioTab = this.GetFirstChildOfType<AudioTab>(recurse: true);
            _videoTab = this.GetFirstChildOfType<VideoTab>(recurse: true);

            var isValid = true;

            if (_audioTab == null)
            {
                this.LogError($"No suitable node for '{nameof(_audioTab)}' found in the scene!");
                isValid = false;
            }

            if (_videoTab == null)
            {
                this.LogError($"No suitable node for '{nameof(_videoTab)}' found in the scene!");
                isValid = false;
            }

            if (!isValid)
            {
                this.LogError("SettingsMenu encountered errors during Ready and cannot be loaded!");
                return;
            }

            _audioTab.Initialize();
            _videoTab.Initialize(GameManager.Instance.SettingsManager.DefaultSettingsData.VideoSettingsData);
        }

        private void OnBtnBackPressed()
        {
            MusicManager.Instance.ButtonSoundPlayer.Play();
            GameManager.Instance.SettingsManager.SaveSettings();
            GameManager.Instance.SettingsManager.SaveSettingsToFile(SettingsConfig.USER_SETTINGS_FILE_PATH);
            GameManager.Instance.StateMachine.TransitionToPrevious();
        }
    }
}
