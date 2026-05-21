using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HitBoxBehaviour が生成したオブジェクトにアタッチして
/// OnTriggerStay を受け取る MonoBehaviour。
///
/// OnTriggerStay は毎物理フレーム呼ばれるため、_hitTimes のエントリが
/// コライダーが Trigger を抜けた時に削除される必要がある。
/// さもなければメモリ膨張の原因になる。
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class HitBoxDetector : MonoBehaviour
{
    private LayerMask                   _hitLayer;
    private float                       _hitInterval;
    private Dictionary<Collider, float> _hitTimes;
    private Action<Collider>            _onHit;
    private Transform                   _ownerRoot; // 自分自身の除外に使う

    public void Init(
        LayerMask                   hitLayer,
        float                       hitInterval,
        Dictionary<Collider, float> hitTimes,
        Action<Collider>            onHit,
        Transform                   ownerRoot)
    {
        _hitLayer   = hitLayer;
        _hitInterval = hitInterval;
        _hitTimes   = hitTimes;
        _onHit      = onHit;
        _ownerRoot  = ownerRoot;
    }

    private void OnTriggerStay(Collider other)
    {
        // レイヤーチェック
        if ((_hitLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // 自分自身（攻撃者の階層）への誤検知を除外
        if (_ownerRoot != null && other.transform.IsChildOf(_ownerRoot)) return;

        // 無敵中のキャラクターはヒット履歴にも残さず、完全に当たらない扱いにする。
        CharacterStats stats = other.GetComponentInParent<CharacterStats>();
        if (stats != null && !stats.CanReceiveHit) return;

        // 再ヒット間隔チェック
        if (_hitTimes.TryGetValue(other, out float lastTime))
        {
            if (_hitInterval <= 0f) return; // 1回のみの場合
            if (Time.time - lastTime < _hitInterval) return; // インターバル未経過
        }

        _hitTimes[other] = Time.time;
        _onHit?.Invoke(other);
    }

    /// <summary>
    /// Trigger を抜けたコライダーのヒット履歴を削除する。
    /// これにより大量敵の多段ヒット後にメモリが膨張するのを防ぐ。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        _hitTimes.Remove(other);
    }
}
