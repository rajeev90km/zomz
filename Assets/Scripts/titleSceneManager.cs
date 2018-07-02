using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titleSceneManager : MonoBehaviour {

	titleSceneAnimation titleSceneAnim;

	void Start () {
		titleSceneAnim = GetComponent<titleSceneAnimation>();
	}

	void Update () {
		
	}

	// Title scene level manager methods
	public void LoadLevel (int index)	{
        Debug.Log("Load Level");
        SceneManager.LoadScene(index);
	}

	public void OpenLevelSelector () {
		titleSceneAnim.continueRequested = true;
	}

	public void CloseLevelSelector () {
		titleSceneAnim.continueRequested = false;
	}

	public void OpenOptions () {
		titleSceneAnim.optionsRequested = true;
	}

	public void CloseOptions () {
		titleSceneAnim.optionsRequested = false;
	}

	public void QuitRequest () {
		Application.Quit();
	}

}
