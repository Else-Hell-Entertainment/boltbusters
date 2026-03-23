// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Controls a group of player controlled chainguns of base type BaseWeapon. This controller is used to manage the
    /// chaingun's SFX.
    /// </summary>
    public partial class PlayerChaingunController : PlayerWeaponGroupController
    {
        [ExportGroup("Heat mechanics")]
        // How many units are increased to _current heat per shot.
        [Export]
        private float _heatBuildupRate = 0.2f;

        // Heat will be reduced every second by _baseCoolingRate + CoolingRateUpgrade value. Base is what the weapon
        // starts with and upgrades are bought during gameplay.
        [Export]
        private float _baseCoolingRate = 2f;

        // When cooling is upgraded, increase the CoolingRateUpgrade by this amount per upgrade.
        [Export]
        private float _coolingUpgradeIncrease = 1f;

        // After overheating the weapon must cool down below this level to be able to fire again.
        [Export]
        private float _overheatRecoveryThreshold = 80f;

        [Export]
        private float _range = 7f;

        [Export]
        private AudioStreamPlayer3D _shootingAudio;

        private float _attackTimer;
        private float _attackInterval = 0.5f;
        private float _overheatLimit = 100;
        private float _currentHeat = 0;

        // TODO: Implement chainguns automatically adjusting to target. Currently hardcoded!

        private Sprite3D _reticle;

        public ChaingunState CurrentState { get; private set; } = ChaingunState.None;

        public float CoolingRateUpgrade { get; private set; } = 0;

        public override WeaponType WeaponType => WeaponType.Chaingun;

        [Signal]
        public delegate void ChaingunStateChangedEventHandler(int state);

        public enum ChaingunState
        {
            None,
            ReadyToFire,
            NotReadyToFire,
            Firing,
            HeatChanged,
            Overheat,
        }

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            // AddWeapon();
            // AddWeapon();
            // AddWeapon();
            // AddWeapon();
            // AddWeapon();
            // AddWeapon();
            // AddWeapon();

            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
        }

        public override bool AddWeapon()
        {
            if (!base.AddWeapon())
            {
                return false;
            }
            SetAttackInterval();
            return true;
        }

        public override bool RemoveWeapon()
        {
            if (!base.RemoveWeapon())
            {
                return false;
            }
            SetAttackInterval();
            return true;
        }

        public override void Attack()
        {
            if (_attackTimer < _attackInterval)
                return;
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack)
                {
                    weapon.Attack();
                    EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.Firing);
                    AddHeat(_heatBuildupRate);
                    _attackTimer = 0;
                    if (!_shootingAudio.IsPlaying())
                    {
                        _shootingAudio.Play();
                    }
                    return;
                }
            }
        }

        public override void _Process(double delta)
        {
            float deltaTime = (float)delta;
            if (_attackTimer < _attackInterval)
            {
                _attackTimer += deltaTime;
            }
            ReduceHeat((_baseCoolingRate + CoolingRateUpgrade) * deltaTime);
        }

        public float GetCurrentHeat()
        {
            return _currentHeat;
        }

        public void UpgradeCooling()
        {
            CoolingRateUpgrade += _coolingUpgradeIncrease;
        }

        public void DowngradeCooling()
        {
            CoolingRateUpgrade -= _coolingUpgradeIncrease;
            if (CoolingRateUpgrade < 0)
            {
                CoolingRateUpgrade = 0;
            }
        }

        private void AddHeat(float heatAmount)
        {
            _currentHeat += heatAmount;
            if (_currentHeat > _overheatLimit)
            {
                TriggerOverheat();
            }
            _currentHeat = Mathf.Clamp(_currentHeat, 0, _overheatLimit);
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.HeatChanged);
        }

        private void ReduceHeat(float heatAmount)
        {
            if (_currentHeat <= 0)
            {
                return;
            }
            _currentHeat -= heatAmount;
            _currentHeat = Mathf.Clamp(_currentHeat, 0, _overheatLimit);
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.HeatChanged);
        }

        private void TriggerOverheat()
        {
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.Overheat);
        }

        private void ChangeState(ChaingunState newState)
        {
            switch (newState)
            {
                case ChaingunState.Firing:
                    break;
                case ChaingunState.ReadyToFire:
                    break;
                case ChaingunState.NotReadyToFire:
                    break;
                case ChaingunState.Overheat:
                    break;
                case ChaingunState.HeatChanged:
                    break;
                case ChaingunState.None:
                    break;
            }
        }

        # region Chaingun state change events

        private void HeatChanged() { }

        #endregion

        /// <summary>
        /// Sets the attack interval based on number of guns and the individual gun's cooldown to create a continuous
        /// firing effect while making sure the individual ROF is still accounted for. Example: if gun's cooldown is
        /// 0.5 seconds and there are 5 guns, interval will be 0.1 seconds so that every gun still fires when ready,
        /// but they don't all fire at once.
        /// </summary>
        private void SetAttackInterval()
        {
            float numberOfGuns = Weapons.Count;

            if (Weapons.Count > 0 && Weapons[0] is Chaingun chaingun)
            {
                float gunCooldown = chaingun.Cooldown;
                _attackInterval = gunCooldown / numberOfGuns; // Denominator is confirmed to be > 0.
            }
        }
    }
}
