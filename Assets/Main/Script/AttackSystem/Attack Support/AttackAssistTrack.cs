using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 攻撃アシストを管理する Timeline トラック。
///
/// ■ セットアップ手順
///   1. PlayableDirector が付いた GameObject の Timeline を開く
///   2. このトラックを追加し、Binding 欄に AttackAssistController をアサインする
///   3. トラック上にクリップを配置し、各クリップの設定を Inspector で調整する
///      - moveDistance       : 移動量（m）
///      - minApproachDistance: 敵への最小接近距離（m）
///      - useWarp            : false = スムーズ移動 / true = ワープ
///      - wallLayer          : ワープ時の壁判定レイヤー
///      - wallCheckRadius    : ワープ時の SphereCast 半径
///   4. クリップの長さがスムーズ移動の所要時間になる（クリップを短くすると速く移動）
/// </summary>
[TrackColor(1f, 0.35f, 0.1f)]                       // Timeline 上でオレンジ色表示
[TrackClipType(typeof(AttackAssistAsset))]           // このトラックに配置できるクリップ型
[TrackBindingType(typeof(AttackAssistController))]   // バインド対象のコンポーネント型
public class AttackAssistTrack : TrackAsset
{
    // ─── カスタムミキサー ─────────────────────────────────────────────
    // 複数クリップのブレンドは行わないが、ミキサーで playerData を
    // 各 Behaviour の ProcessFrame に正しく渡すために定義する。

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<AttackAssistMixerBehaviour>.Create(graph, inputCount);
    }
}

/// <summary>
/// 攻撃アシストトラックのミキサー。
/// クリップのブレンドは行わず、アクティブなクリップの Behaviour にそのまま委譲する。
/// </summary>
public class AttackAssistMixerBehaviour : PlayableBehaviour
{
    // 攻撃アシストは同時に複数クリップをブレンドしない設計のため、
    // ミキサー自体の処理は不要。各 Behaviour の ProcessFrame が個別に動作する。
}
