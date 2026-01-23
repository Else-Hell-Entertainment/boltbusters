using System;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// TargetProvider is a simple "service" node that stores a reference
    /// to the current Player (CharacterBody3D) and notifies other systems
    /// when that player changes (spawn/despawn/respawn).
    ///
    /// Current use cases:
    /// - Camera follows player
    /// - Enemies seek player
    /// Could be used for:
    /// - Enemy targeting
    /// - Enemy temporary movement target (out of spawn area)
    /// - UI shows player info
    ///
    /// It uses:
    /// 1) Singleton access via TargetProvider.Instance
    /// 2) An event (PlayerChanged) so listeners can react immediately
    /// </summary>
    public partial class TargetProvider : Node
    {
        /// <summary>
        /// Global singleton instance.
        /// Only one TargetProvider should exist at a time.
        /// </summary>
        public static TargetProvider Instance { get; private set; }

        /// <summary>
        /// Event fired when the current Player reference changes.
        ///
        /// Action <CharacterBody3D> means:
        /// - listeners receive a CharacterBody3D parameter
        /// - returns void (no return value)
        ///
        /// The parameter will be:
        /// - the new Player when registered
        /// - null when unregistered (player removed)
        /// </summary>
        public event Action<CharacterBody3D> PlayerChanged;

        /// <summary>
        /// The current player reference.
        /// Can be read by other systems, but only set by this class.
        /// </summary>
        public CharacterBody3D Player { get; private set; }

        /// <summary>
        /// Godot callback: called when this node enters the scene tree.
        /// We use it to enforce the singleton rule.
        /// </summary>
        public override void _EnterTree()
        {
            // If another instance already exists, destroy this duplicate
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            // This becomes the global instance.
            Instance = this;
        }

        /// <summary>
        /// Godot callback: called when this node exits the scene tree.
        /// Clears the singleton reference to avoid stale pointers.
        /// </summary>
        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Register a player as the current Player.
        /// If it changes, the PlayerChanged event is fired.
        /// </summary>
        public void RegisterPlayer(CharacterBody3D player)
        {
            // If the same player is already registered, do nothing.
            if (Player == player)
                return;
            Player = player;

            // Notify all subscribers:
            // - Camera can start following
            // - Enemies can start chasing
            //
            // ?. means: "only do this if there are subscribers"
            PlayerChanged?.Invoke(Player);
        }

        /// <summary>
        /// Unregister the player if it is currently the registered Player.
        /// Fires PlayerChanged with null to indicate "no current player".
        /// </summary>
        public void UnregisterPlayer(CharacterBody3D player)
        {
            // Only unregister if the given player is the current player.
            if (Player != player)
                return;
            Player = null;

            // Notify listeners that there is no player now
            PlayerChanged?.Invoke(null);
        }
    }
}
