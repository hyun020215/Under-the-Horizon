using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EvidenceConnectionGraphic : Graphic
{
    private readonly List<(RectTransform from, RectTransform to)> connections = new();
    [SerializeField, Min(1f)] private float thickness = 4f;

    public void SetConnections(IEnumerable<(RectTransform from, RectTransform to)> value)
    {
        connections.Clear();
        if (value != null)
            connections.AddRange(value);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper helper)
    {
        helper.Clear();
        foreach ((RectTransform from, RectTransform to) in connections)
        {
            if (from == null || to == null)
                continue;
            Vector2 start = rectTransform.InverseTransformPoint(from.TransformPoint(from.rect.center));
            Vector2 end = rectTransform.InverseTransformPoint(to.TransformPoint(to.rect.center));
            Vector2 direction = end - start;
            if (direction.sqrMagnitude < .01f)
                continue;
            Vector2 normal = new(-direction.y, direction.x);
            normal = normal.normalized * thickness * .5f;
            int index = helper.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = start - normal; helper.AddVert(vertex);
            vertex.position = start + normal; helper.AddVert(vertex);
            vertex.position = end + normal; helper.AddVert(vertex);
            vertex.position = end - normal; helper.AddVert(vertex);
            helper.AddTriangle(index, index + 1, index + 2);
            helper.AddTriangle(index, index + 2, index + 3);
        }
    }
}
