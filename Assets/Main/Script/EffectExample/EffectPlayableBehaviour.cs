
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// A behaviour that is attached to a playable
public class EffectPlayableBehaviour : PlayableBehaviour
{
    public TimelineClip[] Clips { get; set; }
    public PlayableDirector Director { get; set; }
    public ParticleSystem particleSystem { get; set; }
    public EffectExampleBehaviour Example_tf { get; set; }

    ParticleSystem ps;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable)
    {
        
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {

    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (ps != null)
        {
            GameObject.DestroyImmediate(ps.gameObject);
        }
    }

    // Called each frame while the state is set to Play
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (ps == null)
        {
            ps = ParticleSystem.Instantiate(particleSystem,playerData as Transform);
            ps.transform.localPosition = Example_tf.Position;
            ps.transform.localRotation = 
                Quaternion.Euler(Example_tf.Rotate.x, Example_tf.Rotate.y, Example_tf.Rotate.z);
            ps.transform.localScale = Example_tf.Scale;
            ps.Play();
            return;
        }

        if (ps != null)
        {
            // Timelineの時間をパーティクルのシミュレーション時間に同期
            ps.Simulate((float)playable.GetTime(), true, true, false);
        }
    }
}
