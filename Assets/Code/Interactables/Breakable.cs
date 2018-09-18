using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Breakable : Interactable 
{
    [SerializeField]
    private float _durability = 10f;
    public float Durability
    {
        get { return _durability; }    
    }

    [SerializeField]
    private GameObject _breakFX;

    public void Damage(float pDamage)
    {
        if (_durability - pDamage > 0)
            _durability -= pDamage;
        else
            _durability = 0f;

        if(_durability<=0.1f)
        {
            Break();    
        }
    }

	public void Break()
	{
        if(_breakFX)
        {
            GameObject breakFXObj = Instantiate(_breakFX);
            breakFXObj.transform.position = transform.position;
        }

        Destroy(gameObject);
	}

}
