using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPath
{

    [SerializeField, HideInInspector]
    private List<Vector2> _points;

    [SerializeField, HideInInspector]
    private bool _isClosed;

    [SerializeField, HideInInspector]
    private bool _autoSetControlPoints;

    public BezierPath(Vector2 center)
    {
        _points = new List<Vector2>
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right
        };
    }

    public Vector2 this[int i] { get { return _points[i]; } }

    public bool AutoSetControlPoints
    {
        get { return _autoSetControlPoints; }
        set
        {
            if (_autoSetControlPoints != value)
            {
                _autoSetControlPoints = value;
                if (_autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public int NumPoints { get { return _points.Count; } }

    public int NumSegments { get { return _points.Count / 3; } }

    public bool IsClosed
    { 
        get
        {
            return _isClosed;
        }
        set
        {
            if (_isClosed != value)
            {
                _isClosed = value;

                if (_isClosed)
                {
                    _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
                    _points.Add(_points[0] * 2 - _points[1]);
                    if (_autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoint(0);
                        AutoSetAnchorControlPoint(_points.Count - 3);
                    }
                }
                else
                {
                    _points.RemoveRange(_points.Count - 2, 2);
                    if (_autoSetControlPoints)
                    {
                        AutoSetStartAndEndControlPoints();
                    }
                }
            }
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { _points[i * 3], _points[i * 3 + 1], _points[i * 3 + 2], _points[LoopIndex(i * 3 + 3)] };
    }

    public void AddSegment(Vector2 anchorPos)
    {
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        _points.Add((_points[_points.Count - 1] + anchorPos) * 0.5f);
        _points.Add(anchorPos);

        if (_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(_points.Count - 1);
        }
    }

    public void SplitSegment(Vector2 anchorPos, int segmentIndex)
    {
        _points.InsertRange(segmentIndex * 3 + 2, new Vector2[] { Vector2.zero, anchorPos, Vector2.zero });
        if (_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoint(segmentIndex * 3 + 3);
        }
    }
    
    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || !_isClosed && NumSegments > 1) {
            if (anchorIndex == 0)
            {
                if (_isClosed)
                {
                    _points[_points.Count - 1] = _points[2];
                }
                _points.RemoveRange(0, 3);
            }
            else if (anchorIndex == _points.Count - 1 && !_isClosed)
            {
                _points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                _points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public void MoveControlPoint(int i, Vector2 pos)
    {
        _points[LoopIndex(i)] = pos;
    }

    public void MovePoint(int i, Vector2 pos)
    {

        Vector2 deltaMove = pos - _points[i];

        if (i % 3 == 0 || !_autoSetControlPoints)
        {
            _points[i] = pos;

            if (_autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else
            {
                if (i % 3 == 0)
                {
                    if (i + 1 < _points.Count || _isClosed)
                    {
                        _points[LoopIndex(i + 1)] += deltaMove;
                    }
                    if (i - 1 >= 0 || _isClosed)
                    {
                        _points[LoopIndex(i - 1)] += deltaMove;
                    }
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
                    int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

                    if (correspondingControlIndex >= 0 && correspondingControlIndex < _points.Count || _isClosed)
                    {
                        float dst = (_points[LoopIndex(anchorIndex)] - _points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector2 dir = (_points[LoopIndex(anchorIndex)] - pos).normalized;
                        _points[LoopIndex(correspondingControlIndex)] = _points[LoopIndex(anchorIndex)] + dir * dst;
                    }
                }
            }
        }
    }


    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1f, Vector2 pos = new Vector2())
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(_points[0]);
        Vector2 previousPoint = _points[0];
        float dstSinceLastEvenPoint = 0f;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);

            float t = 0f;
            while (t <= 1f)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        if (_isClosed)
        {
            evenlySpacedPoints.Add(evenlySpacedPoints[0]);
        }

        for (int i = 0; i < evenlySpacedPoints.Count; i++)
        {
            evenlySpacedPoints[i] = evenlySpacedPoints[i] - pos;
        }

        return evenlySpacedPoints.ToArray();
    }



    private void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < _points.Count || _isClosed)
            {
                AutoSetAnchorControlPoint(LoopIndex(i));
            }
        }
        AutoSetStartAndEndControlPoints();
    }

    private void AutoSetAllControlPoints()
    {
        for (int i = 0; i < _points.Count; i += 3)
        {
            AutoSetAnchorControlPoint(i);
        }
        AutoSetStartAndEndControlPoints();
    }

    private void AutoSetAnchorControlPoint(int anchorIndex)
    {
        Vector2 anchorPos = _points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < _points.Count || _isClosed)
            {
                _points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    private void AutoSetStartAndEndControlPoints()
    {
        if (!_isClosed)
        {
            _points[1] = (_points[0] + _points[2]) * 0.5f;
            _points[_points.Count - 2] = (_points[_points.Count - 1] + _points[_points.Count - 3]) * 0.5f;
        }
    }

    private int LoopIndex(int i)
    {
        return (i + _points.Count) % _points.Count;
    }
}
