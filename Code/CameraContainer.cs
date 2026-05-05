// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    public partial class CameraContainer : SubViewportContainer
    {
        private const string NO_SUITABLE_NODE = "No suitable node for {0} found in the scene!";

        private CameraRig _cameraRig;

        // This has to be here cuz fuck godot things.
        private AudioListener3D _listener;

        /// <summary>
        ///  Reference to the global camera in the camera rig.
        /// </summary>
        public Camera3D Camera { get; private set; }

        /// <summary>
        ///  Reference to the sub-viewport in the camera container.
        /// </summary>
        public SubViewport Viewport { get; private set; }

        /// <summary>
        ///  Sets the viewport, camera rig, and camera. If any of these steps
        ///  fails, an error is logged and the method returns.
        /// </summary>
        public override void _Ready()
        {
            if (!SetViewport())
            {
                this.LogError(string.Format(NO_SUITABLE_NODE, nameof(Viewport)));
                return;
            }

            if (!SetCameraRig())
            {
                this.LogError(string.Format(NO_SUITABLE_NODE, nameof(_cameraRig)));
                return;
            }

            if (!SetCamera())
            {
                this.LogError(string.Format(NO_SUITABLE_NODE, nameof(Camera)));
                return;
            }

            if (!SetListener())
            {
                this.LogError(string.Format(NO_SUITABLE_NODE, nameof(_listener)));
                return;
            }

            this.LogDebug("Ready.");
        }

        public override void _Process(double delta)
        {
            if (LevelManager.Active != null && LevelManager.Active.Player != null)
            {
                _listener.GlobalPosition = LevelManager.Active.Player.GlobalPosition;
            }
        }

        private bool SetViewport()
        {
            Viewport = this.GetFirstChildOfType<SubViewport>();

            if (Viewport == null)
            {
                return false;
            }

            // Viewport.Size = (Vector2I)GetViewport().GetWindow().GetVisibleRect().Size;
            Viewport.AudioListenerEnable3D = true;

            this.LogDebug("Set viewport.");
            return true;
        }

        private bool SetCameraRig()
        {
            _cameraRig = Viewport.GetFirstChildOfType<CameraRig>();

            if (_cameraRig == null)
            {
                return false;
            }

            // Suggested settings:
            // _cameraRig.HeightAboveGround = 10f;
            // _cameraRig.UseSmoothFollow = false;

            this.LogDebug("Set camera rig.");
            return true;
        }

        private bool SetCamera()
        {
            Camera = _cameraRig.GetFirstChildOfType<Camera3D>();
            this.LogDebug("Set camera.");
            return Camera != null;
        }

        private bool SetListener()
        {
            _listener = Viewport.GetFirstChildOfType<AudioListener3D>(recurse: true);
            this.LogDebug("Set listener.");
            return _listener != null;
        }
    }
}
