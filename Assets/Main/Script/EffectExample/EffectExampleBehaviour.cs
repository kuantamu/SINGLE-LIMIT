using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class EffectExampleBehaviour : PlayableBehaviour
{
    public ParticleSystem ParticleObj;
    public Vector3 Position;
    public Vector3 Rotate;
    public Vector3 Scale = new Vector3(1,1,1);
}
