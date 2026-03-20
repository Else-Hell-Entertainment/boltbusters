using System;
using System.Globalization;
using EHE.BoltBusters;
using Godot;

public partial class RoundTimerUi : Control
{
    [Export]
    private Label _roundTimerLabel;

    public override void _Process(double delta)
    {
        if (LevelManager.Active.RoundInProgress)
        {
            string timeString = "";
            double timeRemaining = LevelManager.Active.GetRemainingRoundTime();
            if (timeRemaining > 5)
            {
                int time = (int)Math.Ceiling(timeRemaining);
                timeString = time.ToString();
            }
            else
            {
                timeString = timeRemaining.ToString(("0.##"), CultureInfo.InvariantCulture);
            }

            _roundTimerLabel.Text = timeString;
        }
    }
}
