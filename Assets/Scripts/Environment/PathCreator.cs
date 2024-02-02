using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PathCreator : MonoBehaviour
{

    [HideInInspector]
    public BezierPath Path;
    // Can probaly make these local vars
    [HideInInspector]
    public EdgeCollider2D EdgeCollider;
    [HideInInspector]
    public SpriteShapeController SpriteShapeController;

    public Color AnchorColor = Color.red;
    public Color ControlColor = Color.white;
    public Color SegmentColor = Color.green;
    public Color SelectedSegmentColor = Color.yellow;
    public float AnchorDiameter = 0.1f;
    public float ControlDiameter = 0.075f;
    public bool DisplayControlPoints = false;
    public float ColliderPointSpacing = 0.1f;
    public float ColliderPointResolution = 1f;

    public void CreatePath()
    {
        Path = new BezierPath(transform.position);
    }

    public void MatchSpriteShapeSpline()
    {
        SpriteShapeController = GetComponent<SpriteShapeController>();

        if (SpriteShapeController == null)
        {
            Debug.LogError("No SpriteShapeController found");
            return;
        }

        Spline spline = SpriteShapeController.spline;

        for (int i = 0; i < spline.GetPointCount() * 3; i+=3)
        {
            Path.MovePoint(i, spline.GetPosition(i / 3) + transform.position);
            Path.MoveControlPoint(i - 1, spline.GetLeftTangent(i / 3) + spline.GetPosition(i / 3) + transform.position);
            Path.MoveControlPoint(i + 1, spline.GetRightTangent(i / 3) + spline.GetPosition(i / 3) + transform.position);
        }
    }
    
    public void SetCollider()
    {
        EdgeCollider = GetComponent<EdgeCollider2D>();

        if (EdgeCollider == null)
        {
            Debug.LogError("No EdgeCollider2D found");
            return;
        }

        EdgeCollider.points = Path.CalculateEvenlySpacedPoints(ColliderPointSpacing, ColliderPointResolution, transform.position);
    }

    private void Reset()
    {
        CreatePath();
    }
}
