using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageNumberPopup : MonoBehaviour
{
    private const string ResourcePath = "DamageNumberPopup";

    [SerializeField] private TextMeshPro _text; // 3D版 TextMeshPro
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Sprite _slashImage;
    [SerializeField] private Sprite _pierceImage;
    [SerializeField] private Sprite _strikeImage;
    [SerializeField] private TextMeshPro _resistanceTypeText;

    private float  _floatSpeed;
    private float  _duration;
    private float  _elapsed;
    private Color  _startColor;
    private Camera _cam;

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
        
        if (_cam == null)
            _cam = Camera.main;

        if (_text != null)
        {
            _text.text     = damage.ToString();
            _text.color    = color;
            _text.fontSize = fontSize;
        }

        switch (damageType)
        {
            case AttributeType.Slash:
                _sprite.sprite = _slashImage;
                break;
            case AttributeType.Strike:
                _sprite.sprite = _strikeImage;
                break;
            case AttributeType.Pierce:
                _sprite.sprite = _pierceImage;
                break;
        }

        if (_resistanceTypeText != null)
            _resistanceTypeText.text = resistanceLevel.ToString();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

        if (_text != null)
        {
            Color c = _startColor;
            c.a         = Mathf.Lerp(1f, 0f, _elapsed / _duration);
            _text.color = c;
        }

        if (_cam != null)
            transform.forward = _cam.transform.forward;

        if (_elapsed >= _duration)
            Destroy(gameObject);
    }
}
