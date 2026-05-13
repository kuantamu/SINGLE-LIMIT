using UnityEngine;

public static class EffectExampleDurationUtility
{
    public static float GetEffectDuration(GameObject effectPrefab)
    {
        if (effectPrefab == null) return 0f;

        ParticleSystem[] particleSystems = effectPrefab.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
        float maxDuration = 0f;

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null) continue;

            ParticleSystem.MainModule main = particleSystem.main;
            float particleDuration =
                main.startDelay.GetMaxValue() +
                main.duration +
                main.startLifetime.GetMaxValue();

            maxDuration = Mathf.Max(maxDuration, particleDuration);
        }

        return maxDuration;
    }

    private static float GetMaxValue(this ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.Curve:
                return GetCurveMaxValue(curve.curve) * curve.curveMultiplier;
            case ParticleSystemCurveMode.TwoCurves:
                return Mathf.Max(
                    GetCurveMaxValue(curve.curveMin),
                    GetCurveMaxValue(curve.curveMax)) * curve.curveMultiplier;
            default:
                return 0f;
        }
    }

    private static float GetCurveMaxValue(AnimationCurve curve)
    {
        if (curve == null || curve.length == 0) return 0f;

        float maxValue = float.MinValue;
        foreach (Keyframe key in curve.keys)
        {
            maxValue = Mathf.Max(maxValue, key.value);
        }

        return Mathf.Max(0f, maxValue);
    }
}
