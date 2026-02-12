// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//
// Adapted from the following implementation by Sami Kojo.
// Source: https://github.com/samikojo-tuni/GArkanoid-2025/blob/c53ac400cc9fbd8855bc59f907ab751aba83c205/Code/Systems/GameManager.cs
// License: https://github.com/samikojo-tuni/GArkanoid-2025/blob/c53ac400cc9fbd8855bc59f907ab751aba83c205/LICENSE

using System;
using System.Collections.Generic;
using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters.Systems
{
    /// <summary>
    ///  <para>
    ///   Handles transitions for states that are derived from
    ///   <see cref="GameState"/>.
    ///  </para>
    ///  <para>
    ///   Keeps track of all registered states (e.g., all possible states) and
    ///   state history. States must be registered at creation time, and they
    ///   cannot be modified afterward.
    ///  </para>
    /// </summary>
    /// <seealso cref="GameState"/>
    public class GameloopStateMachine
    {
        // Created based on pseudocode implementation by M365 Copilot and
        // working implementation by Sami Kojo.

        ///////////////////////////////////////////////////////////////////////
        // Fields (private, protected)
        ///////////////////////////////////////////////////////////////////////

        #region Fields

        /// <summary>
        /// All possible <see cref="GameState"/>s identified by their
        /// <see cref="StateType"/>.
        /// </summary>
        private readonly Dictionary<StateType, GameState> _registeredStates = new();

        /// <summary>
        /// States that are currently loaded into memory.
        /// </summary>
        private readonly Stack<GameState> _stateHistory = new();

        #endregion Fields


        ///////////////////////////////////////////////////////////////////////
        // Properties (private, protected, public)
        ///////////////////////////////////////////////////////////////////////

        #region Properties

        /// <summary>
        /// The currently active state or <c>null</c> if one doesn't exist.
        /// </summary>
        public GameState CurrentState => _stateHistory.Count > 0 ? _stateHistory.Peek() : null;

        #endregion Properties


        ///////////////////////////////////////////////////////////////////////
        // Constructors (public, protected)
        ///////////////////////////////////////////////////////////////////////

        #region Constructors

        /// <summary>
        /// Creates a new state machine and registers the given states.
        /// </summary>
        /// <param name="states"><see cref="GameState"/>s to register.</param>
        public GameloopStateMachine(params GameState[] states)
        {
            if (states.Length < 1)
            {
                throw new InvalidOperationException("Cannot create state machine: no states provided!");
            }

            RegisterStates(states);
        }

        #endregion Constructors


        ///////////////////////////////////////////////////////////////////////
        // Public Methods
        ///////////////////////////////////////////////////////////////////////

        #region Public Methods

        /// <summary>
        /// Performs a state transition from one state to another if possible.
        /// </summary>
        ///
        /// <param name="targetStateType">
        ///  Type of the target state to transition to.
        /// </param>
        /// <param name="forceLoad">
        ///  If the scene associated with the state should be instantiated
        ///  again instead of using the cached scene.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the state transition was successful.<br/>
        ///  <c>false</c> if the state transition is illegal or if the target
        ///  state was not found.
        /// </returns>
        public bool TransitionTo(StateType targetStateType, bool forceLoad = false)
        {
            GameState currentState = CurrentState;

            if (currentState == null)
            {
                GD.PushWarning("Transitioning from non-existent state.");
            }

            // Current state exists but transition is illegal.
            if (currentState != null && !currentState.CanTransitionTo(targetStateType))
            {
                GD.PushError(
                    $"Cannot transition to '{targetStateType}', illegal transition ({currentState.StateType})."
                );
                return false;
            }

            // Target state is not found.
            if (!_registeredStates.TryGetValue(targetStateType, out GameState nextState))
            {
                GD.PushError($"Cannot transition to '{targetStateType}', not found.");
                return false;
            }

            // Exit current state if it exists. Keep the state in memory if next state is additive.
            if (currentState != null)
            {
                currentState.Exit(keepLoaded: nextState.IsAdditive);
            }

            // If next state is not additive, clear state history.
            if (!nextState.IsAdditive)
            {
                _stateHistory.Clear();
            }

            // Add next state to state history and enter the state.
            _stateHistory.Push(nextState);
            nextState.Enter(forceLoad);
            return true;
        }

        /// <summary>
        /// Transitions to the previous state if possible.
        /// </summary>
        ///
        /// <returns>
        ///  <c>true</c> if transition was successfull,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool TransitionToPrevious()
        {
            if (_stateHistory.Count < 2)
            {
                GD.PushError($"Cannot transition to previous state, not found.");
                return false;
            }

            GameState currentState = _stateHistory.Pop();
            GameState previousState = _stateHistory.Peek();

            currentState.Exit();
            previousState.Enter();

            return true;
        }

        #endregion Public Methods


        ///////////////////////////////////////////////////////////////////////
        // Private Methods
        ///////////////////////////////////////////////////////////////////////

        #region Private Methods

        /// <summary>
        /// Adds the given <see cref="GameState"/>s to registered states.
        /// </summary>
        /// <param name="gameStates">One or more unique game states to register.</param>
        private void RegisterStates(params GameState[] gameStates)
        {
            GD.Print("Registering states");
            foreach (var gameState in gameStates)
            {
                if (!RegisterState(gameState))
                {
                    GD.PushError($"Failed to register state '{gameState.StateType}'.");
                }
            }
        }

        /// <summary>
        /// Adds the given <see cref="GameState"/> to registered states.
        /// </summary>
        ///
        /// <param name="gameState">Game state to register.</param>
        ///
        /// <returns>
        ///  <c>true</c> if the state was added successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool RegisterState(GameState gameState)
        {
            StateType stateType = gameState.StateType;

            if (_registeredStates.TryAdd(stateType, gameState))
            {
                return true;
            }

            GD.PushError($"Cannot register state '{stateType}', already registered.");
            return false;
        }

        #endregion Private Methods
    }
}
