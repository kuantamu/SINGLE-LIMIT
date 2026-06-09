using UnityEngine;

public interface IHitEffect
{
    void Execute(Collider hitCollider, Transform attacker);
}
