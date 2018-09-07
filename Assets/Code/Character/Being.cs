using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Being : MonoBehaviour 
{    
    protected bool _isAlive = true;
    public bool IsAlive
    {
        get { return _isAlive; }
        set { _isAlive = value; }
    }

    public abstract IEnumerator Attack();

    public abstract IEnumerator Hurt(float pDamage = 0.0f);

    //Get A Being based on layermask in look range of character
    public Being GetBeingInLookRange(int pLayerMask, float pRange)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pRange, pLayerMask);

        foreach (Collider hit in colliders)
        {
            Being being = hit.gameObject.GetComponent<Being>();

            if ((hit.GetComponent<Collider>() == transform.GetComponent<Collider>()))
            {
                continue;
            }

            if (being != null && !being.IsAlive)
            {
                continue;
            }

            float distanceToBeing = Vector3.Distance(transform.position, being.transform.position);

            if (distanceToBeing <= pRange)
            {
                return being;
            }

        }

        return null;
    }


    //Get closest being to attack in attack range based on layer mask
    public Being GetClosestBeingToAttack(int pLayerMask, float pAttackRange)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pAttackRange, pLayerMask);
        Being closestBeing = null;

        foreach (Collider hit in colliders)
        {
            Being being = hit.gameObject.GetComponent<Being>();

            if ((hit.GetComponent<Collider>() == transform.GetComponent<Collider>()))
            {
                continue;
            }

            if (being != null && !being.IsAlive)
            {
                continue;
            }

            if (!closestBeing)
            {
                closestBeing = being;
            }
            //compares distances
            if (Vector3.Distance(transform.position, being.transform.position) <= Vector3.Distance(transform.position, closestBeing.transform.position))
            {
                closestBeing = being;
            }
        }

        return closestBeing;
    }
}
