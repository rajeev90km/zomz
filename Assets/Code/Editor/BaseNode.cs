using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class BaseNode
{

	public Rect rect;

	[NonSerialized]
	public string title;

	[NonSerialized]
	public bool isDragged;

	[NonSerialized]
	public bool isSelected;

	private const float INIT_SCALE = 1.0f;

	private const float MAX_SIZE = 300f;
	private const float MIN_SIZE = 100f;

	[NonSerialized]
	public GUIStyle style;

	[NonSerialized]
	public GUIStyle defaultNodeStyle;

	[NonSerialized]
	public GUIStyle selectedNodeStyle;

	public ConnectionPoint inPoint;

	public ConnectionPoint outPoint;

	public NodeType Type;

	[NonSerialized]
	public Action<BaseNode> OnRemoveNode;

	public BaseNode ()
	{
		
	}

	public BaseNode (Vector2 position, float pNodeSize, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode)
	{
		rect = new Rect (position.x, position.y, pNodeSize, pNodeSize);
		style = nodeStyle;
		inPoint = new ConnectionPoint (this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
		outPoint = new ConnectionPoint (this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
		defaultNodeStyle = nodeStyle;
		selectedNodeStyle = selectedStyle;
		OnRemoveNode = OnClickRemoveNode;
	}

	public BaseNode (Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint,
	                Action<ConnectionPoint> OnClickOutPoint, Action<BaseNode> OnClickRemoveNode, string inPointID, string outPointID)
	{
		rect = new Rect (position.x, position.y, width, height);
		style = nodeStyle;
		inPoint = new ConnectionPoint (this, ConnectionPointType.In, inPointStyle, OnClickInPoint, inPointID);
		outPoint = new ConnectionPoint (this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint, outPointID);
		defaultNodeStyle = nodeStyle;
		selectedNodeStyle = selectedStyle;
		OnRemoveNode = OnClickRemoveNode;
	}

	public virtual void Drag (Vector2 delta)
	{
		rect.position += delta;
	}

	public virtual void Resize (float scale)
	{
		if ((rect.width >= MIN_SIZE && rect.width <= MAX_SIZE) && (rect.height >= MIN_SIZE && rect.height <= MAX_SIZE))
		{
			rect.width += scale;
			rect.height += scale;
		} 

		if (rect.width > MAX_SIZE)
		{
			rect.width = MAX_SIZE;
			rect.height = MAX_SIZE;
		}

		if (rect.width < MIN_SIZE)
		{
			rect.width = MIN_SIZE;
			rect.height = MIN_SIZE;
		}
	}

	public virtual void Draw ()
	{
		if (inPoint != null)
			inPoint.Draw ();
		if (outPoint != null)
			outPoint.Draw ();
		GUI.Box (rect, title, style);
	}

	public bool ProcessEvents (Event e)
	{
		switch (e.type)
		{
		case EventType.MouseDown:
			if (e.button == 0)
			{
				if (rect.Contains (e.mousePosition))
				{
					isDragged = true;
					GUI.changed = true;
					isSelected = true;
					style = selectedNodeStyle;
				} else
				{
					isSelected = false;
					style = defaultNodeStyle;
					GUI.changed = true;
				}
			} 
			if (e.button == 1 && isSelected && rect.Contains (e.mousePosition))
			{
				ProcessContextMenu ();
				e.Use ();
			}
				
			break;

		case EventType.MouseUp:
			isDragged = false;
			break;

		case EventType.MouseDrag:
			if (e.button == 0 && isDragged)
			{
				Drag (e.delta);
				e.Use ();
				return true;
			}
			break;
		}

		return false;
	}

	private void ProcessContextMenu ()
	{
		GenericMenu genericMenu = new GenericMenu ();
		genericMenu.AddItem (new GUIContent ("Remove Node"), false, () => OnClickRemove ()); 
		genericMenu.ShowAsContext ();
	}

	private void OnClickRemove ()
	{
		if (OnRemoveNode != null)
			OnRemoveNode (this);
	}

	public virtual IEnumerator ProcessNode (StoryProgressMonitor spm)
	{
		yield return null;
	}
}
