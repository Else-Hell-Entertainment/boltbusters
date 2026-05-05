// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using EHE.Common.Godot.Logging;
using Godot;
using GenSysCollections = System.Collections.Generic;

namespace EHE.BoltBusters
{
    public abstract partial class ShaderComponent
    {
        #region Materials Fields

        [Export]
        private MeshInstance3D[] _meshes = new MeshInstance3D[0];

        private readonly GenSysCollections.List<ShaderMaterial> _materials =
            new GenSysCollections.List<ShaderMaterial>();

        #endregion Materials Fields

        #region Materials Setup

        /// <summary>
        /// Duplicates overlay ShaderMaterials on configured meshes so that
        /// each instance has its own independent material for runtime changes.
        /// Logs warnings for any invalid or missing configuration.
        /// </summary>
        private void PrepareMaterials()
        {
            _materials.Clear();

            if (_meshes == null || _meshes.Length == 0)
            {
                this.LogWarning($"ShaderComponent has no meshes assigned. No effects will be visible.");
                return;
            }

            foreach (MeshInstance3D mesh in _meshes)
            {
                if (mesh == null)
                {
                    this.LogWarning($"A MeshInstance3D reference in _meshes is null.");
                    continue;
                }

                if (mesh.MaterialOverlay == null)
                {
                    this.LogWarning($"MaterialOverlay is NOT assigned on '{mesh.Name}'. Effects shader will not run.");
                    continue;
                }

                if (mesh.MaterialOverlay is not ShaderMaterial shaderMaterial)
                {
                    this.LogWarning(
                        $"MaterialOverlay on '{mesh.Name}' is NOT a ShaderMaterial. Expected a ShaderMaterial using the effects shader."
                    );
                    continue;
                }

                ShaderMaterial uniqueMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
                uniqueMaterial.ResourceLocalToScene = true;

                mesh.MaterialOverlay = uniqueMaterial;
                _materials.Add(uniqueMaterial);
            }

            if (_materials.Count == 0)
            {
                this.LogWarning(
                    $"No valid ShaderMaterials prepared. Check that MaterialOverlay uses the correct effects shader."
                );
            }
        }

        #endregion Materials Setup
    }
}
