using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator Creator;
    private BezierPath _path
    {
        get
        {
            return Creator.Path;
        }
    }

    private const float _segmentSelectDistanceThreshold = 0.1f;
    private int _selectedSegmentIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create New"))
        {
            Undo.RecordObject(Creator, "Create New");
            Creator.CreatePath();
        }

        if (GUILayout.Button("Match Sprite Shape Spline"))
        {
            Undo.RecordObject(Creator, "Match Sprite Shape Spline");
            Creator.MatchSpriteShapeSpline();
        }

        if (GUILayout.Button("Set Collider"))
        {
            Undo.RecordObject(Creator, "Set Collider");
            Creator.SetCollider();
        }

        bool isClosed = GUILayout.Toggle(_path.IsClosed, "Closed");
        if (isClosed != _path.IsClosed)
        {
            Undo.RecordObject(Creator, "Toggle closed");
            _path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(_path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != _path.AutoSetControlPoints)
        {
            Undo.RecordObject(Creator, "Toggle Auto Set Control Points");
            _path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {

            if (_selectedSegmentIndex != -1)
            {
                Undo.RecordObject(Creator, "Split Segment");
                _path.SplitSegment(mousePos, _selectedSegmentIndex);
            }
            else if (!_path.IsClosed)
            {
                Undo.RecordObject(Creator, "Add Segment");
                _path.AddSegment(mousePos);
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDstToAnchor = Creator.AnchorDiameter * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < _path.NumPoints; i += 3)
            {
                float dst = Vector2.Distance(mousePos, _path[i]);
                if (dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(Creator, "Delete Segment");
                _path.DeleteSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDstToSegment = _segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;
            for (int i = 0; i < _path.NumSegments; i++)
            {
                Vector2[] points = _path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minDstToSegment)
                {
                    minDstToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != _selectedSegmentIndex)
            {
                _selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }

    private void Draw()
    {

        for (int i = 0; i < _path.NumSegments; i++)
        {
            Vector2[] points = _path.GetPointsInSegment(i);

            if (Creator.DisplayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);
                Color segmentColor = (i == _selectedSegmentIndex && Event.current.shift) ? Creator.SelectedSegmentColor : Creator.SegmentColor;
                Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2f);
            }
        }

        for (int i = 0; i < _path.NumPoints; i++)
        {
            if (i % 3 == 0 || Creator.DisplayControlPoints)
            {
                Handles.color = (i % 3 == 0) ? Creator.AnchorColor : Creator.ControlColor;
                float handleSize = (i % 3 == 0) ? Creator.AnchorDiameter : Creator.ControlDiameter;
                Vector2 newPos = Handles.FreeMoveHandle(_path[i], Quaternion.identity, handleSize, Vector2.zero, Handles.CylinderHandleCap);

                if (_path[i] != newPos)
                {
                    Undo.RecordObject(Creator, "Move Point");
                    _path.MovePoint(i, newPos);
                }
            }
        }
    }

    private void OnEnable()
    {
        Creator = (PathCreator)target;
        if (Creator.Path == null)
        {
            Creator.CreatePath();
        }
    }

}
