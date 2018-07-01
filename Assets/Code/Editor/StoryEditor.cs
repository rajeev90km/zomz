using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Serializable]
public class StoryObj
{
	[SerializeField]
	public List<string> nodes;

	[SerializeField]
	public List<Connection> connections;

	public StoryObj (List<string> pNodes, List<Connection> pConnections)
	{
		nodes = pNodes;
		connections = pConnections;
	}
}

public enum NodeType
{
	TEXTNODE = 0,
	MOVENODE = 1,
	AUDIONODE = 2
}

public class StoryEditor : EditorWindow
{
	public List<BaseNode> nodes;

	public List<Connection> connections;

	private const string PLACEHOLDER_STORY_NAME = "";
	private string storyName = PLACEHOLDER_STORY_NAME;

	private const string STORY_PATH = "Assets/Resources/Data/Stories/";

	private GUIStyle nodeStyle;
	private GUIStyle selectedNodeStyle;
	private GUIStyle inPointStyle;
	private GUIStyle outPointStyle;

	private ConnectionPoint selectedInPoint = null;
	private ConnectionPoint selectedOutPoint = null;

	private const int NODE_SIZE = 200;
	private const int NODE_BORDER = 12;
	private const float INIT_SCALE = 1.0f;
	private float MENU_BAR_HEIGHT = 20f;
	private Rect menuBar;


	private Vector2 offset;
	private Vector2 drag;

	private float currentNodeSize = NODE_SIZE;
	private float currentScale = INIT_SCALE;

	private Story _story;


	[MenuItem ("Zomz/Story Canvas")]
	private static void OpenWindow ()
	{
		StoryEditor window = GetWindow<StoryEditor> ();
		window.titleContent = new GUIContent ("Story Canvas");
	}

	private void OnEnable ()
	{
		nodeStyle = new GUIStyle ();
		nodeStyle.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/node1.png") as Texture2D;
		nodeStyle.border = new RectOffset (NODE_BORDER, NODE_BORDER, NODE_BORDER, NODE_BORDER);

		selectedNodeStyle = new GUIStyle ();
		selectedNodeStyle.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/node1 on.png") as Texture2D;
		selectedNodeStyle.border = new RectOffset (NODE_BORDER, NODE_BORDER, NODE_BORDER, NODE_BORDER);

		inPointStyle = new GUIStyle ();
		inPointStyle.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/btn left.png") as Texture2D;
		inPointStyle.active.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/btn left on.png") as Texture2D;
		inPointStyle.border = new RectOffset (4, 4, 12, 12);

		outPointStyle = new GUIStyle ();
		outPointStyle.normal.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/btn right.png") as Texture2D;
		outPointStyle.active.background = EditorGUIUtility.Load ("builtin skins/darkskin/images/btn right on.png") as Texture2D;
		outPointStyle.border = new RectOffset (4, 4, 12, 12);
	}

	private void OnGUI ()
	{
		DrawGrid (20, 0.2f, Color.gray);
		DrawGrid (100, 0.4f, Color.gray);
		DrawMenuBar ();

		DrawNodes ();
		DrawConnections ();

		DrawConnectionLine (Event.current);

		ProcessNodeEvents (Event.current);
		ProcessEvents (Event.current);

		if (GUI.changed)
			Repaint ();
	}

