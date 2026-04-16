// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Rihu, Miska <miska.rihu@tuni.fi>

using System;
using EHE.BoltBusters.States;
using EHE.BoltBusters.Ui;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuVictory : Menu
    {
        [Export]
        private Button _btnCredits;

        [Export]
        private Button _btnMainMenu;

        [Export]
        private Button _btnQuit;

        public override void _Ready()
        {
            var isValid = true;

            if (_btnCredits == null)
            {
                this.LogErrorNoStackTrace($"{nameof(_btnCredits)} not assigned!");
                isValid = false;
            }

            if (_btnMainMenu == null)
            {
                this.LogErrorNoStackTrace($"{nameof(_btnMainMenu)} not assigned!");
                isValid = false;
            }

            if (_btnQuit == null)
            {
                this.LogErrorNoStackTrace($"{nameof(_btnQuit)} not assigned!");
                isValid = false;
            }

            if (!isValid)
            {
                this.LogErrorNoStackTrace($"Errors encountered in {nameof(_Ready)}.");
            }
        }

        public override void _EnterTree()
        {
            _btnCredits.Pressed += OnBtnCreditsPressed;
            _btnMainMenu.Pressed += OnBtnMainMenuPressed;
            _btnQuit.Pressed += OnBtnQuitPressed;
        }

        public override void _ExitTree()
        {
            _btnCredits.Pressed -= OnBtnCreditsPressed;
            _btnMainMenu.Pressed -= OnBtnMainMenuPressed;
            _btnQuit.Pressed -= OnBtnQuitPressed;
        }

        private void OnBtnCreditsPressed()
        {
            throw new NotImplementedException("No credits window implemented.");
        }

        private void OnBtnMainMenuPressed()
        {
            GameManager.Instance.StateMachine.TransitionTo(StateType.MainMenu);
        }

        private void OnBtnQuitPressed()
        {
            GameManager.Instance.Quit();
        }
    }
}
