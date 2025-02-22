﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SpectralDaze.DialogueSystem
{

    public class DialogueSave
    {
        public List<Node> Nodes = new List<Node>();
        public List<Connection> Connections = new List<Connection>();
        public List<Message> Messages = new List<Message>();
    }
#if UNITY_EDITOR
    /// <summary>
    /// The custom dialogue editor
    /// </summary>
    /// <seealso cref="UnityEditor.EditorWindow" />
    public class DialogueEditor : EditorWindow
    {
        /// <summary>
        /// A class to be serialized holding all the information that needs to be saved by editor.
        /// </summary>

        /// <summary>
        /// The node count that is upped by one every time one is created.
        /// </summary>
        private int NodeCount = 0;

        /// <summary>
        /// A list of nodes in the editor window currently.
        /// </summary>
        public List<Node> Nodes = new List<Node>();
        /// <summary>
        /// A list of connections in the editor window currently.
        /// </summary>
        public List<Connection> Connections = new List<Connection>();
        /// <summary>
        /// The input connection point that is selected.
        /// </summary>
        public ConnectionPoint SelectedInPoint;
        /// <summary>
        /// The selected output point that is selected.
        /// </summary>
        public ConnectionPoint SelectedOutputPoint;

        /// <summary>
        /// The drag delta
        /// </summary>
        private Vector2 _drag;
        /// <summary>
        /// The offset
        /// </summary>
        private Vector2 _offset;

        /// <summary>
        /// The menu bar height
        /// </summary>
        private float menuBarHeight = 20f;
        /// <summary>
        /// The menu bar rect
        /// </summary>
        private Rect menuBar;

        /// <summary>
        /// Is the editor loading a dialogue map.
        /// </summary>
        private bool _loading;

        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Window/Dialogue Editor")]
        private static void OpenWindow()
        {
            DialogueEditor window = GetWindow<DialogueEditor>();
            window.titleContent = new GUIContent("Dialogue Editor");
        }
            
        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            if (!_loading)
            {
                BeginWindows();
                foreach (var node in Nodes)
                {
                    node.Draw();
                    node.ProcessEvents(Event.current);
                }
                EndWindows();

                foreach (var connection in Connections)
                {
                    connection.Draw();
                }

                DrawConnectionLine(Event.current);
            }

            DrawMenuBar();

            ProcessEvents(Event.current);
            if (GUI.changed) Repaint();
        }

        /// <summary>
        /// Processes the events.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void ProcessEvents(Event e)
        {
            _drag = Vector2.zero;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        SelectedOutputPoint = null;
                        SelectedInPoint = null;
                    }
                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                        e.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete)
                    {
                        RemoveNode(Nodes.SingleOrDefault(x => x.Selected));
                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates menu, generates menu, and opens menu.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        public void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false, () =>
            {
                NodeCount++;
                Nodes.Add(new Node(NodeCount, mousePosition.x, mousePosition.y, 200, 100, OnNodeConnectorClicked, RemoveNode, SetStartingNode, SetEndingNode));
                Repaint();
            });
            genericMenu.ShowAsContext();
        }

        /// <summary>
        /// Called when background is dragged.
        /// </summary>
        /// <param name="delta">The delta.</param>
        private void OnDrag(Vector2 delta)
        {
            _drag = delta;
            foreach (var node in Nodes)
            {
                node.Drag(delta);
            }
            Repaint();
        }

        /// <summary>
        /// Draws the menu bar.
        /// </summary>
        private void DrawMenuBar()
        {
            menuBar = new Rect(0, 0, position.width, menuBarHeight);

            GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(35)))
            {
                Save();
            }
            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(35)))
            {
                Load();
            }
            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(45)))
            {
                Connections.Clear();
                Nodes.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Saves the editors current map.
        /// </summary>
        private void Save()
        {
            var save = new DialogueSave()
            {
                Connections = Connections,
                Nodes = Nodes
            };
           var path = EditorUtility.SaveFilePanel(
                "Save Dialogue",
                Application.dataPath + "/Resources/",
                "Diagloue.json",
                "json");
            if (path == "")
                return;
            List<Message> tmpMessages = new List<Message>();

            var firstNode = save.Nodes.SingleOrDefault(x => x.First);
            List<Option> firstOptions = new List<Option>();
            for (int i = 0; i < firstNode.Options.Count; i++)
            {
                firstOptions.Add(new Option()
                {
                    Content = firstNode.Options[i],
                    RedirectionMessageID = firstNode.Outputs[i].AttachedNode.Id
                });
            }
            tmpMessages.Add(new Message()
            {
                CharacterPath = firstNode.CharacterPath,
                Content = firstNode.Message,
                Id = firstNode.Id,
                Options = firstOptions,
                Last = firstNode.Last,
                First = firstNode.First
            });

            var lastNodes = Nodes.Where(x => x.Last).ToList();

            foreach (var node in save.Nodes)
            {
                if(node==firstNode)
                    continue;
                if (lastNodes.Contains(node))
                    continue;

                List<Option> options = new List<Option>();
                for(int i=0; i< node.Options.Count; i++)
                {
                    options.Add(new Option()
                    {
                        Content = node.Options[i],
                        RedirectionMessageID = node.Outputs[i].AttachedNode.Id
                    });
                }
                tmpMessages.Add(new Message()
                {
                    CharacterPath = node.CharacterPath,
                    Content = node.Message,
                    Id = node.Id,
                    Options = options,
                    Last = node.Last,
                    First = node.First
                });
            }

            foreach (var node in lastNodes)
            {

                List<Option> lastOption = new List<Option>();
                for (int i = 0; i < node.Options.Count; i++)
                {
                    lastOption.Add(new Option()
                    {
                        Content = node.Options[i],
                        RedirectionMessageID = node.Outputs[i].AttachedNode.Id
                    });
                }
                tmpMessages.Add(new Message()
                {
                    CharacterPath = node.CharacterPath,
                    Content = node.Message,
                    Id = node.Id,
                    Options = lastOption,
                    Last = node.Last,
                    First = node.First
                });
            }

            save.Messages = tmpMessages;
            var json = JsonConvert.SerializeObject(save);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads a dialogue map into the editor.
        /// </summary>
        private void Load()
        {
            _loading = true;
            Nodes.Clear();
            Connections.Clear();
            var path = EditorUtility.OpenFilePanel("Open Dialogue", "/Resources/", "json");
            if (path == "")
                return; 
            string json = File.ReadAllText(path);
            DialogueSave save = JsonConvert.DeserializeObject<DialogueSave>(json);
            foreach (var node in save.Nodes)
            {
                var tmpNode = new Node(
                    node.Id,
                    node.Rect.position.x,
                    node.Rect.position.y,
                    node.Rect.width,
                    node.Rect.height,
                    OnNodeConnectorClicked,
                    RemoveNode,
                    SetStartingNode,
                    SetEndingNode
                );
                tmpNode.First = node.First;
                tmpNode.Last = node.Last;
                tmpNode.Message = node.Message;
                tmpNode.Character = Resources.Load<CharacterInformation>(node.CharacterPath.Replace(".asset","").Replace("Assets/Resources/", ""));
                tmpNode.Options = node.Options;
                tmpNode.Outputs = node.Outputs;
                tmpNode.Input = node.Input;
                tmpNode.Input.OwnerNode = tmpNode;
                tmpNode.Input.OnClickConnectionPoint = OnNodeConnectorClicked;
                foreach (var output in tmpNode.Outputs)
                {
                    output.OwnerNode = tmpNode;
                    output.OnClickConnectionPoint = OnNodeConnectorClicked;
                }
                Nodes.Add(tmpNode);
            }

            foreach (var connection in save.Connections)
            {
                var inPoint = Nodes.First(n => n.Input.Id == connection.InputPoint.Id).Input;
                foreach (var node in Nodes)
                {
                    foreach (var output in node.Outputs)
                    {
                        if (output.Id != connection.OutputPoint.Id) continue;
                        Connections.Add(new Connection(inPoint, output, OnClickRemoveConnection));
                        break;
                    }
                }
            }

            NodeCount = Nodes.Last().Id;        
            _loading = false;
        }

        /// <summary>
        /// Draws the connection line if only one point is seleceted..
        /// </summary>
        /// <param name="e">The event data.</param>
        private void DrawConnectionLine(Event e)
        {
            if (SelectedInPoint != null && SelectedOutputPoint == null)
            {
                Handles.DrawBezier(
                    SelectedInPoint.Rect.center,
                    e.mousePosition,
                    SelectedInPoint.Rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (SelectedOutputPoint != null && SelectedInPoint == null)
            {
                Handles.DrawBezier(
                    SelectedOutputPoint.Rect.center,
                    e.mousePosition,
                    SelectedOutputPoint.Rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Draws a grid.
        /// </summary>
        /// <param name="gridSpacing">The grid spacing.</param>
        /// <param name="gridOpacity">The grid opacity.</param>
        /// <param name="gridColor">Color of the grid.</param>
        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            _offset += _drag * 0.5f;
            Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Removes the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        private void RemoveNode(Node node)
        {
            Nodes.Remove(node);
            Connection connectionToRemove = null;
            foreach (var connection in Connections)
            {
                if (connection.OutputPoint.OwnerNode == node || connection.InputPoint.OwnerNode == node)
                    connectionToRemove = connection;
            }

            if(connectionToRemove!=null)
                Connections.Remove(connectionToRemove);

            Repaint();
        }

        /// <summary>
        /// Sets the starting node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void SetStartingNode(Node node)
        {
            foreach (var n in Nodes)
            {
                if (n.First)
                {
                    n.First = false;
                    break;
                }
            }
            node.First = true;
        }

        /// <summary>
        /// Sets the ending node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void SetEndingNode(Node node)
        {
            node.Last = true;
        }

        /// <summary>
        /// Called when [node connector point clicked].
        /// </summary>
        /// <param name="point">The point.</param>
        public void OnNodeConnectorClicked(ConnectionPoint point)
        {
            switch (point.Type)
            {
                case ConnectionType.In:
                    SelectedInPoint = point;
                    if (SelectedOutputPoint == null)
                        return;
                    CreateConnection();
                    break;
                case ConnectionType.Out:
                    SelectedOutputPoint = point;
                    if (SelectedInPoint == null)
                        return;
                    CreateConnection();
                    break;
            }
            GUI.changed = true;
        }

        /// <summary>
        /// Creates a connection.
        /// </summary>
        public void CreateConnection()
        {
            var connectionExists = false;

            foreach (var connection in Connections)
            {
                if (connection.OutputPoint == SelectedOutputPoint || connection.InputPoint == SelectedOutputPoint)
                {
                    connectionExists = true;
                    break;
                }
            }   
            Connections.Add(new Connection(SelectedInPoint, SelectedOutputPoint, OnClickRemoveConnection));
            SelectedInPoint = null;
            SelectedOutputPoint = null;
            Repaint();
        }

        /// <summary>
        /// Called when [click remove connection].
        /// </summary>
        /// <param name="connection">The connection.</param>
        private void OnClickRemoveConnection(Connection connection)
        {
            Connections.Remove(connection);
            Repaint();
        }
    }
#endif
}
