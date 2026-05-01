using UnityEngine;

/// <summary>
/// HitPointEffect の設定を保持する ScriptableObject。
/// Assets を右クリック → Create → HitEffects → HitPointEffectData で作成する。
/// </summary>
[CreateAssetMenu(
    fileName = "HitPointEffectData",
    menuName  = "HitEffects/HitPointEffectData")]
public class HitPointEffectData : ScriptableObject
{
    [Header("着弾エフェクト")]
    [Tooltip("着弾点に生成する ParticleSystem の Prefab")]
    public GameObject EffectPrefab;

    [Tooltip("エフェクトの自動削除時間（秒）。0 で自動削除しない）")]
    public float AutoDestroyTime = 2f;

    [Header("レイの設定")]
    [Tooltip("着弾点を探すレイの最大距離")]
    public float RayMaxDistance = 10f;

    [Tooltip("レイが当たるレイヤー。エフェクトを出したいオブジェクトのレイヤーを設定する")]
    public LayerMask RayLayer;
}
