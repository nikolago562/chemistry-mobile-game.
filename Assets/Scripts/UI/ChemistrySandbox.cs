using System;
using UnityEngine;

namespace ChemistryGame
{
    public static class ChemistrySandbox
    {
        public static float Concentration(float moles, float litres) => moles / Mathf.Max(0.001f, litres);
        // Strong monoprotic acid/base, equal 0.1 mol/L stocks, Kw = 1e-14 at 25 C.
        public static double PH(double acidML, double baseML, double waterML)
        {
            double delta = 0.1 * (acidML - baseML) / Math.Max(0.1, acidML + baseML + waterML);
            double root = Math.Sqrt(delta * delta + 4e-14);
            double hydrogen = delta >= 0 ? (delta + root) / 2 : 2e-14 / (root - delta);
            return -Math.Log10(hydrogen);
        }
        /// <summary>Red-cabbage indicator colour for a pH.</summary>
        public static Color Indicator(float ph)
        {
            Color[] stops = { new Color(0.85f, 0.15f, 0.25f), new Color(0.9f, 0.35f, 0.6f), new Color(0.6f, 0.3f, 0.75f), new Color(0.45f, 0.35f, 0.8f),
                new Color(0.3f, 0.45f, 0.9f), new Color(0.2f, 0.65f, 0.75f), new Color(0.3f, 0.75f, 0.35f), new Color(0.9f, 0.85f, 0.25f) };
            float t = Mathf.InverseLerp(2, 12, ph) * (stops.Length - 1);
            int i = Mathf.Clamp(Mathf.FloorToInt(t), 0, stops.Length - 2);
            return Color.Lerp(stops[i], stops[i + 1], t - i);
        }
    }
}