	private void DrawMenuBar ()
	{ 
		menuBar = new Rect (0, 0, position.width, MENU_BAR_HEIGHT);

		GUILayout.BeginArea (menuBar, EditorStyles.toolbar);
		GUILayout.BeginHorizontal ();

		GUILayout.FlexibleSpace ();
		storyName = GUILayout.TextField (storyName, GUI.skin.FindStyle ("ToolbarSeachTextField"), GUILayout.Width (200));
		if (GUILayout.Button (new GUIContent ("Load"), EditorStyles.toolbarButton, GUILayout.Width (35)))
		{
			Load ();
		}
		if (GUILayout.Button (new GUIContent ("Save"), EditorStyles.toolbarButton, GUILayout.Width (35)))
		{
			Save ();
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}

	private void DrawConnections ()
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Count; i++)
			{
				connections [i].Draw ();
			} 
		}
	}

	private void DrawNodes ()
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes [i].Draw ();
			}
		}
	}

	private void DrawConnectionLine (Event e)
	{
		if (selectedInPoint != null && selectedOutPoint == null)
		{
			Handles.DrawBezier (
				selectedInPoint.rect.center,
				e.mousePosition,
				selectedInPoint.rect.center + Vector2.left * 50f,
				e.mousePosition - Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}

		if (selectedOutPoint != null && selectedInPoint == null)
		{
			Handles.DrawBezier (
				selectedOutPoint.rect.center,
				e.mousePosition,
				selectedOutPoint.rect.center - Vector2.left * 50f,
				e.mousePosition + Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}
	}

	private void DrawGrid (float gridSpacing, float gridOpacity, Color gridColor)
	{
		int widthDivs = Mathf.CeilToInt (position.width / gridSpacing);
		int heightDivs = Mathf.CeilToInt (position.height / gridSpacing);

		Handles.BeginGUI ();
		Handles.color = new Color (gridColor.r, gridColor.g, gridColor.b, gridOpacity);

		offset += drag * 0.5f;
		Vector3 newOffset = new Vector3 (offset.x % gridSpacing, offset.y % gridSpacing, 0);

		for (int i = 0; i < widthDivs; i++)
		{
			Handles.DrawLine (new Vector3 (gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3 (gridSpacing * i, position.height, 0f) + newOffset);
		}

		for (int j = 0; j < heightDivs; j++)
		{
			Handles.DrawLine (new Vector3 (-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3 (position.width, gridSpacing * j, 0f) + newOffset);
		}

		Handles.color = Color.white;
		Handles.EndGUI ();
	}

	private void ProcessNodeEvents (Event e)
	{
		if (nodes != null)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				bool guiChanged = nodes [i].ProcessEvents (e);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}
		}
	}

	private void ProcessEvents (Event e)
	{
		drag = Vector2.zero;

		switch (e.type)
		{
		case EventType.MouseDown:
			if (e.button == 1)
			{
				ProcessContextMenu (e.mousePosition);
			}
			break;

		case EventType.MouseDrag:
			if (e.button == 2)
			{
				OnDrag (e.delta);
			}
			break;
		case EventType.ScrollWheel:
			currentScale = e.delta.y + INIT_SCALE;
			OnResize (currentScale);
			e.Use ();
			break;
		}
	}

	private void OnResize (float delta)
	{
		if (currentNodeSize >= 100 && currentNodeSize <= 500)
			currentNodeSize += delta;

		if (currentNodeSize > 500)
			currentNodeSize = 500;

		if (currentNodeSize < 100)
			currentNodeSize = 100;

		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes [i].Resize (delta);
			}
		}

		GUI.changed = true;	
	}

	private void OnDrag (Vector2 delta)
	{
		drag = delta;

		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes [i].Drag (delta);
			}
		}

		GUI.changed = true;
	}

	private void ProcessContextMenu (Vector2 pMousePosition)
	{
		GenericMenu genericMenu = new GenericMenu ();
		//genericMenu.AddItem (new GUIContent ("Add node"), false, () => OnClickAddNode (pMousePosition)); 
		genericMenu.AddItem (new GUIContent ("Add Text node"), false, () => OnClickAddNode (NodeType.TEXTNODE, pMousePosition)); 
		genericMenu.AddItem (new GUIContent ("Add Move node"), false, () => OnClickAddNode (NodeType.MOVENODE, pMousePosition)); 
		genericMenu.AddItem (new GUIContent ("Add Audio node"), false, () => OnClickAddNode (NodeType.AUDIONODE, pMousePosition)); 
		genericMenu.ShowAsContext ();
	}


	private void Save ()
	{
		if (storyName != PLACEHOLDER_STORY_NAME)
		{

			string filePath = STORY_PATH + storyName + ".asset";
			_story = (Story)AssetDatabase.LoadAssetAtPath(filePath, typeof(Story));

			List<string> nodeData = new List<string> ();
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes [i].Type == NodeType.TEXTNODE)
				{
					nodeData.Add (JsonUtility.ToJson ((TextNode)nodes [i]));
				} 
				else if (nodes [i].Type == NodeType.MOVENODE)
				{
					nodeData.Add (JsonUtility.ToJson ((MoveNode)nodes [i]));
				}
				else if (nodes [i].Type == NodeType.AUDIONODE)
				{
					nodeData.Add (JsonUtility.ToJson ((AudioNode)nodes [i]));
				}
			}

			StoryObj so = new StoryObj (nodeData, connections);

			if (_story == null)
			{
				_story = ScriptableObjectUtility.CreateAsset<Story> (STORY_PATH,storyName);
			}
			_story.StoryJSON = JsonUtility.ToJson (so);
			EditorUtility.SetDirty (_story);

			#if UNITY_EDITOR
			UnityEditor.AssetDatabase.SaveAssets ();
			UnityEditor.AssetDatabase.Refresh ();
			#endif
		}
		else
		{
			ShowNotification(new GUIContent("Enter story name to Save."));
		}

	}

	private void Load ()
	{
		if (storyName != PLACEHOLDER_STORY_NAME)
		{
			string filePath = STORY_PATH + storyName + ".asset";
			Story s = (Story)AssetDatabase.LoadAssetAtPath(filePath, typeof(Story));

			if (s!=null)
			{
				string dataAsJson = s.StoryJSON;
				StoryObj importedObj = JsonUtility.FromJson<StoryObj> (dataAsJson);

				List<string> importedNodes = importedObj.nodes;
				List<Connection> importedConnections = importedObj.connections;

				nodes = new List<BaseNode> ();
				connections = new List<Connection> ();

				foreach (string node in importedNodes)
				{
					if (JsonUtility.FromJson<BaseNode> (node).Type == NodeType.TEXTNODE)
					{
						TextNode n = JsonUtility.FromJson<TextNode> (node);
						nodes.Add(new TextNode(n._text,n.rect.position,n.rect.width,n.rect.width,nodeStyle,selectedNodeStyle,inPointStyle,outPointStyle,OnClickInPoint,OnClickOutPoint,OnClickRemoveNode,n.inPoint.id,n.outPoint.id));
						currentNodeSize = n.rect.width;
					}
					else if(JsonUtility.FromJson<BaseNode> (node).Type == NodeType.MOVENODE)
					{
						MoveNode n = JsonUtility.FromJson<MoveNode> (node);
						nodes.Add(new MoveNode(n._sourceObject,n._moveTime,n._targetObject,n.rect.position,n.rect.width,n.rect.width,nodeStyle,selectedNodeStyle,inPointStyle,outPointStyle,OnClickInPoint,OnClickOutPoint,OnClickRemoveNode,n.inPoint.id,n.outPoint.id));
						currentNodeSize = n.rect.width;
					}
					else if(JsonUtility.FromJson<BaseNode> (node).Type == NodeType.AUDIONODE)
					{
						AudioNode n = JsonUtility.FromJson<AudioNode> (node);
						nodes.Add(new AudioNode(n._audioManager,n._sourceClip,n.rect.position,n.rect.width,n.rect.width,nodeStyle,selectedNodeStyle,inPointStyle,outPointStyle,OnClickInPoint,OnClickOutPoint,OnClickRemoveNode,n.inPoint.id,n.outPoint.id));
						currentNodeSize = n.rect.width;
					}
				}

				foreach (Connection c in importedConnections)
				{
					ConnectionPoint inPoint = nodes.First (n => n.inPoint.id == c.inPoint.id).inPoint;
					ConnectionPoint outPoint = nodes.First (n => n.outPoint.id == c.outPoint.id).outPoint;
					connections.Add (new Connection (inPoint, outPoint, OnClickRemoveConnection));
				}

			}
			else
			{
				ShowNotification(new GUIContent("Story Not Found."));
			}
		}
		else
		{
			ShowNotification(new GUIContent("No Story Name entered."));
		}
	}

	private void OnClickAddNode (NodeType pNodeType, Vector2 pMousePosition)
	{
		if (nodes == null)
		{
			nodes = new List<BaseNode> ();
		}

		switch (pNodeType)
		{
		case NodeType.TEXTNODE:
			nodes.Add (new TextNode ("Enter Text", pMousePosition, currentNodeSize, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
			break;
		case NodeType.MOVENODE:
			nodes.Add (new MoveNode (null, 0f, null, pMousePosition, currentNodeSize, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
			break;
		case NodeType.AUDIONODE:
			nodes.Add (new AudioNode (null,null, pMousePosition, currentNodeSize, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
			break;
		}

		//nodes.Add (new BaseNode (pMousePosition, currentNodeSize, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
	}

	private void OnClickInPoint (ConnectionPoint inPoint)
	{
		selectedInPoint = inPoint;

		if (selectedOutPoint != null)
		{
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection ();
				ClearConnectionSelection (); 
			} else
			{
				ClearConnectionSelection ();
			}
		}
	}

	private void OnClickOutPoint (ConnectionPoint outPoint)
	{
		selectedOutPoint = outPoint;

		if (selectedInPoint != null)
		{
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection ();
				ClearConnectionSelection ();
			} else
			{
				ClearConnectionSelection ();
			}
		}
	}

	private void OnClickRemoveConnection (Connection connection)
	{
		connections.Remove (connection);
	}

	private void CreateConnection ()
	{
		
		if (connections == null)
		{
			connections = new List<Connection> ();
		}

		connections.Add (new Connection (selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
	}

	private void ClearConnectionSelection ()
	{
		selectedInPoint = null;
		selectedOutPoint = null;
	}

	private void OnClickRemoveNode (BaseNode node)
	{
		if (connections != null)
		{
			List<Connection> connectionsToRemove = new List<Connection> ();

			for (int i = 0; i < connections.Count; i++)
			{
				if (connections [i].inPoint == node.inPoint || connections [i].outPoint == node.outPoint)
				{
					connectionsToRemove.Add (connections [i]);
				}
			}

			for (int i = 0; i < connectionsToRemove.Count; i++)
			{
				connections.Remove (connectionsToRemove [i]);
			}

			connectionsToRemove = null;
		}

		nodes.Remove (node);
	}

}