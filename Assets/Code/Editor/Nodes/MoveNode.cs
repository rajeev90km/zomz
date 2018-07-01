using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class MoveNode : BaseNode
{
	[NonSerialized]
	private const float TEXTBOX_HEIGHT = 25f;

	[NonSerialized]
	private const float MAX_WIDTH = 260f;
	[NonSerialized]
	private const float MIN_WIDTH = 60f;

	[NonSerialized]
	private const int PADDING = 20;

	public Rect sourceGameObjectRect;
	public Rect moveTimeRect;
	public Rect targetGameObjectRect;

	public ObjectMarker _sourceObject;

	[NonSerialized]
	private const float DEFAULT_MOVE_TIME = 3f;

	public float _moveTime = 0f;

	public ObjectMarker _targetObject;

	public MoveNode ()
	{
		Type = NodeType.MOVENODE;
	}

	public MoveNode (ObjectMarker pGO, float pMoveTime, ObjectMarker tGO, Vector2 position, float pNodeSize, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode)
		: base (position, pNodeSize, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode)
	{
		sourceGameObjectRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		targetGameObjectRect = new Rect (rect.x + PADDING, rect.y + TEXTBOX_HEIGHT + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		moveTimeRect = new Rect (rect.x + PADDING, rect.y + (TEXTBOX_HEIGHT * 2) + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);

		_sourceObject = pGO;
		_targetObject = tGO;
		_moveTime = pMoveTime;

		if (_moveTime <= 0)
			_moveTime = DEFAULT_MOVE_TIME;

		Type = NodeType.MOVENODE;
	}

	public MoveNode (ObjectMarker pGO, float pMoveTime, ObjectMarker tGO, Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint,
	                 Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode, string inPointID, string outPointID)
		: base (position, width, height, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, inPointID, outPointID)
	{
		sourceGameObjectRect = new Rect (rect.x + PADDING, rect.y + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		targetGameObjectRect = new Rect (rect.x + PADDING, rect.y + TEXTBOX_HEIGHT + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		moveTimeRect = new Rect (rect.x + PADDING, rect.y + (TEXTBOX_HEIGHT * 2) + PADDING, rect.width - (PADDING * 2), TEXTBOX_HEIGHT);
		_sourceObject = pGO;
		_targetObject = tGO;
		_moveTime = pMoveTime;

		if (_moveTime <= 0)
			_moveTime = DEFAULT_MOVE_TIME;

		Type = NodeType.MOVENODE;
	}


	public override void Draw ()
	{
		base.Draw ();

		GUILayout.BeginArea (sourceGameObjectRect);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Source", GUILayout.Width (sourceGameObjectRect.width / 4));
		_sourceObject = (ObjectMarker)EditorGUILayout.ObjectField (_sourceObject, typeof(ObjectMarker), false, GUILayout.Width (3 * sourceGameObjectRect.width / 4));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		GUILayout.BeginArea (moveTimeRect);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Move Time", GUILayout.Width (moveTimeRect.width / 4));
		_moveTime = EditorGUILayout.FloatField (_moveTime, GUILayout.Width (3 * moveTimeRect.width / 4));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		GUILayout.BeginArea (targetGameObjectRect);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Target", GUILayout.Width (targetGameObjectRect.width / 4));
		_targetObject = (ObjectMarker)EditorGUILayout.ObjectField (_targetObject, typeof(ObjectMarker), false, GUILayout.Width (3 * targetGameObjectRect.width / 4));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}

	public override void Drag (Vector2 delta)
	{
		base.Drag (delta);
		sourceGameObjectRect.position += delta;
		moveTimeRect.position += delta;
		targetGameObjectRect.position += delta;
	}

	public override void Resize (float scale)
	{
		base.Resize (scale);

		sourceGameObjectRect.width += scale;
		moveTimeRect.width += scale;
		targetGameObjectRect.width += scale;

		if (sourceGameObjectRect.width > MAX_WIDTH)
			sourceGameObjectRect.width = MAX_WIDTH;

		if (sourceGameObjectRect.width < MIN_WIDTH)
			sourceGameObjectRect.width = MIN_WIDTH;

		if (moveTimeRect.width > MAX_WIDTH)
			moveTimeRect.width = MAX_WIDTH;

		if (moveTimeRect.width < MIN_WIDTH)
			moveTimeRect.width = MIN_WIDTH;

		if (targetGameObjectRect.width > MAX_WIDTH)
			targetGameObjectRect.width = MAX_WIDTH;

		if (targetGameObjectRect.width < MIN_WIDTH)
			targetGameObjectRect.width = MIN_WIDTH;
	}

	public override IEnumerator ProcessNode (StoryProgressMonitor spm)
	{
		if (_moveTime <= 0)
			_moveTime = DEFAULT_MOVE_TIME;

		float time = 0.0f;

		if (_sourceObject.Marker != null && _targetObject.Marker != null)
		{
			Transform source = _sourceObject.Marker;
			Transform target = _targetObject.Marker;

			if (source != null && target != null)
			{
				Vector3 startingPos = source.position;

				while (time < _moveTime)
				{
					source.position = Vector3.Lerp (startingPos, target.position, time / _moveTime);

					//TODO: Change these to separate nodes
					source.LookAt (target);

					Animator _anim = source.gameObject.GetComponent<Animator> ();
					_anim.SetFloat ("Forward", 0.6f);

					time += Time.deltaTime;
					yield return null;
				}
				source.position = target.position;
			}
		}

		spm.isNodePlaying = false;
		yield return null;
	}

}


