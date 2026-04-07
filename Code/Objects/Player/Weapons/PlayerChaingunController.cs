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
        [Export]
        private float _range = 7f;

        [Export]
        private AudioStreamPlayer3D _shootingAudio;

        [Export]
        private Node3D _aimPoint;

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

        private float _attackTimer;
        private float _attackInterval = 0.5f;
        private float _overheatLimit = 100;
        private float _currentHeat = 0;

        // TODO: Implement chainguns automatically adjusting to target. Currently hardcoded!

        private Sprite3D _reticle;

        /// <summary>
        /// Use only ReadyToFire, NotReadyToFire or Overheating states. Currently only used to update UI so other
        /// states are not required because they are transitional and do not persist over one frame.
        /// </summary>
        public ChaingunState CurrentPersistentState { get; private set; } = ChaingunState.None;

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
            BarrelCountChanged,
        }

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            if (_aimPoint != null)
            {
                LookAt(_aimPoint.GlobalPosition);
            }
            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
            CurrentPersistentState = ChaingunState.ReadyToFire;
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.ReadyToFire);
        }

        public override bool AddWeapon()
        {
            if (!base.AddWeapon())
            {
                return false;
            }
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.BarrelCountChanged);
            SetAttackInterval();
            return true;
        }

        public override bool RemoveWeapon()
        {
            if (!base.RemoveWeapon())
            {
                return false;
            }
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.BarrelCountChanged);
            SetAttackInterval();
            return true;
        }

        public override void Attack()
        {
            if (_attackTimer < _attackInterval)
                return;

            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack && CurrentPersistentState == ChaingunState.ReadyToFire)
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

        #region Heating mechanics

        /// <summary>
        /// Current heat level of the chaingun.
        /// </summary>
        /// <returns>Current heat level as float between 0 - 100.</returns>
        public float GetCurrentHeat()
        {
            return _currentHeat;
        }

        /// <summary>
        /// Adds one level of cooling upgrade (defined in code).
        /// </summary>
        public void UpgradeCooling()
        {
            CoolingRateUpgrade += _coolingUpgradeIncrease;
        }

        /// <summary>
        /// Removes one level of cooling upgrades. Total cooling can never drop below base value defined in code.
        /// </summary>
        public void DowngradeCooling()
        {
            CoolingRateUpgrade -= _coolingUpgradeIncrease;
            if (CoolingRateUpgrade < 0)
            {
                CoolingRateUpgrade = 0;
            }
        }

        /// <summary>
        /// Adds the indicated amount of heat to the chaingun. This is called during the attack and should be a one-time
        /// increase when the individual chaingun fires. It's possible to use as gradual increase if multiplied by
        /// delta on each frame (this is how cooling works). Can never go above overheatlimit and can trigger overheat.
        /// </summary>
        /// <param name="heatAmount">Value to be added to total heat.</param>
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

        /// <summary>
        /// Deducst the indicated amount from current heat. Can not go below zero. Typically called on each frame in
        /// Process for smooth cooling.
        /// </summary>
        /// <param name="heatAmount">Value to be deducted from total heat.</param>
        private void ReduceHeat(float heatAmount)
        {
            if (_currentHeat <= 0)
            {
                return;
            }

            _currentHeat -= heatAmount;
            _currentHeat = Mathf.Clamp(_currentHeat, 0, _overheatLimit);
            // Inform UI of heat change.
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.HeatChanged);
            // Handle case where weapon was overheating and has cooled down.
            if (CurrentPersistentState == ChaingunState.Overheat && _currentHeat < _overheatRecoveryThreshold)
            {
                CurrentPersistentState = ChaingunState.ReadyToFire;
                EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.ReadyToFire);
            }
        }

        /// <summary>
        /// Triggers the overheat event. Notifies UI and sets the current state to Overheat.
        /// </summary>
        private void TriggerOverheat()
        {
            CurrentPersistentState = ChaingunState.Overheat;
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.Overheat);
            EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.NotReadyToFire);
        }

        #endregion Heating mechanics

        public int GetBarrelCount()
        {
            return Weapons.Count;
        }

        public override void ResetWeapons()
        {
            base.ResetWeapons();
            ResetChaingun();
        }

        /// <summary>
        /// Resets the chaingun state to zero heat buildup, sets the state to ReadyToFire and notifies the UI.
        /// </summary>
        public void ResetChaingun()
        {
            CurrentPersistentState = ChaingunState.ReadyToFire;
            //EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.ReadyToFire);
            _currentHeat = 0;
            //EmitSignal(SignalName.ChaingunStateChanged, (int)ChaingunState.HeatChanged);
        }

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
