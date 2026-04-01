using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    public partial class EnemyGroupManager : Node3D
    {
        private GroupBehaviourCannonbotDiamond _diamondGroup;

        private float _repathInterval = 0.05f;

        private double _timer;

        public bool IsActive { get; set; }

        public override void _Ready()
        {
            base._Ready();
            _diamondGroup = new GroupBehaviourCannonbotDiamond();
            AddChild(_diamondGroup);

            IsActive = true;
            _diamondGroup.IsActive = true;
            GameManager.Instance.RoundStateChanged += OnRoundStateChanged;
        }

        private void OnRoundStateChanged(bool inProgress)
        {
            if (!inProgress)
            {
                _diamondGroup.ClearBots();
                GD.Print("Clearing bots");
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
        }

        public void AddEnemy(Enemy enemy)
        {
            if (_diamondGroup.RegisterBot(enemy))
            {
                GD.Print("Manager found the bot.");
            }
        }
    }
}
