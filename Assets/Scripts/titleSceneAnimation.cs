using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class titleSceneAnimation : MonoBehaviour {

	Animator titleLogoAnim;
	Animator mainMenusAnim;

	public bool isTitleSceneLoaded = false;
	public bool continueRequested = false;
	public bool optionsRequested = false;

	void Start () {
		titleLogoAnim = GameObject.Find("TitleLogo").GetComponent<Animator>();
		mainMenusAnim = GameObject.Find("MainMenus").GetComponent<Animator>();
	}

	void Update () {
		// Triggering title logo and main menus animation and transition
		if (isTitleSceneLoaded) {
			titleLogoAnim.SetTrigger ("titleSceneLoaded");
			mainMenusAnim.SetTrigger ("titleSceneLoaded");
		}

		if (isTitleSceneLoaded && continueRequested) {
			titleLogoAnim.SetBool ("isContinueRequested", true);
			mainMenusAnim.SetBool ("isContinueRequested", true);
		} else if (isTitleSceneLoaded && !continueRequested) {
			titleLogoAnim.SetBool ("isContinueRequested", false);
			mainMenusAnim.SetBool ("isContinueRequested", false);
		}

		if (isTitleSceneLoaded && optionsRequested) {
			titleLogoAnim.SetBool ("isOptionsRequested", true);
			mainMenusAnim.SetBool ("isOptionsRequested", true);
		} else if (isTitleSceneLoaded && !optionsRequested) {
			titleLogoAnim.SetBool ("isOptionsRequested", false);
			mainMenusAnim.SetBool ("isOptionsRequested", false);
		}
	}
}
