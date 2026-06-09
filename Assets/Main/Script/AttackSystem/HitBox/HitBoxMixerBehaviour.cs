using UnityEngine;
using UnityEngine.Playables;

public class HitBoxMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform owner = playerData as Transform;
        if (owner == null) return;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            Playable input = playable.GetInput(i);

            if (!input.IsValid()) continue;
            if (input.GetPlayableType() != typeof(HitBoxBehaviour)) continue;

            var inputPlayable = (ScriptPlayable<HitBoxBehaviour>)input;
            HitBoxBehaviour behaviour = inputPlayable.GetBehaviour();
            if (behaviour == null) continue;

            bool active = playable.GetInputWeight(i) > 0f;

            if (active)
                behaviour.Spawn(owner);
            else
                behaviour.Despawn();
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        DespawnAll(playable);
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        DespawnAll(playable);
    }

    private void DespawnAll(Playable playable)
    {
        if (!playable.IsValid()) return;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            Playable input = playable.GetInput(i);

            if (!input.IsValid()) continue;
            if (input.GetPlayableType() != typeof(HitBoxBehaviour)) continue;

            var inputPlayable = (ScriptPlayable<HitBoxBehaviour>)input;
            HitBoxBehaviour behaviour = inputPlayable.GetBehaviour();
            behaviour?.Despawn();
        }
    }
}
