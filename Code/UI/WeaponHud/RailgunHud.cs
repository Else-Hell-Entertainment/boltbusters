// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class RailgunHud : Control
    {
        private PlayerRailgunController _railgunController;

        TextureRect[] _railgunIcons = new TextureRect[4];

        [ExportGroup("Node references")]
        [Export]
        private TextureRect RailgunIcon1;

        [Export]
        private TextureRect RailgunIcon2;

        [Export]
        private TextureRect RailgunIcon3;

        [Export]
        private TextureRect RailgunIcon4;

        public override void _Ready()
        {
            base._Ready();
            // With only 4 icons this is hardcoded for now (won't change).
            _railgunIcons[0] = RailgunIcon1;
            _railgunIcons[1] = RailgunIcon2;
            _railgunIcons[2] = RailgunIcon3;
            _railgunIcons[3] = RailgunIcon4;

            CallDeferred(MethodName.Initialize);
        }

        private void Initialize()
        {
            if (_railgunController != null)
            {
                ClearRailgunList();
                _railgunController.RailgunConfigurationChanged -= RefreshIcons;
            }
            _railgunController = LevelManager.Active.Player.RailgunController;
            _railgunController.RailgunConfigurationChanged += RefreshIcons;
            PopulateRailgunList();
        }

        private void PopulateRailgunList()
        {
            for (int i = 0; i < _railgunController.Weapons.Count; i++)
            {
                BaseWeapon wp = _railgunController.Weapons[i];
                if (wp is Railgun rg)
                {
                    rg.RailgunStateChanged += OnRailgunStateChanged;
                }
            }
        }

        private void ClearRailgunList()
        {
            for (int i = 0; i < _railgunController.Weapons.Count; i++)
            {
                BaseWeapon wp = _railgunController.Weapons[i];
                if (wp is Railgun rg)
                {
                    rg.RailgunStateChanged -= OnRailgunStateChanged;
                }
            }
        }

        private void OnRailgunStateChanged(int state) { }

        private void RefreshIcons()
        {
            int railgunCount = _railgunController.Weapons.Count;

            for (int i = 0; i < _railgunIcons.Length; i++)
            {
                if (i < railgunCount)
                {
                    _railgunIcons[i].Show();
                }
                else
                {
                    _railgunIcons[i].Hide();
                }
            }

            for (int i = 0; i < _railgunController.Weapons.Count; i++)
            {
                //_railgunIcons[i].Show();
            }
        }
    }
}
