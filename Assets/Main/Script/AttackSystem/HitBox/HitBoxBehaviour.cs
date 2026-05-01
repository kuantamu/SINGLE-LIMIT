using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// HitBoxClip の再生動作。
/// 生成処理は HitBoxMixerBehaviour.ProcessFrame から呼ぶ。
/// OnBehaviourPlay では生成しない（Owner がまだ null のため）。
/// </summary>
public class HitBoxBehaviour : PlayableBehaviour
{
    // ---- クリップから渡される設定 ----
    public Vector3          Offset;
    public Vector3          Size;
    public float            HitInterval;
    public LayerMask        HitLayer;
    public List<IHitEffect> HitEffects;

    // ---- 内部 ----
    private GameObject _instance;
    private bool       _spawned;

    /// <summary>
    /// ミキサーの ProcessFrame から呼ぶ。
    /// Owner が確定してから初めて呼ばれる。
    /// </summary>
    public void Spawn(Transform owner)
    {
        if (_spawned || !Application.isPlaying || owner == null) return;
        _spawned = true;

        _instance = new GameObject("HitBox_Runtime");
        _instance.transform.SetParent(owner, worldPositionStays: false);
        _instance.transform.localPosition = Offset;
        _instance.transform.localRotation = Quaternion.identity;

        var col       = _instance.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = Size;
        col.center    = Vector3.zero;

        // 攻撃ごとに新しい辞書を作成することで前の攻撃のヒット履歴を持ち越さない
        var hitTimes = new Dictionary<Collider, float>();

        var detector = _instance.AddComponent<HitBoxDetector>();
        detector.Init(HitLayer, HitInterval, hitTimes, OnHit, owner);
    }

    /// <summary>ミキサーの ProcessFrame からクリップが非アクティブになった時に呼ぶ。</summary>
    public void Despawn()
    {
        if (!_spawned) return;
        _spawned = false;

        if (_instance != null)
        {
            Object.Destroy(_instance);
            _instance = null;
        }
    }

    // Timeline が破棄された時の安全弁
    public override void OnPlayableDestroy(Playable playable) => Despawn();

    // ---- ヒット処理 ----

    private void OnHit(Collider hitCollider)
    {
        if (HitEffects == null || _instance == null) return;

        // _instance の親が Owner（アタッカーの Transform）
        Transform owner = _instance.transform.parent;

        foreach (var effect in HitEffects)
            effect.Execute(hitCollider, owner);
    }
}
