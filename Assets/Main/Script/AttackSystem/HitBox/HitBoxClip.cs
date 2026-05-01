using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// HitBoxTrack に配置するクリップ。
/// </summary>
[System.Serializable]
public class HitBoxClip : PlayableAsset, ITimelineClipAsset
{
    [Header("判定の位置（キャラクターのローカル座標）")]
    public Vector3 Offset = new Vector3(0f, 0.5f, 0.8f);

    [Header("判定の大きさ")]
    public Vector3 Size = new Vector3(1f, 1f, 1f);

    [Header("多段ヒット設定")]
    [Tooltip("同一対象への再ヒットまでの間隔（秒）。0 で1回のみ")]
    public float HitInterval = 0.3f;

    [Header("対象レイヤー")]
    public LayerMask HitLayer;

    [Header("着弾エフェクト設定")]
    [Tooltip("設定しない場合はエフェクトなし")]
    public HitPointEffectData HitPointEffect;

    [Header("ダメージ数字設定")]
    public DamageNumberEffect DamageNumber = new DamageNumberEffect();

    [Header("ノックバック設定")]
    public KnockbackSettings Knockback = new KnockbackSettings();

    [Header("技威力倍率設定")]
    public float SkillPower;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable  = ScriptPlayable<HitBoxBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.Offset      = Offset;
        behaviour.Size        = Size;
        behaviour.HitInterval = HitInterval;
        behaviour.HitLayer = HitLayer;

        #region HIT設定
        //HitEffectに関するデータのやり取り
        var effects = new List<IHitEffect>
        {
            DamageNumber,
            new DamageHitEffect { DamageEffectSkillPower = SkillPower },
            new KnockbackHitEffect { Settings = Knockback },
            
        };
        //HITした場合の行動
        if (HitPointEffect != null)
            effects.Add(new HitPointEffect { Data = HitPointEffect });
        #endregion
        behaviour.HitEffects = effects;

        return playable;
    }
}
