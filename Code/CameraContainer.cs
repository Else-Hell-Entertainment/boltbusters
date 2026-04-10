// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    public partial class CameraContainer : SubViewportContainer
    {
        private const string NO_SUITABLE_NODE = "No suitable node for {0} found in the scene!";

        private CameraRig _cameraRig;

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
                GD.PushError(string.Format(NO_SUITABLE_NODE, nameof(Viewport)));
                return;
            }

            if (!SetCameraRig())
            {
                GD.PushError(string.Format(NO_SUITABLE_NODE, nameof(_cameraRig)));
                return;
            }

            if (!SetCamera())
            {
                GD.PushError(string.Format(NO_SUITABLE_NODE, nameof(Camera)));
                return;
            }

            this.PrintDebug("Ready.");
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

            this.PrintDebug("Set viewport.");
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

            this.PrintDebug("Set camera rig.");
            return true;
        }

        private bool SetCamera()
        {
            Camera = _cameraRig.GetFirstChildOfType<Camera3D>();
            this.PrintDebug("Set camera.");
            return Camera != null;
        }
    }
}
