using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdEntityController : MonoBehaviour {

    [SerializeField]
    private List<string> _animations;

    private Animator _animator;

	void Start () 
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(StartAnimationLoop());	
	}

    IEnumerator StartAnimationLoop()
    {
        while (true)
        {
            int i = Random.Range(0, _animations.Count - 1);
            _animator.SetTrigger(_animations[i]);
            yield return new WaitForSeconds(Random.Range(4, 10));
        }
    }
	
	void Update () 
    {
		
	}
}
