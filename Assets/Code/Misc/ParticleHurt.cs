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

        if(gameObject.CompareTag("Player"))
        {
            CharacterControls player = GetComponent<CharacterControls>();

            if (player && player.IsAlive)
                player.StartCoroutine(player.Hurt(_perParticleHurtAmount));
        }
        else if(gameObject.CompareTag("Enemy"))
        {
            ZombieBase zombieBase = GetComponent<ZombieBase>();

            if (zombieBase && zombieBase.IsAlive && !zombieBase.IsHurting)
                zombieBase.StartCoroutine(zombieBase.Hurt(_perParticleHurtAmount));
        }
    }
}
