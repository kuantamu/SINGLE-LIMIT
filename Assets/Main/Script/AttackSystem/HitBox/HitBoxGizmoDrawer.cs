using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Scene パネルにヒットボックスの Gizmo を描画するコンポーネント。
/// キャラクター（PlayableDirector と同じ GameObject）にアタッチする。
///
/// ■ 表示内容
///   ・Timeline 上の全 HitBoxClip の位置とサイズをワイヤーフレームで表示
///   ・Timeline を再生すると有効なクリップが赤、無効なクリップが灰色で表示
///   ・再生していない時はすべて灰色で表示（配置の確認用）
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class HitBoxGizmoDrawer : MonoBehaviour
{
#if UNITY_EDITOR
    private PlayableDirector _director;

    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();
    }

    private void OnDrawGizmos()
    {
        _director = GetComponent<PlayableDirector>();
        if (_director == null || _director.playableAsset == null) return;

        TimelineAsset timeline = _director.playableAsset as TimelineAsset;
        if (timeline == null) return;

        double currentTime = _director.time;

        foreach (TrackAsset track in timeline.GetOutputTracks())
        {
            if (track is not HitBoxTrack) continue;

            foreach (TimelineClip clip in track.GetClips())
            {
                HitBoxClip hitBoxClip = clip.asset as HitBoxClip;
                if (hitBoxClip == null) continue;

                bool isActive = currentTime >= clip.start && currentTime <= clip.end;

                DrawHitBoxGizmo(hitBoxClip, isActive);
            }
        }
    }

    private void DrawHitBoxGizmo(HitBoxClip clip, bool isActive)
    {
        // キャラクターのローカル座標をワールド座標に変換
        Vector3    worldPos = transform.TransformPoint(clip.Offset);
        Quaternion worldRot = transform.rotation;
        Vector3    worldSize = Vector3.Scale(clip.Size, transform.lossyScale);

        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, worldRot, Vector3.one);
        Gizmos.matrix = matrix;

        if (isActive)
        {
            // 有効時：赤の半透明塗りつぶし + 赤のワイヤーフレーム
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.25f);
            Gizmos.DrawCube(Vector3.zero, worldSize);
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Gizmos.DrawWireCube(Vector3.zero, worldSize);
        }
        else
        {
            // 無効時：薄い灰色のワイヤーフレームのみ
            Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
            Gizmos.DrawWireCube(Vector3.zero, worldSize);
        }

        // matrix をリセット
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
