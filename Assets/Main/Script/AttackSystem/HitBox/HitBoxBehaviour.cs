using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class HitBoxBehaviour : PlayableBehaviour
{
    public Vector3          Offset;
    public Vector3          Size;
    public float            HitInterval;
    public LayerMask        HitLayer;
    public List<IHitEffect> HitEffects;

    private GameObject _instance;
    private bool       _spawned;

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

        var hitTimes = new Dictionary<Collider, float>();

        var detector = _instance.AddComponent<HitBoxDetector>();
        detector.Init(HitLayer, HitInterval, hitTimes, OnHit, owner);
    }

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

    public override void OnPlayableDestroy(Playable playable) => Despawn();

    private void OnHit(Collider hitCollider)
    {
        if (HitEffects == null || _instance == null) return;

        Transform owner = _instance.transform.parent;

        foreach (var effect in HitEffects)
            effect.Execute(hitCollider, owner);
    }
}
