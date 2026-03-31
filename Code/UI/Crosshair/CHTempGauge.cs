using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class CHTempGauge : TextureProgressBar
    {
        [Export]
        private Color _tempGaugeGreen = Colors.Green;

        [Export]
        private Color _tempGaugeYellow = Colors.Yellow;

        [Export]
        private Color _tempGaugeOrange = Colors.Orange;

        [Export]
        private Color _tempGaugeRed = Colors.Red;

        /// <summary>
        /// When to shift from green to orange.
        /// </summary>
        [Export]
        private float _warningThreshold = 60f;

        /// <summary>
        /// When to shift from orange to red.
        /// </summary>
        [Export]
        private float _overheatThreshold = 80f;

        public bool IsOverheating;

        public override void _Ready()
        {
            Reset();
        }

        public override void _Process(double delta)
        {
            if (IsOverheating)
            {
                PlayOverheatEffect();
            }
        }

        public void SetGaugeFill(float fillPercent)
        {
            Value = fillPercent;
            if (fillPercent < _warningThreshold)
            {
                TintProgress = _tempGaugeGreen;
            }
            else if (fillPercent > _warningThreshold && fillPercent < _overheatThreshold)
            {
                TintProgress = _tempGaugeYellow;
            }
            else if (fillPercent > _overheatThreshold && !IsOverheating)
            {
                TintProgress = _tempGaugeOrange;
            }
            else if (IsOverheating)
            {
                TintProgress = _tempGaugeRed;
            }
        }

        public void Reset()
        {
            SetGaugeFill(0f);
            IsOverheating = false;
        }

        private void PlayOverheatEffect() { }
    }
}
