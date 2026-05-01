using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// HitBoxTrack のミキサー。
/// ProcessFrame でクリップのアクティブ状態を監視し
/// Spawn / Despawn のタイミングを制御する。
/// </summary>
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

            // キャスト前に有効性と型を確認する
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
        // playable 自体が無効な場合は何もしない
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
