using UnityEngine;
using TMPro;

/// <summary>
/// ダメージ数字ポップアップ。
/// TextMeshPro（3D版）を使用してワールド空間に直接描画する。
/// Canvas は使用しない。
///
/// ■ Prefab の構成
///   GameObject（DamageNumberPopup をアタッチ）
///   └── TextMeshPro - Text（3D版・UI版ではない）← _text にアタッチ
///
/// ■ Prefab の配置場所
///   Resources/DamageNumberPopup という名前で保存すること。
/// </summary>
public class DamageNumberPopup : MonoBehaviour
{
    private const string ResourcePath = "DamageNumberPopup";

    [SerializeField] private TextMeshPro _text; // 3D版 TextMeshPro
    [SerializeField] private TextMeshPro _damageTypeText;
    [SerializeField] private TextMeshPro _resistanceTypeText;

    private float  _floatSpeed;
    private float  _duration;
    private float  _elapsed;
    private Color  _startColor;
    private Camera _cam;

    // ---- Prefab キャッシュ ----
    private static GameObject _cachedPrefab;

    public static GameObject GetPrefab()
    {
        if (_cachedPrefab != null) return _cachedPrefab;

        _cachedPrefab = Resources.Load<GameObject>(ResourcePath);

        if (_cachedPrefab == null)
            Debug.LogWarning(
                $"[DamageNumberPopup] Resources/{ResourcePath} が見つかりません。" +
                "Prefab を Resources フォルダに配置してください。");

        return _cachedPrefab;
    }

    /// <summary>生成直後に呼ぶ初期化メソッド。</summary>
    public void Init(
        int   damage,
        Color color,
        float fontSize,
        float floatSpeed,
        float duration,
        AttributeType damageType,
        ResistanceLevel resistanceLevel)
    {
        _floatSpeed = floatSpeed;
        _duration   = duration;
        _elapsed    = 0f;
        _startColor = color;
        _cam        = Camera.main;

        _text.text     = damage.ToString();
        _text.color    = color;
        _text.fontSize = fontSize;

        _damageTypeText.text = damageType.ToString();
        Debug.Log(resistanceLevel);
        _resistanceTypeText.text = resistanceLevel.ToString();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        // 上昇
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

        // フェードアウト
        Color c = _startColor;
        c.a         = Mathf.Lerp(1f, 0f, _elapsed / _duration);
        _text.color = c;

        // カメラの方を向く（ビルボード）
        if (_cam != null)
            transform.forward = _cam.transform.forward;

        if (_elapsed >= _duration)
            Destroy(gameObject);
    }
}
