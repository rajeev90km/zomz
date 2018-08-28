using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Being : MonoBehaviour {

    protected bool _isAlive = true;
    public bool IsAlive
    {
        get { return _isAlive; }
        set { _isAlive = value; }
    }

    public abstract IEnumerator Attack();

    public abstract IEnumerator Hurt(float pDamage = 0.0f);

}
