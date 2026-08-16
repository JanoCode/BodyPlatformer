using UnityEngine;

public class BodyColliders : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;

    [SerializeField] private float edgeRadius = 0.08f;

    private readonly int[,] connections =
    {
        {11, 12}, // hombros

        {11, 13}, // brazo izquierdo
        {13, 15},

        {12, 14}, // brazo derecho
        {14, 16},

        {11, 23}, // torso izquierdo
        {12, 24}, // torso derecho
        {23, 24}, // cadera

        {23, 25}, // pierna izquierda
        {25, 27},

        {24, 26}, // pierna derecha
        {26, 28}
    };

    private EdgeCollider2D[] colliders;

    private void Start()
    {
        colliders = new EdgeCollider2D[connections.GetLength(0)];

        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject colliderObject =
                new GameObject("BodyCollider_" + i);

            colliderObject.transform.parent = transform;

            EdgeCollider2D edge =
                colliderObject.AddComponent<EdgeCollider2D>();

            edge.edgeRadius = edgeRadius;

            colliders[i] = edge;
        }
    }

    private void FixedUpdate()
    {
        if (poseReceiver == null)
            return;

        GameObject[] points =
            poseReceiver.GetLandmarkObjects();

        if (points == null || points.Length < 33)
            return;

        for (int i = 0; i < colliders.Length; i++)
        {
            int startIndex = connections[i, 0];
            int endIndex = connections[i, 1];

            if (points[startIndex] == null ||
                points[endIndex] == null)
                continue;

            Vector2 start =
                transform.InverseTransformPoint(
                    points[startIndex].transform.position
                );

            Vector2 end =
                transform.InverseTransformPoint(
                    points[endIndex].transform.position
                );

            colliders[i].points =
                new Vector2[]
                {
                    start,
                    end
                };
        }
    }
}