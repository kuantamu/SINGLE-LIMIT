using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline 上でエフェクト（ParticleSystem、VFX など）を再生する Behaviour。
/// 
/// ■ 仕様
///   - EffectPrefab: 再生する GameObject Prefab（ParticleSystem/VFX Graph など任意）
///   - Position / Rotation / Scale: ローカル座標での配置設定
///   - Timeline の再生速度・シークに完全同期（絶対時刻で Simulate）
///   - Timeline の開始・停止タイミングに対応
/// </summary>
[System.Serializable]
public class EffectExampleBehaviour : PlayableBehaviour
{
    [Header("エフェクト")]
    public GameObject EffectPrefab;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    private GameObject _effectInstance;
    private ParticleSystem _particleSystemComponent;
    private Transform _parentTransform;

    /// <summary>
    /// クリップが再生開始された時に呼ばれる。
    /// エフェクト Prefab をインスタンス化する。
    /// </summary>
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // 既にインスタンスがある場合はスキップ
        if (_effectInstance != null) return;

        if (EffectPrefab == null)
        {
            Debug.LogWarning("[EffectExampleBehaviour] Effect Prefab が設定されていません。");
            return;
        }

        // GameObject をインスタンス化
        // 親が指定されている場合はそこに、ない場合はワールド座標に生成
        if (_parentTransform != null)
        {
            _effectInstance = Object.Instantiate(EffectPrefab, _parentTransform);
        }
        else
        {
            _effectInstance = Object.Instantiate(EffectPrefab);
        }

        if (_effectInstance != null)
        {
            // ローカル座標で配置
            _effectInstance.transform.localPosition = Position;
            _effectInstance.transform.localRotation = Quaternion.Euler(Rotation);
            _effectInstance.transform.localScale = Scale;

            // ParticleSystem があれば取得（Timeline 速度同期用）
            _particleSystemComponent = _effectInstance.GetComponent<ParticleSystem>();
            if (_particleSystemComponent != null)
            {
                // 自動再生を無効化（Simulate で制御するため）
                var main = _particleSystemComponent.main;
                main.playOnAwake = false;
            }
        }
    }

    /// <summary>
    /// クリップが停止・ポーズされた時に呼ばれる。
    /// </summary>
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (_particleSystemComponent != null)
        {
            _particleSystemComponent.Stop();
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる。
    /// Timeline の再生時間（絶対時刻）に合わせてパーティクルをシミュレートする。
    /// 速度変更・シーク操作に完全対応。
    /// </summary>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Transform バインドから親を取得
        Transform target = playerData as Transform;
        if (target != null)
            _parentTransform = target;

        // インスタンスが生成されていない場合はスキップ
        if (_effectInstance == null) return;

        // 親が設定されている場合、親の子として設定
        if (_parentTransform != null && _effectInstance.transform.parent != _parentTransform)
        {
            _effectInstance.transform.SetParent(_parentTransform, worldPositionStays: false);
        }

        // ParticleSystem がある場合、Timeline 時刻で Simulate
        if (_particleSystemComponent != null)
        {
            // Timeline の絶対時刻を取得
            float timelineTime = (float)playable.GetTime();
            
            // ParticleSystem をシミュレート（Timeline 時刻に同期）
            // 速度変更やシークにも自動対応
            _particleSystemComponent.Simulate(timelineTime, true, true, false);
        }
    }

    /// <summary>
    /// Timeline が破棄される時に呼ばれる。
    /// インスタンスをクリーンアップする。
    /// </summary>
    public override void OnPlayableDestroy(Playable playable)
    {
        if (_effectInstance != null)
        {
            // エディターモード対応：DestroyImmediate を使用
            if (Application.isEditor && !Application.isPlaying)
            {
                Object.DestroyImmediate(_effectInstance);
            }
            else
            {
                Object.Destroy(_effectInstance);
            }
            
            _effectInstance = null;
            _particleSystemComponent = null;
        }
    }
}

