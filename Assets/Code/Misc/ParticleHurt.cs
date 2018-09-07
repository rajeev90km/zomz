using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHurt : MonoBehaviour {

    [SerializeField]
    private float _perParticleHurtAmount = 10f;

    [SerializeField]
    private int _hurtThreshold = 100;

    int particleCount;

	private void OnParticleCollision(GameObject other)
	{
        if(other.CompareTag("ParticleHurt"))
        {
            particleCount += 1;

            if(particleCount == _hurtThreshold)
            {
                StartHurting();
            }
        }
	}

    private void StartHurting()
    {
        particleCount = 0;

        if(gameObject.CompareTag("Player") || gameObject.CompareTag("Enemy") || gameObject.CompareTag("Human")) 
        {
            Being being = GetComponent<Being>();

            if (being!=null && being.IsAlive)
                being.StartCoroutine(being.Hurt(_perParticleHurtAmount));
        }
    }
}
