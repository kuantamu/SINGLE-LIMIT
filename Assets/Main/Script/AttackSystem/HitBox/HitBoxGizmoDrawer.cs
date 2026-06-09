using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
        Vector3    worldPos = transform.TransformPoint(clip.Offset);
        Quaternion worldRot = transform.rotation;
        Vector3    worldSize = Vector3.Scale(clip.Size, transform.lossyScale);

        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, worldRot, Vector3.one);
        Gizmos.matrix = matrix;

        if (isActive)
        {
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.25f);
            Gizmos.DrawCube(Vector3.zero, worldSize);
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Gizmos.DrawWireCube(Vector3.zero, worldSize);
        }
        else
        {
            Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
            Gizmos.DrawWireCube(Vector3.zero, worldSize);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
