
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EffectPlayableBehaviour : PlayableBehaviour
{
    public TimelineClip[] Clips { get; set; }
    public PlayableDirector Director { get; set; }
    public ParticleSystem particleSystem { get; set; }
    public EffectExampleBehaviour Example_tf { get; set; }

    ParticleSystem ps;

    public override void OnGraphStart(Playable playable)
    {
        
    }

    public override void OnGraphStop(Playable playable)
    {
        
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {

    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (ps != null)
        {
            GameObject.DestroyImmediate(ps.gameObject);
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (ps == null)
        {
            ps = ParticleSystem.Instantiate(particleSystem,playerData as Transform);
            ps.transform.localPosition = Example_tf.Position;
            ps.transform.localRotation = 
                Quaternion.Euler(Example_tf.Rotation.x, Example_tf.Rotation.y, Example_tf.Rotation.z);
            ps.transform.localScale = Example_tf.Scale;
            ps.Play();
            return;
        }

        if (ps != null)
        {
            ps.Simulate((float)playable.GetTime(), true, true, false);
        }
    }
}
