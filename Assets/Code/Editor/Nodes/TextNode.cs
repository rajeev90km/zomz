using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class TextNode : BaseNode
{
	[NonSerialized]
	private const float TEXTBOX_HEIGHT = 25f;

	[NonSerialized]
	private const float MAX_WIDTH = 260f;
	[NonSerialized]
	private const float MIN_WIDTH = 60f;

	[NonSerialized]
	private const int PADDING = 20;

	public Rect textFieldRect;

	public string _text;

	public TextNode ()
	{
		
	}

	public TextNode (string text, Vector2 position, float pNodeSize, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode)
		: base (position, pNodeSize, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode)
	{
		textFieldRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		_text = text;

		Type = NodeType.TEXTNODE;
	}

	public TextNode (string text,Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint,
	                 Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode, string inPointID, string outPointID)
		: base (position, width, height, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, inPointID, outPointID)
	{
		textFieldRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		_text = text;
		Type = NodeType.TEXTNODE;
	}


	public override void Draw ()
	{
		base.Draw ();

		_text = GUI.TextField (textFieldRect, _text);
	}

	public override void Drag (Vector2 delta)
	{
		base.Drag (delta);
		textFieldRect.position += delta;
	}

	public override void Resize (float scale)
	{
		base.Resize (scale);

		textFieldRect.width += scale;

		if (textFieldRect.width > MAX_WIDTH)
			textFieldRect.width = MAX_WIDTH;

		if (textFieldRect.width < MIN_WIDTH)
			textFieldRect.width = MIN_WIDTH;

	}

	public override IEnumerator ProcessNode(StoryProgressMonitor spm)
	{
		Debug.Log ("Text Node Stuff");
		yield return null;
	}
}


