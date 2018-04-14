using System;
using UnityEngine;

public enum ConnectionPointType
{
	In,
	Out

}

[Serializable]
public class ConnectionPoint
{
	public string id;

	[NonSerialized]
	public Rect rect;

	[NonSerialized]
	public ConnectionPointType type;

	[NonSerialized]
	public BaseNode node;

	private const int CP_HEIGHT = 20;
	private const int CP_WIDTH = 10;

	[NonSerialized]
	public GUIStyle style;

	[NonSerialized]
	public Action<ConnectionPoint> OnClickConnectionPoint;

	public ConnectionPoint ()
	{
	}

	public ConnectionPoint (BaseNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint, string id = null)
	{
		this.node = node;
		this.type = type;
		this.style = style;
		this.OnClickConnectionPoint = OnClickConnectionPoint;
		this.id = id ?? Guid.NewGuid ().ToString ();
		rect = new Rect (0, 0, CP_WIDTH, CP_HEIGHT);
	}

	public void Draw ()
	{
		rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

		switch (type)
		{
		case ConnectionPointType.In:
			rect.x = node.rect.x - rect.width + 8f;
			break;

		case ConnectionPointType.Out:
			rect.x = node.rect.x + node.rect.width - 8f;
			break;
		}

		if (GUI.Button (rect, "", style))
		{
			if (OnClickConnectionPoint != null)
			{
				OnClickConnectionPoint (this);
			}
		}
	}
}