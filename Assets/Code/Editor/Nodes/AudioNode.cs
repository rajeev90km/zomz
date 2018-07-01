using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class AudioNode : BaseNode
{
	[NonSerialized]
	private const float TEXTBOX_HEIGHT = 25f;

	[NonSerialized]
	private const float MAX_WIDTH = 260f;
	[NonSerialized]
	private const float MIN_WIDTH = 60f;

	[NonSerialized]
	private const int PADDING = 20;

	public Rect audioRect;
	public Rect audioManagerRect;

	public ObjectMarker _audioManager;

	public AudioClip _sourceClip;

	public AudioNode ()
	{
		Type = NodeType.AUDIONODE;
	}

	public AudioNode (ObjectMarker pAudioManager, AudioClip pSourceAudio, Vector2 position, float pNodeSize, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode)
		: base (position, pNodeSize, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode)
	{

		audioManagerRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		audioRect = new Rect (rect.x + PADDING, rect.y + PADDING + TEXTBOX_HEIGHT, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);

		_sourceClip = pSourceAudio;
		_audioManager = pAudioManager;
		Type = NodeType.AUDIONODE;
	}

	public AudioNode (ObjectMarker pAudioManager, AudioClip pSourceAudio, Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint,
	                  Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode, string inPointID, string outPointID)
		: base (position, width, height, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, inPointID, outPointID)
	{
		audioManagerRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		audioRect = new Rect (rect.x + PADDING, rect.y + PADDING + TEXTBOX_HEIGHT, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);

		_sourceClip = pSourceAudio;
		_audioManager = pAudioManager;
		Type = NodeType.AUDIONODE;
	}


	public override void Draw ()
	{
		base.Draw ();

		GUILayout.BeginArea (audioManagerRect);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Manager", GUILayout.Width (audioManagerRect.width / 4));
		_audioManager = (ObjectMarker)EditorGUILayout.ObjectField (_audioManager, typeof(ObjectMarker), false, GUILayout.Width (3 * audioManagerRect.width / 4));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		GUILayout.BeginArea (audioRect);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Clip", GUILayout.Width (audioRect.width / 4));
		_sourceClip = (AudioClip)EditorGUILayout.ObjectField (_sourceClip, typeof(AudioClip), false, GUILayout.Width (3 * audioRect.width / 4));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}

	public override void Drag (Vector2 delta)
	{
		base.Drag (delta);
		audioRect.position += delta;
		audioManagerRect.position += delta;
	}

	public override void Resize (float scale)
	{
		base.Resize (scale);

		audioRect.width += scale;
		audioManagerRect.width += scale;

		if (audioRect.width > MAX_WIDTH)
		{
			audioRect.width = MAX_WIDTH;
			audioManagerRect.width = MAX_WIDTH;
		}

		if (audioRect.width < MIN_WIDTH)
		{
			audioRect.width = MIN_WIDTH;
			audioManagerRect.width = MIN_WIDTH;
		}
	}

	public override IEnumerator ProcessNode (StoryProgressMonitor spm)
	{
		if (_sourceClip != null && _audioManager != null)
		{
			GameObject _am = _audioManager.Marker.gameObject;

			if (_am != null)
			{
				AudioSource _as = _am.GetComponent<AudioSource> ();

				if (_as != null)
				{
					_as.clip = _sourceClip;
					_as.Play ();
					yield return new WaitForSeconds (_sourceClip.length);
					_as.clip = null;
				}
			}

		}
		spm.isNodePlaying = false;
		yield return null;
	}

}


