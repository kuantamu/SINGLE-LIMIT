using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HitBoxDetector : MonoBehaviour
{
    private LayerMask _hitLayer;
    private float _hitInterval;
    private Dictionary<Collider, float> _hitTimes;
    private Action<Collider> _onHit;
    private Transform _ownerRoot;
    private CharacterStats _ownerStats;

    public void Init(
        LayerMask hitLayer,
        float hitInterval,
        Dictionary<Collider, float> hitTimes,
        Action<Collider> onHit,
        Transform ownerRoot)
    {
        _hitLayer = hitLayer;
        _hitInterval = hitInterval;
        _hitTimes = hitTimes;
        _onHit = onHit;
        _ownerRoot = ownerRoot;
        _ownerStats = CharacterFactionRules.GetCharacterStats(ownerRoot);
    }

    private void OnTriggerStay(Collider other)
    {
        if ((_hitLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (_ownerRoot != null && other.transform.IsChildOf(_ownerRoot)) return;

        CharacterStats stats = other.GetComponentInParent<CharacterStats>();
        if (stats == null) return;
        if (!CharacterFactionRules.CanAttack(_ownerStats, stats)) return;
        if (!stats.CanReceiveHit) return;

        if (_hitTimes.TryGetValue(other, out float lastTime))
        {
            if (_hitInterval <= 0f) return;
            if (Time.time - lastTime < _hitInterval) return;
        }

        _hitTimes[other] = Time.time;
        _onHit?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _hitTimes.Remove(other);
    }
}
