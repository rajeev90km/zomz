using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryPlayer : MonoBehaviour
{
	[SerializeField]
	private Story _story;

	[SerializeField]
	private StoryProgressMonitor _storyProgressMonitor;

	private string _storyData;
	private StoryObj StoryData;
	private List<string> storyNodes = new List<string>();

	IEnumerator Start ()
	{
		_storyData = _story.StoryJSON;
		StoryData = JsonUtility.FromJson<StoryObj> (_storyData);
		storyNodes = StoryData.nodes;
		_storyProgressMonitor.isNodePlaying = false;

		yield return new WaitForEndOfFrame();

		StartCoroutine (StoryExecute ());
	}

	IEnumerator StoryExecute()
	{
		foreach (string node in storyNodes)
		{
			BaseNode bn = JsonUtility.FromJson<BaseNode> (node);

			switch (bn.Type)
			{
			case NodeType.MOVENODE:
				_storyProgressMonitor.isNodePlaying = true;
				MoveNode mn = (MoveNode)JsonUtility.FromJson<MoveNode> (node);
				StartCoroutine (mn.ProcessNode (_storyProgressMonitor));
				break;
				
			case NodeType.AUDIONODE:
				_storyProgressMonitor.isNodePlaying = true;
				AudioNode an = (AudioNode)JsonUtility.FromJson<AudioNode> (node);
				StartCoroutine (an.ProcessNode (_storyProgressMonitor));
				break;
			}

			yield return new WaitUntil (() => _storyProgressMonitor.isNodePlaying == false);
		}

		yield return null;
	}

}
