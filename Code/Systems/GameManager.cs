using EHE.BoltBusters.States;
using EHE.BoltBusters.Systems;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// WIP. Only dummy implementation with the minimum necessary logic to test
    /// the GameState class.
    /// </summary>
    public partial class GameManager : Node
    {
        private SceneTree _sceneTree = null;

        public static GameManager Instance = null;

        public override void _Ready()
        {
            Instance = this;
            StateMachine = new GameloopStateMachine(new GameStateMenuMain(), new GameStateMenuSettings());
        }

        public SceneTree SceneTree
        {
            get
            {
                if (_sceneTree == null)
                {
                    _sceneTree = GetTree();
                }

                return _sceneTree;
            }
        }

        public GameloopStateMachine StateMachine { get; private set; }
    }
}
