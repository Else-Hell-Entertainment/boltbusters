// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using System;
using System.Globalization;
using EHE.BoltBusters;
using Godot;

public partial class RoundTimerUi : Control
{
    [Export]
    private Label _roundTimerLabel;

    [Export]
    private float _alarmPitchMultiplier = 0.1f;

    [Export]
    private float _alarmTimeThreshold = 6f;

    [Export]
    private float _alarmVolumeDb = 9f;

    public override void _EnterTree()
    {
        CallDeferred(MethodName.ConnectSignals);
    }

    public override void _Process(double delta)
    {
        if (LevelManager.Active.RoundInProgress)
        {
            string timeString = "";
            double timeRemaining = LevelManager.Active.GetRemainingRoundTime();
            if (timeRemaining > _alarmTimeThreshold)
            {
                int time = (int)Math.Ceiling(timeRemaining);
                timeString = time.ToString();
            }
            else
            {
                timeString = timeRemaining.ToString(("0.##"), CultureInfo.InvariantCulture);
                float alarmPitch = 1.0f + _alarmPitchMultiplier * (_alarmTimeThreshold + 1f - (float)timeRemaining);
                if (!MusicManager.Instance.AlarmSFX.IsPlaying() && timeRemaining % 1f > 0.9f)
                {
                    MusicManager.Instance.PlayAlarmSound(alarmPitch, _alarmVolumeDb);
                }
            }

            _roundTimerLabel.Text = timeString;
        }
    }

    private void ResetTimer()
    {
        _roundTimerLabel.Text = "--:--";
    }

    private void ConnectSignals()
    {
        if (LevelManager.Active != null)
        {
            LevelManager.Active.Initialized += ResetTimer;
        }
    }
}
