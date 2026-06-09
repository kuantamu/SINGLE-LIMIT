using UnityEngine;
using UnityEngine.Playables;

public class EffectExampleMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform target = playerData as Transform;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            Playable input = playable.GetInput(i);

            if (!input.IsValid()) continue;
            if (input.GetPlayableType() != typeof(EffectExampleBehaviour)) continue;

            var inputPlayable = (ScriptPlayable<EffectExampleBehaviour>)input;
            EffectExampleBehaviour behaviour = inputPlayable.GetBehaviour();
            if (behaviour == null) continue;

            float weight = playable.GetInputWeight(i);
            bool isActive = weight > 0f;

        }
    }

    public override void OnGraphStop(Playable playable)
    {
    }
}
