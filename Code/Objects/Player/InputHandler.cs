// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using EHE.BoltBusters.Config;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Handles player input and translates it into commands for entity control.
    /// Processes keyboard input for movement and mouse input for rotation.
    /// </summary>
    public partial class InputHandler : Node3D
    {
        /// <summary>
        /// The entity controller that receives and executes the generated commands.
        /// </summary>
        private EntityController _entityController;

        /// <summary>
        /// Reference to the active camera used for mouse position calculations.
        /// </summary>
        private Camera3D _camera;

        private bool _isMouseActive;

        public override void _Ready()
        {
            _camera = GameManager.Instance.Camera;
        }

        public override void _PhysicsProcess(double delta)
        {
            GetMovementInput();
            if (_isMouseActive)
            {
                GetMouseRotationInput();
            }
            GetControllerRotationInput();

            GetAttackInput();
        }

        /// <summary>
        /// Used to switch between mouse and controller.
        /// </summary>
        /// <param name="event"></param>
        public override void _Input(InputEvent @event)
        {
            switch (@event)
            {
                case InputEventMouseMotion:
                    _isMouseActive = true;
                    Input.SetMouseMode(Input.MouseModeEnum.Visible);
                    break;
                case InputEventJoypadMotion joypadMotion:
                    if (joypadMotion.Axis == JoyAxis.RightX || joypadMotion.Axis == JoyAxis.RightY)
                    {
                        if (joypadMotion.AxisValue > 0.1f)
                        {
                            _isMouseActive = false;
                            Input.SetMouseMode(Input.MouseModeEnum.Hidden);
                        }
                    }
                    break;
            }
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
            Vector2 inputVector = Input.GetVector(
                ControlConfig.MOVE_LEFT,
                ControlConfig.MOVE_RIGHT,
                ControlConfig.MOVE_DOWN,
                ControlConfig.MOVE_UP
            );
            Vector3 moveVector = new Vector3(inputVector.X, 0, -inputVector.Y);
            _entityController.AddCommand(new MoveToDirectionCommand(moveVector));
        }

        /// <summary>
        /// Reads mouse position and generates rotation commands to face the cursor.
        /// Performs raycasting from the camera to find the intersection point on the ground plane.
        /// </summary>
        private void GetMouseRotationInput()
        {
            // Ensure camera reference is valid
            if (!IsInstanceValid(_camera))
            {
                _camera = GameManager.Instance.Camera;
                if (!IsInstanceValid(_camera))
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

        /// <summary>
        /// When controller is active, motion is taken from controller joystick.
        /// </summary>
        private void GetControllerRotationInput()
        {
            Vector2 rotation = Input.GetVector(
                ControlConfig.ROTATE_LEFT,
                ControlConfig.ROTATE_RIGHT,
                ControlConfig.ROTATE_UP,
                ControlConfig.ROTATE_DOWN
            );
            if (!rotation.IsZeroApprox())
            {
                Vector3 rot = new Vector3(rotation.X, 0, rotation.Y);
                RotateToDirectionCommand command = new RotateToDirectionCommand(rot);
                _entityController.AddCommand(command);
            }
        }

        /// <summary>
        /// Process attack commands from player.
        /// </summary>
        private void GetAttackInput()
        {
            if (Input.IsActionPressed(ControlConfig.FIRE_CHAINGUN))
            {
                _entityController.AddCommand(new AttackCommand(WeaponType.Chaingun));
            }

            if (Input.IsActionJustPressed(ControlConfig.FIRE_RAILGUN))
            {
                _entityController.AddCommand((new AttackCommand(WeaponType.Railgun)));
            }

            if (Input.IsActionJustPressed(ControlConfig.FIRE_ROCKET))
            {
                _entityController.AddCommand(new AttackCommand(WeaponType.Rocket));
            }
        }
    }
}
