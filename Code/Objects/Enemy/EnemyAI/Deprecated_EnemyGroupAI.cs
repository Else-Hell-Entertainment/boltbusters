// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System;
using System.Collections.Generic;
using EHE.BoltBusters.EnemyAI;
using Godot;

namespace EHE.BoltBusters
{
    [Obsolete]
    public partial class Deprecated_EnemyGroupAI : Node3D
    {
        private List<IEnemyGroup> _groupList = new();

        private bool _isSurrounding = true;
        private double _timer;
        private double _switchTime = 10;

        public override void _Ready()
        {
            base._Ready();
            Deprecated_EnemyGroupCannonBotSurroundPlayer _surroundGroup =
                new Deprecated_EnemyGroupCannonBotSurroundPlayer();
            Deprecated_EnemyGroupCannonBotDiamond _diamondGroup1 = new Deprecated_EnemyGroupCannonBotDiamond();
            Deprecated_EnemyGroupCannonBotDiamond _diamondGroup2 = new Deprecated_EnemyGroupCannonBotDiamond();
            AddChild(_diamondGroup1);
            AddChild(_diamondGroup2);
            AddChild(_surroundGroup);

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
                        if (group is Deprecated_EnemyGroupCannonBotSurroundPlayer surrounder)
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
                        if (group is Deprecated_EnemyGroupCannonBotSurroundPlayer surrounder)
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
