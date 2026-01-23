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
        private const string MOVELEFT = "MoveLeft";
        private const string MOVERIGHT = "MoveRight";
        private const string MOVEUP = "MoveUp";
        private const string MOVEDOWN = "MoveDown";
        private const string FIRECHAINGUN = "FireChaingun";
        private const string FIRERAILGUN = "FireRailgun";
        private const string FIREROCKET = "FireRocket";

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
            Vector2 inputVector = Input.GetVector(MOVELEFT, MOVERIGHT, MOVEDOWN, MOVEUP);
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
            if (hit == null)return;

            // Create rotation command to face the hit point
            Vector3 hitPoint = hit.Value;
            hitPoint.Y = 0;
            RotateTowardsCommand cmd = new RotateTowardsCommand(hitPoint);
            _entityController.AddCommand(cmd);
        }

        private void GetAttackInput()
        {
            if (Input.IsActionPressed(FIRECHAINGUN))
            {
                _entityController.AddCommand(new AttackCommand("Chaingun"));
            }

            if (Input.IsActionPressed(FIRERAILGUN))
            {
                _entityController.AddCommand((new AttackCommand("Railgun")));
            }

            if (Input.IsActionPressed(FIREROCKET))
            {
                _entityController.AddCommand(new AttackCommand("Rocket"));
            }
        }
    }
}
