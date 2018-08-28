using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtable : Interactable {

    [SerializeField]
    private float _hurtAmount = 10f;

	private void OnCollisionEnter(Collision pCollision)
	{
        OnCollide(pCollision);
	}

	public override void OnCollide(Collision pCollision)
	{
        base.OnCollide(pCollision);

        Being otherBeing = pCollision.gameObject.GetComponent<Being>();

        Debug.Log(otherBeing);

        if(otherBeing!=null && otherBeing.IsAlive)
        {
            otherBeing.StartCoroutine(otherBeing.Hurt(_hurtAmount));
        }
	}

}
