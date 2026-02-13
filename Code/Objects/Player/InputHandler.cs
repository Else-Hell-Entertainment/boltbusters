using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Handles player input and translates it into commands for entity control.
    /// Processes keyboard input for movement and mouse input for rotation.
    /// </summary>
    public partial class InputHandler : Node3D
    {
        // Input action names
        private const string MOVE_LEFT = "MoveLeft";
        private const string MOVE_RIGHT = "MoveRight";
        private const string MOVE_UP = "MoveUp";
        private const string MOVE_DOWN = "MoveDown";
        private const string FIRE_CHAINGUN = "FireChaingun";
        private const string FIRE_RAILGUN = "FireRailgun";
        private const string FIRE_ROCKET = "FireRocket";

        /// <summary>
        /// The entity controller that receives and executes the generated commands.
        /// </summary>
        private EntityController _entityController;

        /// <summary>
        /// Reference to the active camera used for mouse position calculations.
        /// </summary>
        private Camera3D _camera;

        public override void _Ready()
        {
            _camera = GetViewport().GetCamera3D();
        }

        public override void _PhysicsProcess(double delta)
        {
            GetMovementInput();
            GetRotationInput();
            GetAttackInput();
        }

        /// <summary>
        /// Assigns the entity controller that will receive commands from this input handler.
        /// </summary>
        /// <param name="entityController">The controller to receive input commands.</param>
        public void SetEntityController(EntityController entityController)
        {
            _entityController = entityController;
        }

        /// <summary>
        /// Reads keyboard input and generates movement commands based on WASD or arrow keys.
        /// Converts 2D input into a 3D direction vector on the XZ plane.
        /// </summary>
        private void GetMovementInput()
        {
            Vector2 inputVector = Input.GetVector(MOVE_LEFT, MOVE_RIGHT, MOVE_DOWN, MOVE_UP);
            if (inputVector == Vector2.Zero)
            {
                return;
            }
            Vector3 moveVector = new Vector3(inputVector.X, 0, -inputVector.Y).Normalized();
            _entityController.AddCommand(new MoveToDirectionCommand(moveVector));
        }

        /// <summary>
        /// Reads mouse position and generates rotation commands to face the cursor.
        /// Performs raycasting from the camera to find the intersection point on the ground plane.
        /// </summary>
        private void GetRotationInput()
        {
            // Ensure camera reference is valid
            if (_camera == null)
            {
                _camera = GetViewport().GetCamera3D();
                if (_camera == null)
                {
                    GD.PrintErr("Attempting to Raycast from camera but no camera defined.");
                    return;
                }
            }

            // Cast ray from camera through mouse position
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 rayStart = _camera.ProjectRayOrigin(mousePos);
            Vector3 rayDirection = _camera.ProjectRayNormal(mousePos);

            // Find intersection with ground plane at entity's height
            Plane groundPlane = new Plane(Vector3.Up, GlobalPosition.Y);
            Vector3? hit = groundPlane.IntersectsRay(rayStart, rayDirection);
            if (hit == null)
                return;

            // Create rotation command to face the hit point
            Vector3 hitPoint = hit.Value;
            hitPoint.Y = 0;
            RotateTowardsCommand cmd = new RotateTowardsCommand(hitPoint);
            _entityController.AddCommand(cmd);
        }

        private void GetAttackInput()
        {
            if (Input.IsActionPressed(FIRE_CHAINGUN))
            {
                _entityController.AddCommand(new AttackCommand(WeaponType.Chaingun));
            }

            if (Input.IsActionJustPressed(FIRE_RAILGUN))
            {
                _entityController.AddCommand((new AttackCommand(WeaponType.Railgun)));
            }

            if (Input.IsActionJustPressed(FIRE_ROCKET))
            {
                _entityController.AddCommand(new AttackCommand(WeaponType.Rocket));
            }
        }
    }
}
