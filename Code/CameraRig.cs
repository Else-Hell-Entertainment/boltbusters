using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Camera rig that follows a target (player) in XZ, optionally clamped to arena bounds,
    /// and drives a child Camera3D using adjustable angle and height.
    ///
    /// - Uses an explicit target if set in the inspector.
    /// - Otherwise, uses TargetProvider.Instance.Player if available.
    /// - Rig position is smoothed; camera snaps to its ideal position each frame.
    /// </summary>
    public partial class CameraRig : Marker3D
    {
        // --------------------------------------------------------------------
        // Rig configuration (position, bounds, smoothing)
        // --------------------------------------------------------------------

        [Export]
        public Vector3 WorldOffset { get; set; } = Vector3.Zero;

        /// <summary>
        /// Base Y height of the rig. The camera height is controlled separately
        /// by HeightAboveGround; this is mainly for where the rig marker sits.
        /// </summary>
        [Export]
        public float FixedRigY { get; set; } = 0.0f;

        [Export]
        public bool UseSmoothFollow { get; set; } = true;

        /// <summary>
        /// Higher values make the rig follow more "snappy". Used for exponential smoothing.
        /// </summary>
        [Export]
        public float SmoothSpeed { get; set; } = 12.0f;

        /// <summary>
        /// If true, clamp the rig's X/Z position inside the arena rectangle.
        /// </summary>
        [Export]
        public bool UseArenaBounds { get; set; } = true;

        [Export]
        public float MinX { get; set; } = 0.0f;

        [Export]
        public float MaxX { get; set; } = 48.0f;

        [Export]
        public float MinZ { get; set; } = 0.0f;

        [Export]
        public float MaxZ { get; set; } = 48.0f;

        /// <summary>
        /// Additional Z offset for the focus point (world space).
        /// Positive values move the camera focus "forward" along +Z,
        /// which will shift the player up/down on screen depending on your layout.
        /// </summary>
        [Export]
        public float FocusOffsetZ { get; set; } = 0.0f;

        // --------------------------------------------------------------------
        // Camera configuration (angle, height)
        // --------------------------------------------------------------------

        /// <summary>
        /// Camera node to be driven by this rig.
        /// </summary>
        [Export]
        public Camera3D CameraNode { get; set; }

        /// <summary>
        /// Pitch (X rotation) in degrees. Use negative values to look down (e.g., -70).
        /// </summary>
        [Export]
        public float AngleXDegrees { get; set; } = -65.0f;

        /// <summary>
        /// Camera height above the ground at the focus XZ.
        /// </summary>
        [Export]
        public float HeightAboveGround { get; set; } = 6.0f;

        /// <summary>
        /// If the target's origin is not exactly on the ground, offset for ground level.
        /// For example, if feet are 1 unit below origin, set this to -1.0.
        /// </summary>
        [Export]
        public float TargetGroundYOffset { get; set; } = 0.0f;

        /// <summary>
        /// If true, yaw is locked to zero (looking along -Z).
        /// </summary>
        [Export]
        public bool LockYawToZero { get; set; } = true;

        // --------------------------------------------------------------------
        // Target selection (explicit vs TargetProvider)
        // --------------------------------------------------------------------

        /// <summary>
        /// Optional explicit target. If set, the rig will follow this.
        /// If null, falls back to TargetProvider.Instance.Player if available.
        /// </summary>
        [Export]
        public CharacterBody3D ExplicitTarget { get; set; }

        /// <summary>
        /// Current target used by the rig (explicit first, then TargetProvider).
        /// </summary>
        private CharacterBody3D Target
        {
            get
            {
                if (ExplicitTarget != null)
                {
                    return ExplicitTarget;
                }

                if (TargetProvider.Instance != null)
                {
                    return TargetProvider.Instance.Player;
                }

                return null;
            }
        }

        // --------------------------------------------------------------------
        // Godot callbacks
        // --------------------------------------------------------------------

        public override void _Ready()
        {
            if (CameraNode == null)
            {
                GD.PushError(
                    $"{nameof(CameraRig)}: CameraNode is not assigned. Assign a child Camera3D in the inspector."
                );
            }

            if (ExplicitTarget == null && TargetProvider.Instance == null)
            {
                GD.PushWarning(
                    $"{nameof(CameraRig)}: No ExplicitTarget and TargetProvider.Instance is null. "
                        + "Camera will not follow anything until one of these is set."
                );
            }
        }

        public override void _Process(double delta)
        {
            CharacterBody3D currentTarget = Target;

            if (currentTarget == null || CameraNode == null)
            {
                return;
            }

            // 1) Move rig towards desired position (with smoothing if enabled).
            UpdateRigPosition(currentTarget, delta);

            // 2) Compute camera transform for this tentative rig position.
            UpdateCameraTransform(currentTarget);

            // 3) If using arena bounds, adjust rig so that the camera's visible
            //    ground area stays inside the arena, then recompute camera.
            if (UseArenaBounds)
            {
                ClampRigToArenaByCameraEdges(currentTarget);
                UpdateCameraTransform(currentTarget);
            }
        }

        // --------------------------------------------------------------------
        // Public helpers
        // --------------------------------------------------------------------

        /// <summary>
        /// Immediately snap rig and camera to the current target position,
        /// ignoring smoothing for this frame.
        /// Useful when switching scenes or respawning.
        /// </summary>
        public void SnapToTarget()
        {
            CharacterBody3D currentTarget = Target;

            if (currentTarget == null || CameraNode == null)
            {
                return;
            }

            // Compute the same desired rig position as in UpdateRigPosition, but without smoothing.
            Vector3 rigDesiredPosition = ComputeDesiredRigPosition(currentTarget);
            GlobalPosition = rigDesiredPosition;

            // Directly compute and apply camera transform.
            UpdateCameraTransform(currentTarget);
        }

        // --------------------------------------------------------------------
        // Private methods
        // --------------------------------------------------------------------

        /// <summary>
        /// Compute where the rig wants to be this frame (before smoothing).
        /// This uses player X/Z, world offset, and focus Z offset.
        /// Arena bounds are applied later based on the camera's visible edges,
        /// not directly on this center point.
        /// </summary>
        private Vector3 ComputeDesiredRigPosition(CharacterBody3D currentTarget)
        {
            Vector3 playerPosition = currentTarget.GlobalTransform.Origin;

            float desiredX = playerPosition.X + WorldOffset.X;
            float desiredZ = playerPosition.Z + WorldOffset.Z + FocusOffsetZ;
            float desiredY = FixedRigY + WorldOffset.Y;

            return new Vector3(desiredX, desiredY, desiredZ);
        }

        /// <summary>
        /// Adjusts the rig position so that the camera's visible ground area
        /// (the ground intersections of the four frustum corner rays)
        /// stays inside the arena rectangle [MinX, MaxX] x [MinZ, MaxZ].
        ///
        /// Works for orthographic cameras at an angle by:
        /// - Casting rays through the four screen corners,
        /// - Intersecting them with the ground plane at the target's ground Y,
        /// - Computing min/max X/Z of those intersection points,
        /// - Shifting the rig (and thus the camera) so edges do not cross the arena.
        /// </summary>
        private void ClampRigToArenaByCameraEdges(CharacterBody3D currentTarget)
        {
            if (!UseArenaBounds)
            {
                return;
            }

            Viewport viewport = CameraNode.GetViewport();
            if (viewport == null)
            {
                return;
            }

            Rect2 viewportRect = viewport.GetVisibleRect();
            if (viewportRect.Size.X <= 0.0f || viewportRect.Size.Y <= 0.0f)
            {
                return;
            }

            // Ground plane used for intersection (same as in UpdateCameraTransform).
            Vector3 targetPosition = currentTarget.GlobalTransform.Origin;
            float groundY = targetPosition.Y + TargetGroundYOffset;

            Vector2 topLeft = viewportRect.Position;
            Vector2 size = viewportRect.Size;

            Vector2[] screenPoints = new Vector2[4];
            screenPoints[0] = topLeft;
            screenPoints[1] = topLeft + new Vector2(size.X, 0.0f);
            screenPoints[2] = topLeft + new Vector2(size.X, size.Y);
            screenPoints[3] = topLeft + new Vector2(0.0f, size.Y);

            bool anyHit = false;
            float minX = 0.0f;
            float maxX = 0.0f;
            float minZ = 0.0f;
            float maxZ = 0.0f;

            for (int i = 0; i < screenPoints.Length; i++)
            {
                Vector3 origin = CameraNode.ProjectRayOrigin(screenPoints[i]);
                Vector3 direction = CameraNode.ProjectRayNormal(screenPoints[i]);

                if (Mathf.IsZeroApprox(direction.Y))
                {
                    continue;
                }

                float t = (groundY - origin.Y) / direction.Y;

                // Only consider intersections in front of the camera.
                if (t < 0.0f)
                {
                    continue;
                }

                Vector3 hit = origin + direction * t;

                if (!anyHit)
                {
                    anyHit = true;
                    minX = maxX = hit.X;
                    minZ = maxZ = hit.Z;
                }
                else
                {
                    if (hit.X < minX)
                    {
                        minX = hit.X;
                    }

                    if (hit.X > maxX)
                    {
                        maxX = hit.X;
                    }

                    if (hit.Z < minZ)
                    {
                        minZ = hit.Z;
                    }

                    if (hit.Z > maxZ)
                    {
                        maxZ = hit.Z;
                    }
                }
            }

            if (!anyHit)
            {
                return;
            }

            float viewWidth = maxX - minX;
            float viewHeight = maxZ - minZ;
            float arenaWidth = MaxX - MinX;
            float arenaHeight = MaxZ - MinZ;

            float deltaX = 0.0f;
            float deltaZ = 0.0f;

            // --- X axis: shift so [minX, maxX] fits within [MinX, MaxX] ---
            if (viewWidth <= arenaWidth)
            {
                float minShiftX = MinX - minX; // shift right so left edge hits MinX
                float maxShiftX = MaxX - maxX; // shift left so right edge hits MaxX

                bool outOfBoundsX = minX < MinX || maxX > MaxX;

                if (outOfBoundsX)
                {
                    if (0.0f < minShiftX)
                    {
                        // Need to move right at least this much.
                        deltaX = minShiftX;
                    }
                    else if (0.0f > maxShiftX)
                    {
                        // Need to move left at least this much.
                        deltaX = maxShiftX;
                    }
                }
            }
            else
            {
                // Camera view is wider than arena: we cannot fit edges inside.
                // Best we can do is center the view over the arena.
                float arenaCenterX = 0.5f * (MinX + MaxX);
                float viewCenterX = 0.5f * (minX + maxX);
                deltaX = arenaCenterX - viewCenterX;
            }

            // --- Z axis: shift so [minZ, maxZ] fits within [MinZ, MaxZ] ---
            if (viewHeight <= arenaHeight)
            {
                float minShiftZ = MinZ - minZ;
                float maxShiftZ = MaxZ - maxZ;

                bool outOfBoundsZ = minZ < MinZ || maxZ > MaxZ;

                if (outOfBoundsZ)
                {
                    if (0.0f < minShiftZ)
                    {
                        deltaZ = minShiftZ;
                    }
                    else if (0.0f > maxShiftZ)
                    {
                        deltaZ = maxShiftZ;
                    }
                }
            }
            else
            {
                float arenaCenterZ = 0.5f * (MinZ + MaxZ);
                float viewCenterZ = 0.5f * (minZ + maxZ);
                deltaZ = arenaCenterZ - viewCenterZ;
            }

            if (Mathf.IsZeroApprox(deltaX) && Mathf.IsZeroApprox(deltaZ))
            {
                return;
            }

            // Move the rig in X/Z; camera will be updated again after this.
            GlobalPosition += new Vector3(deltaX, 0.0f, deltaZ);
        }

        /// <summary>
        /// Update the rig's GlobalPosition with optional exponential smoothing.
        /// </summary>
        private void UpdateRigPosition(CharacterBody3D currentTarget, double delta)
        {
            Vector3 desiredPosition = ComputeDesiredRigPosition(currentTarget);

            if (!UseSmoothFollow)
            {
                GlobalPosition = desiredPosition;
                return;
            }

            float t = 1.0f - Mathf.Exp(-SmoothSpeed * (float)delta);
            GlobalPosition = GlobalPosition.Lerp(desiredPosition, t);
        }

        /// <summary>
        /// Update the camera's rotation and position so that it looks at a ground
        /// point under/near the target, using the configured angle and height.
        /// </summary>
        private void UpdateCameraTransform(CharacterBody3D currentTarget)
        {
            // --- Build desired orientation (pitch + yaw) ---
            float pitchDegrees = AngleXDegrees;
            float yawDegrees = LockYawToZero ? 0.0f : CameraNode.RotationDegrees.Y;

            // Avoid degenerate pitch angles (e.g., exactly 0 or 90 degrees).
            float pitchAbs = Mathf.Clamp(Mathf.Abs(pitchDegrees), 1.0f, 89.9f);
            Vector3 eulerRad = new Vector3(pitchDegrees, yawDegrees, 0.0f) * Mathf.DegToRad(1.0f);
            Basis desiredBasis = Basis.FromEuler(eulerRad);

            // Camera forward is -Z in Godot's convention.
            Vector3 forward = -desiredBasis.Z;

            // Ensure we are looking downward (negative Y); if not, flip pitch sign.
            if (forward.Y >= 0.0f)
            {
                pitchDegrees = -pitchAbs;
                eulerRad = new Vector3(pitchDegrees, yawDegrees, 0.0f) * Mathf.DegToRad(1.0f);
                desiredBasis = Basis.FromEuler(eulerRad);
                forward = -desiredBasis.Z;
            }

            // --- Define the ground point we want the camera to "aim" at ---
            Vector3 playerPosition = currentTarget.GlobalTransform.Origin;
            float groundY = playerPosition.Y + TargetGroundYOffset;

            // We use the rig's X/Z as the focus center (already offset + clamped),
            // but the ground height is taken from the target.
            Vector3 groundPoint = new Vector3(GlobalPosition.X, groundY, GlobalPosition.Z);

            // --- Compute camera position so that its central ray passes through groundPoint ---
            // For a given forward direction, we move along that direction so that the camera
            // is HeightAboveGround units above the ground plane in world space.
            float denom = forward.Y; // should be negative (looking down)
            if (Mathf.IsZeroApprox(denom))
            {
                // Avoid division by zero: in this degenerate case we just keep the current position.
                return;
            }

            float t = HeightAboveGround / denom; // usually negative because denom < 0
            Vector3 desiredCameraPosition = groundPoint + forward * t;

            // Apply transform directly; rig smoothing already handled the "follow feel".
            Transform3D newTransform = new Transform3D(desiredBasis, desiredCameraPosition);
            CameraNode.GlobalTransform = newTransform;
        }
    }
}
