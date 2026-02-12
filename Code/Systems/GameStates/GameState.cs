// (c) 2025 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//
// Slightly modified from the base implementation by Sami Kojo.
// Source: https://github.com/samikojo-tuni/GArkanoid-2025/blob/c53ac400cc9fbd8855bc59f907ab751aba83c205/Code/GameStates/GameStateBase.cs
// License: https://github.com/samikojo-tuni/GArkanoid-2025/blob/c53ac400cc9fbd8855bc59f907ab751aba83c205/LICENSE

using System;
using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.States
{
    /// <summary>
    /// Enumeration of all possible game state types. Used for checking possible
    /// transitions between different states.
    /// </summary>
    public enum StateType
    {
        None = 0,
        MenuMain,
        MenuPause,
        MenuSettings,
        MenuGameOver,
        Round,
        Shop,
    }

    /// <summary>
    ///  <para>Abstract base class for game states.</para>
    ///  <para>
    ///   Each game state must have its own  state that is derived from this
    ///   class. Derived classes MUST declare the legal target states in their
    ///   constructor by calling the <see cref="AddTargetState"/> method.
    ///  </para>
    ///  <para>
    ///   Additionally, custom activation and deactivation logic for each state
    ///   can be provided by overriding the <see cref="OnEntered"/> and
    ///   <see cref="OnExited"/> methods respectively.
    ///  </para>
    /// </summary>
    public abstract class GameState
    {
        private List<StateType> _targetStates = [];
        private PackedScene _packedScene = null;
        private Node _scene = null;

        /// <summary>
        /// The <see cref="StateType"/> of this state. Used for transition
        /// checks.
        /// </summary>
        public abstract StateType StateType { get; }

        /// <summary>
        /// The path to the scene file associated scene with the state.
        /// </summary>
        public abstract StringName ScenePath { get; }

        /// <summary>
        /// If the state should be loaded on top of other states or not.
        /// </summary>
        public virtual bool IsAdditive => false;

        /// <summary>
        /// Loads the scene file located at <see cref="ScenePath"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///  Thrown if <see cref="ScenePath"/> is set to an empty string or
        ///  <c>null</c>, or if there is no appropriate scene found at the
        ///  specified location.
        /// </exception>
        protected GameState()
        {
            if (ScenePath == null || ScenePath == "")
            {
                throw new InvalidOperationException($"{nameof(ScenePath)} cannot be empty!");
            }

            _packedScene = GD.Load<PackedScene>(ScenePath);

            if (_packedScene == null)
            {
                throw new InvalidOperationException($"No scene found at '{ScenePath}'!");
            }
        }

        /// <summary>
        ///  Adds the given <paramref name="targetStateType"/> to the list of
        ///  legal target states. I.e., allows transitions from this state to a
        ///  state of the given type.
        /// </summary>
        ///
        /// <param name="targetStateType">
        ///  Type of the target state to add.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if target state was added successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        protected bool AddTargetState(StateType targetStateType)
        {
            if (_targetStates.Contains(targetStateType))
            {
                GD.PushError($"Cannot add target state to '{StateType}': '{targetStateType}' already added!");
                return false;
            }

            _targetStates.Add(targetStateType);
            return true;
        }

        /// <summary>
        ///  Removes the given <paramref name="targetStateType"/> from the list
        ///  of legal target states. I.e., prevents transitions from this state
        ///  to a state of the given type.
        /// </summary>
        ///
        /// <param name="targetStateType">
        ///  Type of the target state to remove.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the target state was removed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        protected bool RemoveTargetState(StateType targetStateType)
        {
            if (!_targetStates.Contains(targetStateType))
            {
                GD.PushError($"Cannot remove target state from '{StateType}': '{targetStateType}' not found!");
                return false;
            }

            _targetStates.Remove(targetStateType);
            return true;
        }

        /// <summary>
        ///  Tells if it is possible to transition to a state of the given
        ///  <paramref name="targetStateType"/>.
        /// </summary>
        ///
        /// <param name="targetStateType">
        ///  Type of the target state to check the transition to.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the transition is possible,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool CanTransitionTo(StateType targetStateType)
        {
            return _targetStates.Contains(targetStateType);
        }

        /// <summary>
        ///  <para>
        ///   Activates the state and adds the associated scene to the scene
        ///   tree.
        ///  </para>
        ///  <para>
        ///   If <paramref name="forceLoad"/> is set to <c>true</c>,
        ///   instantiates a new scene from the scene file found at
        ///   <see cref="ScenePath"/> instead of using the cached data.
        ///  </para>
        /// </summary>
        ///
        /// <param name="forceLoad">
        ///  If the scene should be loaded from the file instead of using the
        ///  cached data.
        /// </param>
        public bool Enter(bool forceLoad = false)
        {
            // If force load is triggered and scene exists, clear scene.
            if (forceLoad && _scene != null)
            {
                _scene.QueueFree();
                _scene = null;
            }

            // If scene is not loaded, try to load it.
            if (_scene == null)
            {
                if (_packedScene == null)
                {
                    GD.PushError($"Cannot enter state '{StateType}': no scene found at '{ScenePath}'!");
                    return false;
                }

                _scene = _packedScene.Instantiate();
            }

            // TODO: Threaded level loading: https://docs.godotengine.org/en/latest/tutorials/io/background_loading.html
            GameManager.Instance.SceneTree.CallDeferred(Node.MethodName.AddChild, _scene);
            OnEntered();
            return true;
        }

        /// <summary>
        ///  <para>
        ///   Deactivates the state and removes it from the scene tree.
        ///  </para>
        ///  <para>
        ///   If <paramref name="keepLoaded"/> is set to <c>true</c>, the scene
        ///   is not removed from the scene tree allowing other states to be
        ///   drawn on top.
        ///  </para>
        /// </summary>
        ///
        /// <param name="keepLoaded">
        ///  If the associated scene should be kept in the scene tree after the
        ///  state is deactivated.
        /// </param>
        public void Exit(bool keepLoaded = false)
        {
            if (!keepLoaded && _scene != null)
            {
                _scene.QueueFree();
                _scene = null;
            }

            OnExited();
        }

        /// <summary>
        /// Custom state activation logic. Executed at the end of the
        /// <see cref="Enter"/> method.
        /// </summary>
        protected virtual void OnEntered() { }

        /// <summary>
        /// Custom state deactivation logic. Executed at the end of the
        /// <see cref="Exit"/> method.
        /// </summary>
        protected virtual void OnExited() { }
    }
}
