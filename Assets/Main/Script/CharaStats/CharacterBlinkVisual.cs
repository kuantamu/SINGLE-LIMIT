using System.Collections.Generic;
using System;
using UnityEngine;

public class CharacterBlinkVisual : MonoBehaviour
{
    private Renderer[] _renderers;

    private readonly List<MaterialPropertyBlock> _rendererBlocks = new List<MaterialPropertyBlock>();

    private Color[] _baseColors;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");


    [SerializeField] public Color _invincibleBlinkColor = Color.cyan;

    [SerializeField] public float _invincibleBlinkSpeed = 12f;

    private HitReactionState _lastAppliedHitReactionState;

    public void CacheRenderers(HitReactionState _hitReactionState)
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _baseColors = new Color[_renderers.Length];
        _rendererBlocks.Clear();

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            Material sharedMaterial = renderer.sharedMaterial;

            _baseColors[i] = sharedMaterial != null && sharedMaterial.HasProperty(BaseColorId)
                ? sharedMaterial.GetColor(BaseColorId)
                : sharedMaterial != null && sharedMaterial.HasProperty(ColorId)
                    ? sharedMaterial.GetColor(ColorId)
                    : Color.white;

            _rendererBlocks.Add(new MaterialPropertyBlock());
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }

    public void UpdateHitReactionVisual(bool IsInvincible, HitReactionState _hitReactionState)
    {
        if (_renderers == null || _renderers.Length == 0) return;

        bool shouldBlink = IsInvincible;

        if (!shouldBlink && _lastAppliedHitReactionState == _hitReactionState) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            if (renderer == null) continue;

            MaterialPropertyBlock block = _rendererBlocks[i];
            renderer.GetPropertyBlock(block);

            Color color = shouldBlink
                ? Color.Lerp(_baseColors[i], _invincibleBlinkColor,
                    Mathf.PingPong(Time.time * _invincibleBlinkSpeed, 1f))
                : _baseColors[i];

            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }
}
