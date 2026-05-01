#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// HitBoxClip のカスタムインスペクター。
/// Timeline ウィンドウ閉鎖時の SerializedObject Disposed エラーを回避する。
/// </summary>
[CustomEditor(typeof(HitBoxClip))]
[CanEditMultipleObjects]
public class HitBoxClipEditor : Editor
{
    private SerializedProperty _offset;
    private SerializedProperty _size;
    private SerializedProperty _hitInterval;
    private SerializedProperty _hitLayer;
    private SerializedProperty _hitPointEffect;
    private SerializedProperty _damageNumber;
    private SerializedProperty _knockback;
    private SerializedProperty _skillPower;

    private void OnEnable()
    {
        _offset         = serializedObject.FindProperty("Offset");
        _size           = serializedObject.FindProperty("Size");
        _hitInterval    = serializedObject.FindProperty("HitInterval");
        _hitLayer       = serializedObject.FindProperty("HitLayer");
        _hitPointEffect = serializedObject.FindProperty("HitPointEffect");
        _damageNumber   = serializedObject.FindProperty("DamageNumber");
        _knockback      = serializedObject.FindProperty("Knockback");
        _skillPower      = serializedObject.FindProperty("SkillPower");
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject == null) return;

        try
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("判定の位置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_offset, new GUIContent("Offset"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("判定の大きさ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_size, new GUIContent("Size"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("多段ヒット設定", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hitInterval, new GUIContent("Hit Interval"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("対象レイヤー", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hitLayer, new GUIContent("Hit Layer"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("着弾エフェクト", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hitPointEffect, new GUIContent("Hit Point Effect"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("ダメージ数字", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_damageNumber, new GUIContent("Damage Number"), includeChildren: true);

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_knockback, new GUIContent("Knockback"), includeChildren: true);

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_skillPower, new GUIContent("SkillPower"), includeChildren: true);

            serializedObject.ApplyModifiedProperties();
        }
        catch (System.Exception)
        {
            // Timeline ウィンドウ閉鎖時のエラーを握りつぶす
        }
    }

    private void OnDisable()
    {
        serializedObject?.Dispose();
    }
}
#endif
