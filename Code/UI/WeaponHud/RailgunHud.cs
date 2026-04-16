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
        private TextureRect _railgunIcon1;

        [Export]
        private TextureRect _railgunIcon2;

        [Export]
        private TextureRect _railgunIcon3;

        [Export]
        private TextureRect _railgunIcon4;

        public override void _Ready()
        {
            base._Ready();
            // With only 4 icons this is hardcoded for now (won't change).
            _railgunIcons[0] = _railgunIcon1;
            _railgunIcons[1] = _railgunIcon2;
            _railgunIcons[2] = _railgunIcon3;
            _railgunIcons[3] = _railgunIcon4;

            CallDeferred(MethodName.Initialize);
        }

        private void Initialize()
        {
            if (_railgunController != null)
            {
                ClearRailgunList();
                _railgunController.RailgunConfigurationChanged -= OnConfigurationChanged;
            }

            _railgunController = LevelManager.Active.Player.RailgunController;
            _railgunController.RailgunConfigurationChanged += OnConfigurationChanged;
            PopulateRailgunList();
            RefreshIcons();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (_railgunController != null)
            {
                ClearRailgunList();
                _railgunController.RailgunConfigurationChanged -= OnConfigurationChanged;
            }
        }

        private void OnConfigurationChanged()
        {
            ClearRailgunList();
            PopulateRailgunList();
            RefreshIcons();
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

        private void OnRailgunStateChanged(int state)
        {
            RefreshIcons();
        }

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

            int chargingCount = 0;
            int readyToFireCount = 0;

            for (int i = 0; i < _railgunController.Weapons.Count; i++)
            {
                Railgun rg = (Railgun)_railgunController.Weapons[i];
                if (rg.CurrentState == Railgun.RailgunState.Charging)
                {
                    chargingCount++;
                }
                else if (rg.CurrentState == Railgun.RailgunState.ReadyToFire)
                {
                    readyToFireCount++;
                }
            }

            for (int i = 0; i < readyToFireCount; i++)
            {
                SetIconStateReadyToFire(i);
            }

            for (int i = readyToFireCount; i < readyToFireCount + chargingCount; i++)
            {
                SetIconStateCharging(i);
            }
        }

        private void SetIconStateCharging(int iconIndex)
        {
            _railgunIcons[iconIndex].Modulate = new Color(1, 1, 1, 0.2f);
        }

        private void SetIconStateReadyToFire(int iconIndex)
        {
            _railgunIcons[iconIndex].Modulate = new Color(1, 1, 1, 1f);
        }
    }
}
