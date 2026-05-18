using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Timeline のクリップとして配置できる攻撃アシストアセット。
/// AttackAssistTrack 上に追加するとインスペクターで各パラメータを設定できる。
/// </summary>
[System.Serializable]
public class AttackAssistAsset : PlayableAsset, ITimelineClipAsset
{
    /// <summary>クリップごとの設定値テンプレート。インスペクターで編集される。</summary>
    public AttackAssistBehaviour template = new AttackAssistBehaviour();

    /// <summary>
    /// このクリップはブレンドや速度変更をサポートしない。
    /// ループを許可するとクリップ継続時間の再計算が狂うため None に設定。
    /// </summary>
    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // template の値をコピーして Playable を生成する
        ScriptPlayable<AttackAssistBehaviour> playable =
            ScriptPlayable<AttackAssistBehaviour>.Create(graph, template);
        return playable;
    }
}
