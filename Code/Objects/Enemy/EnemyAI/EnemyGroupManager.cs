// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    /// <summary>
    /// Coordinates enemy group behaviours and routes enemies to groups.
    /// </summary>
    public partial class EnemyGroupManager : Node3D
    {
        private GroupBehaviourCannonbotDiamond _diamondGroup;
        private GroupBehaviourCannonbotStandoff _standoffGroup;
        private GroupBehaviourCannonbotSurround _surroundGroup;

        // How often does the pathfinding run. Same interval for all groups.
        private float _repathInterval = 0.05f;

        private double _timer;

        public bool IsActive { get; set; }

        public override void _Ready()
        {
            base._Ready();
            _diamondGroup = new GroupBehaviourCannonbotDiamond();
            _diamondGroup.Name = "Diamond Group";
            _surroundGroup = new GroupBehaviourCannonbotSurround();
            _surroundGroup.Name = "Surround Group";
            _standoffGroup = new GroupBehaviourCannonbotStandoff();
            _standoffGroup.Name = "Standoff Group";

            AddChild(_diamondGroup);
            AddChild(_standoffGroup);
            AddChild(_surroundGroup);

            _diamondGroup.IsActive = true;
            _standoffGroup.IsActive = true;
            _surroundGroup.IsActive = true;
            GameManager.Instance.RoundStateChanged += OnRoundStateChanged;
        }

        private void OnRoundStateChanged(bool inProgress)
        {
            IsActive = inProgress;
            if (!inProgress)
            {
                _diamondGroup.ClearBots();
                _standoffGroup.ClearBots();
                _surroundGroup.ClearBots();
            }
        }

        public override void _Process(double delta)
        {
            if (IsActive)
            {
                _timer += delta;
                if (_timer >= _repathInterval)
                {
                    _timer = 0;
                    ExecuteGroupBehaviours();
                }
            }
        }

        private void ExecuteGroupBehaviours()
        {
            _diamondGroup.Execute();
            _standoffGroup.Execute();
            _surroundGroup.Execute();
        }

        /// <summary>
        /// Registers an enemy to the first group that accepts it. Cascades down and finally assigns any stragglers
        /// to Standoff group.
        /// </summary>
        /// <param name="enemy">Enemy to register.</param>
        public void AddEnemy(Enemy enemy)
        {
            if (_surroundGroup.RegisterBot(enemy))
            {
                return;
            }
            if (_diamondGroup.RegisterBot(enemy))
            {
                return;
            }

            _standoffGroup.RegisterBot(enemy);
        }
    }
}
