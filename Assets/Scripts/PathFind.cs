using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    private List<NodeRect> _rects;
    private List<NodeEdge> _nodeEdges;
    private List<Vector2> _path;

    private void Awake()
    {
        var n1 = new NodeRect {Rect = new Rect(0, 0, 2, 2)};
        var n2 = new NodeRect {Rect = new Rect(1, 2, 2, 2)};
        var n3 = new NodeRect {Rect = new Rect(3, 1, 1, 2)};
        var n4 = new NodeRect {Rect = new Rect(4, 1, 2, 2)};
        _rects = new List<NodeRect> {n1, n2, n3, n4};

        var e1 = new NodeEdge {Node1 = n1, Node2 = n2, P1 = new Vector2(1, 2), P2 = new Vector2(2, 2)};
        var e2 = new NodeEdge {Node1 = n2, Node2 = n3, P1 = new Vector2(3, 2), P2 = new Vector2(3, 3)};
        var e3 = new NodeEdge {Node1 = n3, Node2 = n4, P1 = new Vector2(4, 1), P2 = new Vector2(4, 3)};

        var p1 = new Vector2(0, 0);
        var p2 = new Vector2(6, 2);
        _nodeEdges = new List<NodeEdge> {e1, e2, e3};
        _path = CreatePath(p1, p2, _nodeEdges, 0.1f);
    }

    private List<Vector2> CreatePath(Vector2 start, Vector2 end, List<NodeEdge> edges, float unitRadius)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(start);
        foreach (var edge in edges)
        {
            if (CanStraightToEnd(path[path.Count - 1], end, edge, unitRadius))
            {
                path.Add(end);
                break;
            }

            path.Add(ChooseCorner(path[path.Count - 1], edge, unitRadius, end));
        }

        path.Add(end);
        return path;
    }

    private bool CanStraightToEnd(Vector3 start, Vector2 end, NodeEdge edge, float unitRadius)
    {
        for (int i = _nodeEdges.Count - 1; i >= _nodeEdges.IndexOf(edge); i--)
        {
            if (!IsInSector(end, _nodeEdges[i], start, unitRadius))
                return false;
        }

        return true;
    }

    private bool IsInSector(Vector2 start, NodeEdge nodeEdge, Vector2 point, float unitRadius)
    {
        var firstSectorSide = nodeEdge.P1 - start + ConstructNormalVector(nodeEdge.P1 - start, unitRadius);
        var secondSectorSide = nodeEdge.P2 - start + ConstructNormalVector(nodeEdge.P2 - start, unitRadius);
        var sectorDot = Vector2.Dot((firstSectorSide).normalized, (secondSectorSide).normalized);
        var firstSidePointDot = Vector2.Dot((firstSectorSide).normalized, (point - start).normalized);
        var secondSidePointDot = Vector2.Dot((secondSectorSide).normalized, (point - start).normalized);
        if (firstSidePointDot < sectorDot || secondSidePointDot < sectorDot)
            return false;
        return true;
    }

    private Vector2 ConstructNormalVector(Vector2 vector, float offset)
    {
        Vector2 normal;
        if (vector.x == 0)
            normal = new Vector2(1, 0);
        else if (vector.y == 0)
            normal = new Vector2(0, 1);
        else
            normal = new Vector2(vector.x + 1, -1 * vector.x * (vector.x + 1) / vector.y);
        var normalizedNormal = normal.normalized;
        return normalizedNormal * offset;
    }

    private Vector2 ChooseCorner(Vector2 start, NodeEdge nodeEdge, float unitRadius, Vector2 end)
    {
        Vector2 corner;
        var nextNodeEdge = _nodeEdges[_nodeEdges.IndexOf(nodeEdge) + 1];
        var middleEdgePoint = new Vector2((nextNodeEdge.P1.x + nextNodeEdge.P2.x) / 2,
            (nextNodeEdge.P1.y + nextNodeEdge.P2.y) / 2);
        var firstSectorSide = nodeEdge.P1 + ConstructNormalVector(nodeEdge.P1 - start, unitRadius);
        var secondSectorSide = nodeEdge.P2 + ConstructNormalVector(nodeEdge.P2 - start, unitRadius);
        var sectorDot = Vector2.Dot((firstSectorSide - start).normalized, (secondSectorSide - start).normalized);
        var secondSidePointDot =
            Vector2.Dot((secondSectorSide - start).normalized, (middleEdgePoint - start).normalized);
        if (secondSidePointDot < sectorDot)
            corner = nodeEdge.P1 + ConstructNormalVector(nodeEdge.P1 - start, unitRadius);
        else
            corner = nodeEdge.P2 - ConstructNormalVector(nodeEdge.P2 - start, unitRadius);
        if (TryFindStraightPathToEnd(corner, end, nodeEdge, unitRadius, out Vector2 betterCorner))
            return betterCorner;
        return corner;
    }

    private bool TryFindStraightPathToEnd(Vector2 corner, Vector2 end, NodeEdge edge, float unitRadius,
        out Vector2 betterCorner)
    {
        betterCorner = Vector2.zero;
        var minLength = corner.magnitude;
        float accuracy = unitRadius;
        corner.Normalize();
        for (int i = 0; edge.Node2.Rect.Contains(corner * (minLength + i * accuracy)); i++)
        {
            Vector2 nextCorner = corner * (minLength + i * accuracy);
            if (CanStraightToEnd(nextCorner, end, _nodeEdges[_nodeEdges.IndexOf(edge) + 1], unitRadius))
            {
                betterCorner = nextCorner;
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        foreach (var rect in _rects)
        {
            DrawRect(rect.Rect);
        }

        foreach (var nodeEdge in _nodeEdges)
        {
            DrawEdge(nodeEdge.P1, nodeEdge.P2);
        }

        DrawPath(_path);
    }

    private void DrawRect(Rect rect)
    {
        Debug.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax));
        Debug.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax));
        Debug.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin));
        Debug.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin));
    }

    private void DrawEdge(Vector2 p1, Vector2 p2)
    {
        Debug.DrawLine(p1, p2, Color.red);
    }

    private void DrawPath(List<Vector2> path)
    {
        if (path != null)
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.green);
            }
    }
}
