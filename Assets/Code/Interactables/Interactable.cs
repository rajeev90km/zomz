using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    public virtual void OnInteract()
    {
        
    }

    public virtual void OnCollide(Collision pCollision)
    {
        
    }

    public virtual void OnParticleCollide(GameObject pOther)
    {
        
    }

    public virtual void OnTriggerEnter(Collider pOther)
    {
        
    }

}
