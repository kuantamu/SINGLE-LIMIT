using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Plays an effect prefab from Timeline.
/// ParticleSystems are sampled with Simulate so seeking and speed changes stay in sync.
/// </summary>
[System.Serializable]
public class EffectExampleBehaviour : PlayableBehaviour
{
    [Header("Effect")]
    public GameObject EffectPrefab;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    private GameObject _effectInstance;
    private ParticleSystem[] _particleSystems;
    private Transform _parentTransform;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        EnsureInstance();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        StopParticles();
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform target = playerData as Transform;
        if (target != null)
            _parentTransform = target;

        EnsureInstance();
        if (_effectInstance == null) return;

        if (_parentTransform != null && _effectInstance.transform.parent != _parentTransform)
        {
            _effectInstance.transform.SetParent(_parentTransform, worldPositionStays: false);
            ApplyLocalTransform();
        }

        SimulateParticles((float)playable.GetTime());
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        DestroyInstance();
    }

    private void EnsureInstance()
    {
        if (_effectInstance != null) return;

        if (EffectPrefab == null)
        {
            Debug.LogWarning("[EffectExampleBehaviour] Effect Prefab is not assigned.");
            return;
        }

        _effectInstance = _parentTransform != null
            ? Object.Instantiate(EffectPrefab, _parentTransform)
            : Object.Instantiate(EffectPrefab);

        ApplyLocalTransform();

        _particleSystems = _effectInstance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void ApplyLocalTransform()
    {
        if (_effectInstance == null) return;

        _effectInstance.transform.localPosition = Position;
        _effectInstance.transform.localRotation = Quaternion.Euler(Rotation);
        _effectInstance.transform.localScale = Scale;
    }

    private void SimulateParticles(float clipTime)
    {
        if (_particleSystems == null || _particleSystems.Length == 0) return;

        SimulateAt(Mathf.Max(0f, clipTime));
    }

    private void SimulateAt(float effectTime)
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            if (particleSystem == null) continue;

            particleSystem.Simulate(effectTime, withChildren: false, restart: true, fixedTimeStep: false);
        }
    }

    private void StopParticles()
    {
        if (_particleSystems == null) return;

        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            if (particleSystem == null) continue;

            particleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void DestroyInstance()
    {
        if (_effectInstance == null) return;

        if (Application.isEditor && !Application.isPlaying)
        {
            Object.DestroyImmediate(_effectInstance);
        }
        else
        {
            Object.Destroy(_effectInstance);
        }

        _effectInstance = null;
        _particleSystems = null;
    }
}
