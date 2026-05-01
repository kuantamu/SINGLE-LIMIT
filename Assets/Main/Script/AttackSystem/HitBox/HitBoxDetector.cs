using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HitBoxBehaviour が生成したオブジェクトにアタッチして
/// OnTriggerStay を受け取る MonoBehaviour。
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

        // 再ヒット間隔チェック
        if (_hitTimes.TryGetValue(other, out float lastTime))
        {
            if (_hitInterval <= 0f) return;
            if (Time.time - lastTime < _hitInterval) return;
        }

        _hitTimes[other] = Time.time;
        _onHit?.Invoke(other);
    }
}
