// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Second iteration of the railgun. This version uses the controller as the actual weapon and each individual
    /// "railgun" is a powerbank that contains a single charge. Adding more railguns means adding additional power
    /// banks.
    /// </summary>
    public partial class Railgun2 : BaseWeapon
    {
        [Export]
        private Timer _reloadTimer;

        [Export]
        private float _reloadTime = 5f;

        /// <summary>
        /// How many % per second the charge meter fills up when player keeps fire button pressed.
        /// </summary>
        [Export]
        private float _baseChargingRate = 50f;

        /// <summary>
        /// How much the base charging rate is increased per bought upgrade.
        /// </summary>
        [Export]
        private float _chargingUpgradeIncrease = 5f;

        /// <summary>
        /// How many % per second the charge meter drops if the weapon is not charging. Both after firing or if the
        /// player cancels the charging action. This value should be several hundred as the discharges shouldn't last
        /// more than a fraction of a second.
        /// </summary>
        [Export]
        private float _dischargeRate = 400f;

        public bool IsActive { get; set; }

        public bool ChargeReady => CurrentChargePercent >= 100f;

        public float CurrentChargePercent { get; private set; }

        public int ChargingUpgradeCount = 0;

        public RailgunState CurrentState { get; private set; }

        public enum RailgunState
        {
            None,
            ReadyToFire,
            Reloading,
            Charging,
            Discharging,
        }

        [Signal]
        public delegate void RailgunStateChangedEventHandler(int state);

        public override void _Ready()
        {
            _reloadTimer = GetNode<Timer>("ReloadTimer");
            _reloadTimer.OneShot = true;
            _reloadTimer.WaitTime = _reloadTime;
            _reloadTimer.Timeout += OnReloadTimerTimeOut;
            ChangeState(RailgunState.ReadyToFire);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            ProcessCurrentState();
        }

        /// <summary>
        /// Actual attack mechanic is handled in RailgunController. This method will ensure that the charging process
        /// starts when attack is initiated.
        /// </summary>
        public override void Attack()
        {
            base.Attack();
            if (CurrentState is RailgunState.ReadyToFire)
            {
                ChangeState(RailgunState.Charging);
            }
        }

        /// <summary>
        /// Begins the discharging process.Discharge needs to be initiated manually to keep railgun behaviour control
        /// within the controller.
        /// The railgun must then reload again after it's finished (handled automatically).
        /// </summary>
        public void Discharge()
        {
            ChangeState(RailgunState.Discharging);
        }

        /// <summary>
        /// Resets the railgun into it's neutral state. Used between rounds.
        /// </summary>
        public void ResetState()
        {
            ChangeState(RailgunState.ReadyToFire);
            ResetCharge();
            _reloadTimer.Stop();
        }

        public void UpgradeChargingSpeed() { }

        public void DowngradeChargingSpeed() { }

        private void IncreaseCharge()
        {
            float deltaTime = (float)GetProcessDeltaTime();
            CurrentChargePercent += (_baseChargingRate + ChargingUpgradeCount * _chargingUpgradeIncrease) * deltaTime;
            CurrentChargePercent = Mathf.Clamp(CurrentChargePercent, 0f, 100f);
            EmitSignal(SignalName.RailgunStateChanged, (int)RailgunState.Charging);
        }

        private void DecreaseCharge()
        {
            float deltaTime = (float)GetProcessDeltaTime();
            CurrentChargePercent -= _dischargeRate * deltaTime;
            if (CurrentChargePercent < 0f)
            {
                CurrentChargePercent = 0f;
                // Weapon was discharging and is now at zero -> change state is required to start cooldown.
                if (CurrentState == RailgunState.Discharging)
                {
                    _reloadTimer.Start();
                    ChangeState(RailgunState.Reloading);
                }
            }
            EmitSignal(SignalName.RailgunStateChanged, (int)RailgunState.Discharging);
        }

        private void ResetCharge()
        {
            CurrentChargePercent = 0;
        }

        private void OnReloadTimerTimeOut()
        {
            ChangeState(RailgunState.ReadyToFire);
        }

        private void ChangeState(RailgunState newState)
        {
            CurrentState = newState;
            EmitSignal(SignalName.RailgunStateChanged, (int)newState);
        }

        private void ProcessCurrentState()
        {
            switch (CurrentState)
            {
                case RailgunState.ReadyToFire:
                    break;
                case RailgunState.Reloading:
                    break;
                case RailgunState.Charging:
                    IncreaseCharge();
                    break;
                case RailgunState.Discharging:
                    DecreaseCharge();
                    break;
            }
        }
    }
}
