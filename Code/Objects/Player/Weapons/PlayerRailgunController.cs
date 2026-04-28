// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    public partial class PlayerRailgunController : PlayerWeaponGroupController
    {
        [Export]
        private float _chargeEmissionModifier = 1.0f;

        [Export]
        private float _chargeBeamBaseRadius = 0.5f;

        [Export]
        private float _chargeBeamWidthModifier = 1.0f;

        [Export]
        private AnimationPlayer _animationPlayer;

        [Export]
        private PackedScene _railgunSparkEffect;

        [ExportGroup("Sound settings")]
        [Export]
        private AudioStreamPlayer3D _shootingSound;

        [Export]
        private AudioStreamPlayer3D _chargingSound;

        [Export]
        private float _chargeSoundPitchTarget = 0.5f;

        [Export]
        private float _chargePitchWindDownTime = 0.5f;

        [Export]
        private float _chargePitchWindDownTarget = 0.1f;

        [Export]
        private float _chargeVolumeWindDownTime = 1f;

        [Export]
        private int _maxSecondaryUpgrades = 4;

        public override WeaponType WeaponType => WeaponType.Railgun;

        public bool IsActive = true;

        private const int COLLISION_MASK_LAYER = 2;
        private const string SHOOT_ANIMATION_NAME = "Shoot";

        private Railgun _activeRailgun;
        private Node3D _muzzle;
        private ShapeCast3D _shapeCast3D;
        private DamageData _damageData;

        private MeshInstance3D _laserSightInstance;
        private CylinderMesh _laserSightMesh;

        private MeshInstance3D _chargeEffectInstanceBeam;
        private CapsuleMesh _chargeEffectMeshBeam;
        private StandardMaterial3D _chargeEffectMaterialBeam;

        private bool _isChargeWindingDown;
        private float _shootingSoundLevel;
        private float _chargeSoundLevel;

        private bool _isAttackPressed;
        private int _physFramesCounter;
        private int _attackFramesCounter;
        private Dictionary _lastRaycastResult = new Dictionary();

        [Signal]
        public delegate void RailgunConfigurationChangedEventHandler();

        public override void _Ready()
        {
            base._Ready();
            InitializeNodes();
            _damageData = new DamageData(150, DamageType.Sniper);
            _shootingSoundLevel = _shootingSound.VolumeDb;
            _chargeSoundLevel = _chargingSound.VolumeDb;
        }

        public override void _Process(double delta)
        {
            if (IsActive && _activeRailgun != null)
            {
                if (_activeRailgun.CurrentState == Railgun.RailgunState.Charging)
                {
                    _laserSightInstance.Hide();
                    _chargeEffectInstanceBeam.Show();
                    UpdateChargeEffect();
                    _isChargeWindingDown = false;
                }
                else
                {
                    if (_chargingSound.IsPlaying() && !_isChargeWindingDown)
                    {
                        WindDownCharge();
                    }
                    _isChargeWindingDown = true;
                    _chargeEffectInstanceBeam.Hide();
                    _laserSightInstance.Show();
                    UpdateLaserSight();
                }
            }
        }

        private void WindDownCharge()
        {
            Tween chargeTween = CreateTween();
            chargeTween.TweenProperty(_chargingSound, "volume_db", -80, _chargeVolumeWindDownTime);
            chargeTween
                .Parallel()
                .TweenProperty(_chargingSound, "pitch_scale", _chargePitchWindDownTarget, _chargePitchWindDownTime);
            chargeTween.TweenCallback(Callable.From(ResetChargeSound));
        }

        private void ResetChargeSound()
        {
            _chargingSound.Stop();
            _chargingSound.PitchScale = 1.0f;
            _chargingSound.VolumeDb = _chargeSoundLevel;
            _isChargeWindingDown = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (IsActive)
            {
                _lastRaycastResult = RayCastForward();
            }

            if (_isAttackPressed)
            {
                _physFramesCounter++;
                if (_physFramesCounter - _attackFramesCounter > 1)
                {
                    _isAttackPressed = false;
                    _physFramesCounter = 0;
                    _attackFramesCounter = 0;
                    _activeRailgun?.Discharge();
                }
            }
        }

        public override bool AddWeapon()
        {
            if (base.AddWeapon())
            {
                SetNextActiveRailgun();
                EmitSignal(SignalName.RailgunConfigurationChanged);
                return true;
            }

            return false;
        }

        public override bool RemoveWeapon()
        {
            if (base.RemoveWeapon())
            {
                SetNextActiveRailgun();
                EmitSignal(SignalName.RailgunConfigurationChanged);
                return true;
            }

            return false;
        }

        public bool UpgradeChargeSpeed()
        {
            if (SecondaryUpgradeCount >= _maxSecondaryUpgrades)
            {
                GD.Print("Cannot upgrade railgun further.");
                return false;
            }

            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun railgun)
                {
                    railgun.UpgradeChargingSpeed();
                    EmitSignal(SignalName.RailgunConfigurationChanged);
                }
            }

            SecondaryUpgradeCount++;

            var playerData = GameManager.Instance.CurrentPlayerData;
            if (playerData == null)
            {
                this.LogWarning("Current player data is null.");
            }
            else
            {
                playerData.SetSecondaryUpgradeCount(WeaponType, SecondaryUpgradeCount);
            }

            return true;
        }

        public bool DowngradeChargeSpeed()
        {
            if (SecondaryUpgradeCount <= 0)
            {
                GD.Print("Cannot downgrade railgun further.");
                return false;
            }
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun railgun)
                {
                    railgun.DowngradeChargingSpeed();
                    EmitSignal(SignalName.RailgunConfigurationChanged);
                }
            }

            SecondaryUpgradeCount--;

            var playerData = GameManager.Instance.CurrentPlayerData;
            if (playerData == null)
            {
                this.LogWarning("Current player data is null.");
            }
            else
            {
                playerData.SetSecondaryUpgradeCount(WeaponType, SecondaryUpgradeCount);
            }

            return true;
        }

        private void InitializeNodes()
        {
            _muzzle = GetNode<Node3D>("Muzzle");
            _shapeCast3D = GetNode<ShapeCast3D>("ShapeCast3D");
            _shapeCast3D.CollisionMask = COLLISION_MASK_LAYER;
            _laserSightInstance = GetNode<MeshInstance3D>("LaserSight");
            _laserSightMesh = (CylinderMesh)_laserSightInstance.Mesh;
            _chargeEffectInstanceBeam = GetNode<MeshInstance3D>("ChargeEffectBeam");
            _chargeEffectMeshBeam = (CapsuleMesh)_chargeEffectInstanceBeam.Mesh;
            _chargeEffectMaterialBeam = (StandardMaterial3D)_chargeEffectMeshBeam.GetMaterial();
            _chargeEffectInstanceBeam.Hide();
        }

        /// <summary>
        /// Intended attack pattern:
        /// - If currently active railgun is in Discharging state, player must wait for it to
        /// finish. They must then click again, holding attack down does not initiate the next attack.
        /// - If currently active is ready to fire, start the charging process.
        /// - If player releases attack and cancels, the railgun must discharge before the next one can start firing
        /// and player must click attack again.
        /// - Keeping attack button pressed should never fire automatically in sequence, player must always click again.
        /// </summary>
        public override void Attack()
        {
            // Null check at the beginning in case the list hasn't been refreshed.
            if (_activeRailgun == null)
            {
                if (!SetNextActiveRailgun()) //If false, there's no active railgun -> abort.
                {
                    return;
                }
            }

            // New attack input: method will return true only if there's a new, ready to fire railgun which can start
            // attacking. Otherwise treat as if input didn't happen.
            if (!_isAttackPressed && !HandleJustPressedAttackInput())
            {
                return;
            }

            _isAttackPressed = true;
            _attackFramesCounter++;

            // Player must always wait for active railgun to finish discharging before they can attack again. Rider
            // complains about possible null reference exception but that's incorrect, it's checked earlier inside a
            // method.
            if (_activeRailgun.CurrentState == Railgun.RailgunState.Discharging)
            {
                return;
            }

            if (_activeRailgun.CurrentState == Railgun.RailgunState.ReadyToFire)
            {
                _activeRailgun.Attack();
            }

            if (_activeRailgun.ChargeReady)
            {
                _activeRailgun.Discharge();
                ShootRailgun();
                _shootingSound.Play();
                _isAttackPressed = false;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        ///  <see cref="UpgradeType.Primary"/> adds more weapons to this
        ///  controller. <see cref="UpgradeType.Secondary"/> upgrades the
        ///  charge speed.
        /// </remarks>
        public override bool Upgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Primary:
                    return AddWeapon();
                case UpgradeType.Secondary:
                    return UpgradeChargeSpeed();
                default:
                    this.LogWarning("Unknown upgrade type.");
                    break;
            }

            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        ///  <see cref="UpgradeType.Primary"/> adds more weapons to this
        ///  controller. <see cref="UpgradeType.Secondary"/> downgrades the
        ///  charge speed.
        /// </remarks>
        public override bool Downgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Primary:
                    return RemoveWeapon();
                case UpgradeType.Secondary:
                    return DowngradeChargeSpeed();
                default:
                    this.LogWarning("Unknown upgrade type.");
                    break;
            }

            return false;
        }

        /// <summary>
        /// Custom method to handle what happens when player has released the attack input and clicks it again.
        /// This is complicated but purely for game feel purposes.
        /// NOTE: does not ensure the _activeRailgun is not null!
        /// </summary>
        /// <returns><c>true</c> if active railgun was set and started to fire, <c>false</c> otherwise.</returns>
        private bool HandleJustPressedAttackInput()
        {
            _isAttackPressed = true;
            _attackFramesCounter++;
            // Player must always wait for the discharge to finish before they can attempt to shoot again.
            if (_activeRailgun.CurrentState == Railgun.RailgunState.Discharging)
            {
                return false;
            }

            if (_activeRailgun.CurrentState == Railgun.RailgunState.ReadyToFire)
            {
                _activeRailgun.Attack();
                return true;
            }

            if (SetNextActiveRailgun())
            {
                _activeRailgun.Attack();
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the shooting process of the railgun.
        /// </summary>
        private void ShootRailgun()
        {
            _animationPlayer.Play(SHOOT_ANIMATION_NAME);
            var collisions = _shapeCast3D.CollisionResult;
            foreach (Dictionary collision in collisions)
            {
                if (collision.ContainsKey("collider"))
                {
                    var collider = collision["collider"];
                    Node target = (Node)collider;
                    if (target is IDamageable damageable)
                    {
                        damageable.TakeDamage(_damageData);
                        Vector3 position = (Vector3)collision["point"];
                        Vector3 normal = (Vector3)collision["normal"];
                        GenerateSparks(position, -normal);
                    }
                }
            }
        }

        /// <summary>
        /// Goes through all railguns and if one is found that is ready to fire, sets it as active railgun. WILL NOT
        /// CHECK IF ACTIVE IS NULL!
        /// </summary>
        /// <returns><c>true</c> if there was railgun ready to fire, <c>false</c> otherwise.</returns>
        private bool SetNextActiveRailgun()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun railgun && railgun.CurrentState == Railgun.RailgunState.ReadyToFire)
                {
                    _activeRailgun = railgun;
                    return true;
                }
            }

            return false;
        }

        private Dictionary RayCastForward()
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            Vector3 start = _muzzle.GlobalPosition;
            Vector3 direction = -_muzzle.GlobalBasis.Z;
            Vector3 end = start + direction.Normalized() * 1000f;
            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);
            return result;
        }

        private void UpdateLaserSight()
        {
            if (_lastRaycastResult.ContainsKey("position"))
            {
                SetMeshToRaycastMidpoint(_laserSightInstance);
                _laserSightInstance.Show();
            }
        }

        private void UpdateChargeEffect()
        {
            if (!_chargingSound.IsPlaying())
            {
                _chargingSound.Play();
            }

            SetMeshToRaycastMidpoint(_chargeEffectInstanceBeam);
            float chargePercent = _activeRailgun.CurrentChargePercent;
            _chargeEffectMeshBeam.Radius = _chargeBeamBaseRadius * _chargeBeamWidthModifier * chargePercent / 100.0f;
            _chargeEffectMaterialBeam.EmissionEnergyMultiplier = _chargeEmissionModifier * chargePercent / 100.0f;
            _chargingSound.PitchScale = 1.0f + (_chargeSoundPitchTarget * chargePercent / 100.0f);
        }

        private void SetMeshToRaycastMidpoint(MeshInstance3D meshInstance)
        {
            Vector3 point = (Vector3)_lastRaycastResult["position"];
            Vector3 direction = point - _muzzle.GlobalPosition;
            float distance = direction.Length();
            Vector3 midpoint = _muzzle.GlobalPosition + direction * 0.5f;
            Mesh mesh = meshInstance.Mesh;
            meshInstance.GlobalPosition = midpoint;
            if (mesh is CapsuleMesh capsule)
            {
                capsule.Height = distance;
            }

            if (mesh is CylinderMesh cylinder)
            {
                cylinder.Height = distance;
            }
        }

        private void GenerateSparks(Vector3 position, Vector3 direction)
        {
            GpuParticles3D effect = (GpuParticles3D)_railgunSparkEffect.Instantiate();
            AddChild(effect);
            effect.GlobalPosition = position;
            effect.LookAt(-direction);
            effect.Emitting = true;
            effect.Finished += effect.QueueFree;
        }
    }
}
