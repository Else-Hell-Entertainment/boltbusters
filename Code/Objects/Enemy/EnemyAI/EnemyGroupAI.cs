using System.Collections.Generic;
using EHE.BoltBusters.EnemyAI;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyGroupAI : Node3D
    {
        [Export]
        private EnemyCannonBot _bot1;

        [Export]
        private EnemyCannonBot _bot2;

        [Export]
        private EnemyCannonBot _bot3;

        [Export]
        private EnemyCannonBot _bot4;

        [Export]
        private EnemyCannonBot _bot5;

        [Export]
        private EnemyCannonBot _bot6;

        [Export]
        private EnemyCannonBot _bot7;

        [Export]
        private EnemyCannonBot _bot8;

        private List<IEnemyGroup> _groupList = new();

        private bool _isSurrounding = true;
        private double _timer;
        private double _switchTime = 10;

        public override void _Ready()
        {
            base._Ready();
            EnemyGroupCannonBotSurroundPlayer _surroundGroup = new EnemyGroupCannonBotSurroundPlayer();
            EnemyGroupCannonBotDiamond _diamondGroup1 = new EnemyGroupCannonBotDiamond();
            EnemyGroupCannonBotDiamond _diamondGroup2 = new EnemyGroupCannonBotDiamond();
            AddChild(_diamondGroup1);
            AddChild(_diamondGroup2);
            AddChild(_surroundGroup);
            _surroundGroup.RegisterBot(_bot1);
            _surroundGroup.RegisterBot(_bot2);
            _surroundGroup.RegisterBot(_bot3);
            _surroundGroup.RegisterBot(_bot4);
            _surroundGroup.RegisterBot(_bot5);
            _surroundGroup.RegisterBot(_bot6);
            _surroundGroup.RegisterBot(_bot7);
            _surroundGroup.RegisterBot(_bot8);
            _diamondGroup1.RegisterBot(_bot1);
            _diamondGroup1.RegisterBot(_bot2);
            _diamondGroup1.RegisterBot(_bot3);
            _diamondGroup1.RegisterBot(_bot4);
            _diamondGroup2.RegisterBot(_bot5);
            _diamondGroup2.RegisterBot(_bot6);
            _diamondGroup2.RegisterBot(_bot7);
            _diamondGroup2.RegisterBot(_bot8);

            _groupList.Add(_surroundGroup);
            _groupList.Add(_diamondGroup1);
            _groupList.Add(_diamondGroup2);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            _timer += delta;
            if (_timer >= _switchTime)
            {
                _timer = 0;
                if (_isSurrounding)
                {
                    _isSurrounding = false;
                    foreach (IEnemyGroup group in _groupList)
                    {
                        if (group is EnemyGroupCannonBotSurroundPlayer surrounder)
                        {
                            surrounder.IsActive = false;
                        }
                        else
                        {
                            group.IsActive = true;
                        }
                    }
                }
                else
                {
                    _isSurrounding = true;
                    foreach (IEnemyGroup group in _groupList)
                    {
                        if (group is EnemyGroupCannonBotSurroundPlayer surrounder)
                        {
                            surrounder.IsActive = true;
                        }
                        else
                        {
                            group.IsActive = false;
                        }
                    }
                }
            }

            foreach (IEnemyGroup group in _groupList)
            {
                group.Execute();
            }
        }
    }
}
