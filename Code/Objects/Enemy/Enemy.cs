using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Enemy AI that pursues the player using NavigationAgent3D.
    ///
    /// Responsibilities:
    /// - Periodically updates navigation target (player position)
    /// - Computes desired velocity along navigation path
    /// - Optionally applies avoidance to get a safe velocity (avoiding other enemies / _navAgent's)
    /// - Smoothly accelerates movement using CharacterBody3D
    /// - Smoothly rotates to face movement direction (XZ only)
    ///
    /// NOTE:
    /// - Facing is smoothed by interpolating the Y-rotation (angle),
    ///   NOT by smoothing vectors (prevents flipping/jitter).
    /// - Many of the exported fields can be tuned for different enemy behaviors and are there for later use cases
    ///   through code.
    /// </summary>
    public partial class Enemy : CharacterBody3D
    {
        // ---------------------
        // Navigation
        // ---------------------
        private NavigationAgent3D _agent;

        // -------------------------
        // Inspector: Movement
        // -------------------------
        [ExportGroup("Movement")]
        [Export]
        private float _moveSpeed = 4.0f;

        [Export]
        private float _acceleration = 18.0f;

        [Export]
        private float _repathInterval = 0.20f;

        [Export]
        private float _stopDistance = 1.4f;

        // --------------------------
        // Inspector: Avoidance
        // --------------------------
        [ExportGroup("Avoidance")]
        [Export]
        private bool _useAvoidance = true;

        [Export]
        private float _avoidanceRadius = 1.2f;

        [Export]
        private float _neighborDistance = 5f;

        [Export]
        private int _maxNeighbors = 12;

        [Export]
        private float _timeHorizonAgents = 1.2f;

        [Export]
        private float _avoidancePriority = 1.0f;


        // -------------------------
        // Inspector: Facing
        // -------------------------
        [ExportGroup("Facing")]
        // If true, rotate to face movement direction.
        [Export]
        private bool _faceMovement = true;

        // Maximum yaw rotation speed (degrees per second).
        [Export]
        private float _turnSpeedDegreesPerSec = 540f;

        // Minimum horizontal speed required to rotate.
        // Prevents jitter when almost stopped.
        [Export]
        private float _rotateMinSpeed = 0.25f;

        // --------------------
        // Runtime state
        // --------------------
        private CharacterBody3D _player;
        private double _repathTimer;

        private Vector3 _desiredVelocity;
        private Vector3 _safeVelocity;
        private bool _hasSafeVelocity;

        // Cached yaw for smooth angular turning
        private float _currentYaw;

        // Properties
        ///////////////////////////////////////////////////////////////////////

        #region Movement

        public float MoveSpeed
        {
            get => _moveSpeed;
            protected set => _moveSpeed = value;
        }

        public float Acceleration
        {
            get => _acceleration;
            protected set => _acceleration = value;
        }

        public float RepathInterval
        {
            get => _repathInterval;
            protected set => _repathInterval = value;
        }

        public float StopDistance
        {
            get => _stopDistance;
            protected set => _stopDistance = value;
        }

        #endregion Movement

        #region Avoidance

        public bool UseAvoidance
        {
            get => _useAvoidance;
            protected set => _useAvoidance = value;
        }

        public float AvoidanceRadius
        {
            get => _avoidanceRadius;
            protected set => _avoidanceRadius = value;
        }

        public float NeighborDistance
        {
            get => _neighborDistance;
            protected set => _neighborDistance = value;
        }

        public int MaxNeighbors
        {
            get => _maxNeighbors;
            protected set => _maxNeighbors = value;
        }

        public float TimeHorizonAgents
        {
            get => _timeHorizonAgents;
            protected set => _timeHorizonAgents = value;
        }

        public float AvoidancePriority
        {
            get => _avoidancePriority;
            protected set => _avoidancePriority = value;
        }

        #endregion Avoidance

        #region Facing

        public bool FaceMovement
        {
            get => _faceMovement;
            protected set => _faceMovement = value;
        }

        public float TurnSpeedDegreesPerSec
        {
            get => _turnSpeedDegreesPerSec;
            protected set => _turnSpeedDegreesPerSec = value;
        }

        public float RotateMinSpeed
        {
            get => _rotateMinSpeed;
            protected set => _rotateMinSpeed = value;
        }

        #endregion Facing
        /// <summary>
        /// Called when the node enters the scene tree and is ready.
        /// Initializes navigation, avoidance, and target subscriptions.
        /// </summary>
        public override void _Ready()
        {
            _agent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");

            if (_agent == null)
            {
                GD.PushError($"{Name}: Missing NavigationAgent3D child node.");
                SetPhysicsProcess(false);
                return;
            }

            ConfigureAvoidance();

            // Receive avoidance-computed safe velocity
            _agent.VelocityComputed += OnVelocityComputed;

            // Initialize yaw for smooth rotation
            _currentYaw = Rotation.Y;

            // Subscribe to player changes
            if (TargetProvider.Instance != null)
            {
                TargetProvider.Instance.PlayerChanged += OnPlayerChanged;
                OnPlayerChanged(TargetProvider.Instance.Player);
            }
        }

        /// <summary>
        /// Unsubscribe from events when leaving the scene tree.
        /// </summary>
        public override void _ExitTree()
        {
            if (TargetProvider.Instance != null)
                TargetProvider.Instance.PlayerChanged -= OnPlayerChanged;

            if (_agent != null)
                _agent.VelocityComputed -= OnVelocityComputed;
        }

        /// <summary>
        /// Called whenever the player reference changes.
        /// Player may be null during scene transitions.
        /// </summary>
        private void OnPlayerChanged(CharacterBody3D player)
        {
            _player = player;
            _repathTimer = 0;
            _hasSafeVelocity = false;
            _safeVelocity = Vector3.Zero;

            if (_agent != null && _player != null)
                _agent.TargetPosition = _player.GlobalPosition;
        }

        /// <summary>
        /// Applies exported avoidance and movement settings
        /// to the NavigationAgent3D.
        /// </summary>
        private void ConfigureAvoidance()
        {
            _agent.AvoidanceEnabled = UseAvoidance;
            _agent.Radius = AvoidanceRadius;
            _agent.NeighborDistance = NeighborDistance;
            _agent.MaxNeighbors = MaxNeighbors;
            _agent.TimeHorizonAgents = TimeHorizonAgents;
            _agent.AvoidancePriority = AvoidancePriority;

            _agent.MaxSpeed = MoveSpeed;
            _agent.TargetDesiredDistance = StopDistance;
        }

        /// <summary>
        /// Receives the safe velocity computed by the avoidance system.
        /// </summary>
        private void OnVelocityComputed(Vector3 safeVelocity)
        {
            _safeVelocity = safeVelocity;
            _hasSafeVelocity = true;
        }

        /// <summary>
        /// Main physics update loop.
        /// Handles navigation, movement, avoidance, and facing.
        /// </summary>
        public override void _PhysicsProcess(double delta)
        {
            // No target → decelerate smoothly
            if (_player == null)
            {
                Velocity = Velocity.MoveToward(Vector3.Zero, (float)(Acceleration * delta));
                MoveAndSlide();
                return;
            }

            // Periodic repath toward moving player
            _repathTimer += delta;
            if (_repathTimer >= RepathInterval)
            {
                _repathTimer = 0;
                _agent.TargetPosition = _player.GlobalPosition;
            }

            // Desired velocity from navigation
            _desiredVelocity = ComputeDesiredVelocity();

            // Request avoidance each frame (fresh result)
            _hasSafeVelocity = false;
            if (UseAvoidance && _agent.AvoidanceEnabled)
                _agent.Velocity = _desiredVelocity;

            // Choose applied velocity
            // If both conditions are true, use _safeVelocity, otherwise _desiredVelocity.
            Vector3 applied = (UseAvoidance && _hasSafeVelocity) ? _safeVelocity : _desiredVelocity;

            // Accelerate movement
            Velocity = Velocity.MoveToward(applied, (float)(Acceleration * delta));
            MoveAndSlide();

            // Facing
            if (FaceMovement)
                FaceInMovementDirectionSmooth(Velocity, delta);
        }

        /// <summary>
        /// Computes desired horizontal velocity toward the player
        /// using NavigationAgent3D's path.
        /// </summary>
        private Vector3 ComputeDesiredVelocity()
        {
            Vector3 myPos = GlobalPosition;
            Vector3 targetPos = _player.GlobalPosition;

            // Stop when near the player (XZ)
            Vector3 toPlayer = targetPos - myPos;
            toPlayer.Y = 0;
            if (toPlayer.Length() <= StopDistance)
                return Vector3.Zero;

            Vector3 next = !_agent.IsNavigationFinished() ? _agent.GetNextPathPosition() : targetPos;

            Vector3 toNext = next - myPos;
            toNext.Y = 0;

            if (toNext.LengthSquared() < 0.000001f)
                return Vector3.Zero;

            return toNext.Normalized() * MoveSpeed;
        }

        /// <summary>
        /// Smoothly rotates the enemy to face the movement direction (XZ only).
        /// Rotation smoothing is performed on the yaw angle,
        /// preventing jitter and direction flipping.
        /// </summary>
        private void FaceInMovementDirectionSmooth(Vector3 velocity, double delta)
        {
            Vector3 flat = new Vector3(velocity.X, 0f, velocity.Z);

            // Do not rotate when barely moving
            if (flat.LengthSquared() < RotateMinSpeed * RotateMinSpeed)
                return;

            // Same yaw logic as your original working version
            float targetYaw = Mathf.Atan2(-flat.X, -flat.Z);

            float maxStep = Mathf.DegToRad(TurnSpeedDegreesPerSec) * (float)delta;
            _currentYaw = MoveTowardAngleRad(_currentYaw, targetYaw, maxStep);

            Rotation = new Vector3(Rotation.X, _currentYaw, Rotation.Z);
        }

        /// <summary>
        /// Moves an angle (radians) toward another angle (radians)
        /// while correctly handling wrap-around (-PI...PI).
        /// </summary>
        private static float MoveTowardAngleRad(float current, float target, float maxDelta)
        {
            float diff = Mathf.Wrap(target - current, -Mathf.Pi, Mathf.Pi);

            if (Mathf.Abs(diff) <= maxDelta)
                return target;

            return current + Mathf.Sign(diff) * maxDelta;
        }
    }
}
