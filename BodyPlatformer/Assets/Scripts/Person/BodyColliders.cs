using UnityEngine;
using System.Collections.Generic;

public class BodyColliders : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;

    [Header("Edge Colliders")]
    [SerializeField] private float edgeRadius = 0.03f;

    [Header("Head")]
    [SerializeField] private float headHeightMultiplier = 0.55f;
    [SerializeField] private float headWidthMultiplier = 1.15f;

    private EdgeCollider2D leftArmCollider;
    private EdgeCollider2D shouldersCollider;
    private EdgeCollider2D rightArmCollider;

    private EdgeCollider2D leftLegCollider;
    private EdgeCollider2D rightLegCollider;

    private EdgeCollider2D headCollider;

    private EdgeCollider2D leftHandCollider;
    private EdgeCollider2D rightHandCollider;


    private void Start()
    {
        int bodyLayer = LayerMask.NameToLayer("Body");

        leftArmCollider =
            CreateEdgeCollider(
                "LeftArmCollider",
                bodyLayer
            );

        shouldersCollider =
            CreateEdgeCollider(
                "ShouldersCollider",
                bodyLayer
            );

        rightArmCollider =
            CreateEdgeCollider(
                "RightArmCollider",
                bodyLayer
            );

        leftLegCollider =
            CreateEdgeCollider(
                "LeftLegCollider",
                bodyLayer
            );

        rightLegCollider =
            CreateEdgeCollider(
                "RightLegCollider",
                bodyLayer
            );

        headCollider =
            CreateEdgeCollider(
                "HeadCollider",
                bodyLayer
            );

        leftHandCollider =
            CreateEdgeCollider(
                "LeftHandCollider",
                bodyLayer
            );

        rightHandCollider =
            CreateEdgeCollider(
                "RightHandCollider",
                bodyLayer
            );

        // Las manos empiezan desactivadas.
        // HandPlatformController decidirá cuándo
        // pueden funcionar como plataforma.
        leftHandCollider.enabled = false;
        rightHandCollider.enabled = false;
    }


    private EdgeCollider2D CreateEdgeCollider(
        string objectName,
        int layer
    )
    {
        GameObject obj =
            new GameObject(objectName);

        obj.transform.SetParent(transform);

        obj.transform.localPosition =
            Vector3.zero;

        obj.transform.localRotation =
            Quaternion.identity;

        obj.transform.localScale =
            Vector3.one;

        obj.layer = layer;

        EdgeCollider2D edge =
            obj.AddComponent<EdgeCollider2D>();

        edge.edgeRadius = edgeRadius;

        return edge;
    }


    private void FixedUpdate()
    {
        if (poseReceiver == null)
            return;

        if (!poseReceiver.IsPersonDetected())
        {
            DisableAllColliders();
            return;
        }

        GameObject[] points =
            poseReceiver.GetLandmarkObjects();

        if (
            points == null ||
            points.Length < 33
        )
        {
            DisableAllColliders();
            return;
        }

        UpdateLeftArm(points);
        UpdateShoulders(points);
        UpdateRightArm(points);

        UpdateLeftLeg(points);
        UpdateRightLeg(points);

        UpdateHead(points);

        // Ahora las manos utilizan
        // los 21 landmarks detallados.
        UpdateLeftHand();
        UpdateRightHand();
    }


    private void DisableAllColliders()
    {
        if (leftArmCollider != null)
            leftArmCollider.enabled = false;

        if (shouldersCollider != null)
            shouldersCollider.enabled = false;

        if (rightArmCollider != null)
            rightArmCollider.enabled = false;

        if (leftLegCollider != null)
            leftLegCollider.enabled = false;

        if (rightLegCollider != null)
            rightLegCollider.enabled = false;

        if (headCollider != null)
            headCollider.enabled = false;

        if (leftHandCollider != null)
            leftHandCollider.enabled = false;

        if (rightHandCollider != null)
            rightHandCollider.enabled = false;
    }


    private bool AreVisible(
        params int[] indexes
    )
    {
        foreach (int index in indexes)
        {
            if (
                !poseReceiver
                    .IsLandmarkVisible(index)
            )
            {
                return false;
            }
        }

        return true;
    }


    // =========================================================
    // BRAZO IZQUIERDO
    // =========================================================

    private void UpdateLeftArm(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                15,
                13,
                11
            );

        leftArmCollider.enabled =
            visible;

        if (!visible)
            return;

        leftArmCollider.points =
            new Vector2[]
            {
                GetLocalPoint(
                    points[15],
                    leftArmCollider
                ),

                GetLocalPoint(
                    points[13],
                    leftArmCollider
                ),

                GetLocalPoint(
                    points[11],
                    leftArmCollider
                )
            };
    }


    // =========================================================
    // HOMBROS
    // =========================================================

    private void UpdateShoulders(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                11,
                12
            );

        shouldersCollider.enabled =
            visible;

        if (!visible)
            return;

        shouldersCollider.points =
            new Vector2[]
            {
                GetLocalPoint(
                    points[11],
                    shouldersCollider
                ),

                GetLocalPoint(
                    points[12],
                    shouldersCollider
                )
            };
    }


    // =========================================================
    // BRAZO DERECHO
    // =========================================================

    private void UpdateRightArm(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                12,
                14,
                16
            );

        rightArmCollider.enabled =
            visible;

        if (!visible)
            return;

        rightArmCollider.points =
            new Vector2[]
            {
                GetLocalPoint(
                    points[12],
                    rightArmCollider
                ),

                GetLocalPoint(
                    points[14],
                    rightArmCollider
                ),

                GetLocalPoint(
                    points[16],
                    rightArmCollider
                )
            };
    }


    // =========================================================
    // PIERNA IZQUIERDA
    // =========================================================

    private void UpdateLeftLeg(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                23,
                25,
                27
            );

        leftLegCollider.enabled =
            visible;

        if (!visible)
            return;

        leftLegCollider.points =
            new Vector2[]
            {
                GetLocalPoint(
                    points[23],
                    leftLegCollider
                ),

                GetLocalPoint(
                    points[25],
                    leftLegCollider
                ),

                GetLocalPoint(
                    points[27],
                    leftLegCollider
                )
            };
    }


    // =========================================================
    // PIERNA DERECHA
    // =========================================================

    private void UpdateRightLeg(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                24,
                26,
                28
            );

        rightLegCollider.enabled =
            visible;

        if (!visible)
            return;

        rightLegCollider.points =
            new Vector2[]
            {
                GetLocalPoint(
                    points[24],
                    rightLegCollider
                ),

                GetLocalPoint(
                    points[26],
                    rightLegCollider
                ),

                GetLocalPoint(
                    points[28],
                    rightLegCollider
                )
            };
    }


    // =========================================================
    // CABEZA
    // =========================================================

    private void UpdateHead(
        GameObject[] points
    )
    {
        bool visible =
            AreVisible(
                0,
                7,
                8
            );

        headCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector3 leftEar =
            points[7]
                .transform
                .position;

        Vector3 rightEar =
            points[8]
                .transform
                .position;

        Vector3 center =
            (leftEar + rightEar) *
            0.5f;

        float width =
            Vector2.Distance(
                leftEar,
                rightEar
            ) *
            headWidthMultiplier;

        float height =
            width *
            headHeightMultiplier;

        float radiusX =
            width *
            0.5f;

        float radiusY =
            height;

        int segments = 20;

        Vector2[] edgePoints =
            new Vector2[
                segments + 1
            ];

        for (
            int i = 0;
            i <= segments;
            i++
        )
        {
            float t =
                (float)i /
                segments;

            float angle =
                Mathf.PI *
                (1f - t);

            Vector3 worldPoint =
                new Vector3(
                    center.x +
                    Mathf.Cos(angle) *
                    radiusX,

                    center.y +
                    Mathf.Sin(angle) *
                    radiusY,

                    0f
                );

            edgePoints[i] =
                headCollider
                    .transform
                    .InverseTransformPoint(
                        worldPoint
                    );
        }

        headCollider.points =
            edgePoints;
    }


    // =========================================================
    // MANO IZQUIERDA
    // =========================================================

    private void UpdateLeftHand()
    {
        if (
            !poseReceiver
                .IsLeftHandDetected()
        )
        {
            leftHandCollider.enabled =
                false;

            return;
        }

        GameObject[] handPoints =
            poseReceiver
                .GetLeftHandLandmarks();

        UpdateDetailedHandCollider(
            handPoints,
            leftHandCollider
        );
    }


    // =========================================================
    // MANO DERECHA
    // =========================================================

    private void UpdateRightHand()
    {
        if (
            !poseReceiver
                .IsRightHandDetected()
        )
        {
            rightHandCollider.enabled =
                false;

            return;
        }

        GameObject[] handPoints =
            poseReceiver
                .GetRightHandLandmarks();

        UpdateDetailedHandCollider(
            handPoints,
            rightHandCollider
        );
    }


    // =========================================================
    // CREAR SUPERFICIE COMPLETA DE LA MANO
    // =========================================================

    private void UpdateDetailedHandCollider(
        GameObject[] handPoints,
        EdgeCollider2D handCollider
    )
    {
        if (
            handPoints == null ||
            handPoints.Length < 21
        )
        {
            return;
        }

        List<Vector2> worldPoints =
            new List<Vector2>();

        for (
            int i = 0;
            i < handPoints.Length;
            i++
        )
        {
            if (handPoints[i] == null)
                continue;

            Vector3 position =
                handPoints[i]
                    .transform
                    .position;

            worldPoints.Add(
                new Vector2(
                    position.x,
                    position.y
                )
            );
        }

        if (worldPoints.Count < 3)
            return;


        // -----------------------------------------
        // Sacamos el contorno exterior de la mano
        // -----------------------------------------

        List<Vector2> hull =
            CalculateConvexHull(
                worldPoints
            );

        if (hull.Count < 2)
            return;


        // -----------------------------------------
        // Buscamos izquierda y derecha del contorno
        // -----------------------------------------

        int leftIndex = 0;
        int rightIndex = 0;

        for (
            int i = 1;
            i < hull.Count;
            i++
        )
        {
            if (
                hull[i].x <
                hull[leftIndex].x
            )
            {
                leftIndex = i;
            }

            if (
                hull[i].x >
                hull[rightIndex].x
            )
            {
                rightIndex = i;
            }
        }


        // Hay dos caminos posibles entre
        // izquierda y derecha del contorno.
        //
        // Elegimos el que esté más arriba,
        // porque esa será la superficie sobre
        // la que puede pararse el Player.

        List<Vector2> pathA =
            GetHullPath(
                hull,
                leftIndex,
                rightIndex,
                1
            );

        List<Vector2> pathB =
            GetHullPath(
                hull,
                leftIndex,
                rightIndex,
                -1
            );

        List<Vector2> surface =
            GetAverageY(pathA) >=
            GetAverageY(pathB)
                ? pathA
                : pathB;


        if (surface.Count < 2)
            return;


        // -----------------------------------------
        // Centro de la superficie
        // -----------------------------------------

        Vector2 center =
            Vector2.zero;

        foreach (
            Vector2 point
            in surface
        )
        {
            center += point;
        }

        center /=
            surface.Count;


        // IMPORTANTE:
        // movemos el Transform del collider.
        //
        // Esto permite que posteriormente
        // podamos tratar la mano como una
        // plataforma móvil real.

        handCollider
            .transform
            .position =
            new Vector3(
                center.x,
                center.y,
                0f
            );


        // -----------------------------------------
        // Convertimos superficie a coordenadas
        // locales del EdgeCollider
        // -----------------------------------------

        Vector2[] localSurface =
            new Vector2[
                surface.Count
            ];

        for (
            int i = 0;
            i < surface.Count;
            i++
        )
        {
            Vector3 local =
                handCollider
                    .transform
                    .InverseTransformPoint(
                        new Vector3(
                            surface[i].x,
                            surface[i].y,
                            0f
                        )
                    );

            localSurface[i] =
                new Vector2(
                    local.x,
                    local.y
                );
        }

        handCollider.points =
            localSurface;

        // OJO:
        // NO activamos el collider aquí.
        //
        // HandPlatformController es quien
        // decide si debe estar activo según
        // Flat / Sideways.
    }


    // =========================================================
    // CONVEX HULL
    // =========================================================

    private List<Vector2> CalculateConvexHull(
        List<Vector2> points
    )
    {
        List<Vector2> sorted =
            new List<Vector2>(
                points
            );

        sorted.Sort(
            (a, b) =>
            {
                int compareX =
                    a.x.CompareTo(
                        b.x
                    );

                if (compareX != 0)
                    return compareX;

                return a.y.CompareTo(
                    b.y
                );
            }
        );

        if (sorted.Count <= 2)
            return sorted;


        List<Vector2> lower =
            new List<Vector2>();

        foreach (
            Vector2 point
            in sorted
        )
        {
            while (
                lower.Count >= 2 &&
                Cross(
                    lower[
                        lower.Count - 2
                    ],
                    lower[
                        lower.Count - 1
                    ],
                    point
                ) <= 0f
            )
            {
                lower.RemoveAt(
                    lower.Count - 1
                );
            }

            lower.Add(point);
        }


        List<Vector2> upper =
            new List<Vector2>();

        for (
            int i =
                sorted.Count - 1;
            i >= 0;
            i--
        )
        {
            Vector2 point =
                sorted[i];

            while (
                upper.Count >= 2 &&
                Cross(
                    upper[
                        upper.Count - 2
                    ],
                    upper[
                        upper.Count - 1
                    ],
                    point
                ) <= 0f
            )
            {
                upper.RemoveAt(
                    upper.Count - 1
                );
            }

            upper.Add(point);
        }


        lower.RemoveAt(
            lower.Count - 1
        );

        upper.RemoveAt(
            upper.Count - 1
        );


        List<Vector2> hull =
            new List<Vector2>();

        hull.AddRange(lower);
        hull.AddRange(upper);

        return hull;
    }


    private float Cross(
        Vector2 origin,
        Vector2 a,
        Vector2 b
    )
    {
        return
            (a.x - origin.x) *
            (b.y - origin.y)
            -
            (a.y - origin.y) *
            (b.x - origin.x);
    }


    // =========================================================
    // OBTENER UN CAMINO DEL CONTORNO
    // =========================================================

    private List<Vector2> GetHullPath(
        List<Vector2> hull,
        int start,
        int end,
        int direction
    )
    {
        List<Vector2> path =
            new List<Vector2>();

        int index = start;

        path.Add(
            hull[index]
        );

        int safety = 0;

        while (
            index != end &&
            safety <
            hull.Count + 2
        )
        {
            index += direction;

            if (index >= hull.Count)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index =
                    hull.Count - 1;
            }

            path.Add(
                hull[index]
            );

            safety++;
        }

        return path;
    }


    private float GetAverageY(
        List<Vector2> points
    )
    {
        if (points.Count == 0)
            return 0f;

        float total = 0f;

        foreach (
            Vector2 point
            in points
        )
        {
            total += point.y;
        }

        return
            total /
            points.Count;
    }


    // =========================================================
    // UTILIDAD
    // =========================================================

    private Vector2 GetLocalPoint(
        GameObject landmark,
        EdgeCollider2D collider
    )
    {
        return collider
            .transform
            .InverseTransformPoint(
                landmark
                    .transform
                    .position
            );
    }
}