// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Rihu, Miska <miska.rihu@tuni.fi>

namespace EHE.BoltBusters.States
{
    /// <summary>
    /// Enumeration of all possible game state types. Used for checking possible
    /// transitions between different states.
    /// </summary>
    public enum StateType
    {
        /// <summary>
        ///  No state active. Use as an error value.
        /// </summary>
        None = 0,

        /// <summary>
        ///  The type of the state that is linked to the main menu scene.
        /// </summary>
        ///
        /// <seealso cref="GameStateMainMenu"/>
        /// <seealso cref="Ui.MenuMain"/>
        MainMenu,

        /// <summary>
        ///  The type of the state that is linked to the pause menu scene.
        /// </summary>
        ///
        /// <seealso cref="GameStatePaused"/>
        /// <seealso cref="Ui.MenuPause"/>
        Paused,

        /// <summary>
        ///  The type of the state that is linked to the settings menu scene.
        /// </summary>
        ///
        /// <seealso cref="GameStateSettingsMenu"/>
        /// <seealso cref="Ui.MenuSettings"/>
        SettingsMenu,

        /// <summary>
        ///  The type of the state that is linked to the game over scene.
        /// </summary>
        ///
        /// <seealso cref="GameOverState"/>
        /// <seealso cref="Ui.MenuGameOver"/>
        GameOver,

        /// <summary>
        ///  The type of the state that is linked to the HUD scene.
        /// </summary>
        ///
        /// <seealso cref="GameStateRound"/>
        /// <seealso cref="Ui.Hud"/>
        Round,

        /// <summary>
        ///  The type of the state that is linked to the shop scene.
        /// </summary>
        ///
        /// <seealso cref="ShopState"/>
        /// <seealso cref="Ui.MenuShop"/>
        Shop,

        /// <summary>
        ///  The state that is linked to the victory scene.
        /// </summary>
        ///
        /// <seealso cref="VictoryState"/>
        /// <seealso cref="Ui.MenuVictory"/>
        Victory,
    }
}
