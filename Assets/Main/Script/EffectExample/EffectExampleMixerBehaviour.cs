using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// EffectExampleTrack のミキサー。
/// 複数のエフェクトクリップのアクティブ状態を監視・制御する。
/// </summary>
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

            // クリップの重みを取得（アクティブな状態を判定）
            float weight = playable.GetInputWeight(i);
            bool isActive = weight > 0f;

            // アクティブな場合は処理を継続、非アクティブなら停止
            // （実際のエフェクト制御は EffectExampleBehaviour の OnBehaviourPlay/Pause で行う）
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        // Timeline が停止した時、特に何もしない
        // OnPlayableDestroy は Timeline システムが自動的に呼ぶ
    }
}
