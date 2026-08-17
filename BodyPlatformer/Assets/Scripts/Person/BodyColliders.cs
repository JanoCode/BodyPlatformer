using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class BodyColliders : MonoBehaviour
{
    [SerializeField]
    private PoseReceiver poseReceiver;

    [Header("Edge Colliders")]
    [SerializeField]
    private float edgeRadius = 0.03f;

    [Header("Head")]
    [SerializeField]
    private float headHeightMultiplier = 0.55f;

    [SerializeField]
    private float headWidthMultiplier = 1.15f;


    private EdgeCollider2D leftArmCollider;
    private EdgeCollider2D shouldersCollider;
    private EdgeCollider2D rightArmCollider;

    private EdgeCollider2D leftLegCollider;
    private EdgeCollider2D rightLegCollider;

    private EdgeCollider2D headCollider;

    private EdgeCollider2D leftHandCollider;
    private EdgeCollider2D rightHandCollider;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        int bodyLayer =
            LayerMask.NameToLayer("Body");

        leftArmCollider =
            CreateBodyPlatform(
                "LeftArmCollider",
                bodyLayer
            );

        shouldersCollider =
            CreateBodyPlatform(
                "ShouldersCollider",
                bodyLayer
            );

        rightArmCollider =
            CreateBodyPlatform(
                "RightArmCollider",
                bodyLayer
            );

        leftLegCollider =
            CreateBodyPlatform(
                "LeftLegCollider",
                bodyLayer
            );

        rightLegCollider =
            CreateBodyPlatform(
                "RightLegCollider",
                bodyLayer
            );

        headCollider =
            CreateBodyPlatform(
                "HeadCollider",
                bodyLayer
            );

        leftHandCollider =
            CreateBodyPlatform(
                "LeftHandCollider",
                bodyLayer
            );

        rightHandCollider =
            CreateBodyPlatform(
                "RightHandCollider",
                bodyLayer
            );

        // Las manos solo se activarán cuando
        // HandPlatformController diga que son utilizables.
        leftHandCollider.enabled = false;
        rightHandCollider.enabled = false;
    }


    // =========================================================
    // CREAR PLATAFORMA CORPORAL
    // =========================================================

    private EdgeCollider2D CreateBodyPlatform(
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


        // Rigidbody cinemático.
        Rigidbody2D rb =
            obj.AddComponent<Rigidbody2D>();

        rb.bodyType =
            RigidbodyType2D.Kinematic;

        rb.gravityScale = 0f;

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;

        rb.freezeRotation = true;


        // Collider.
        EdgeCollider2D edge =
            obj.AddComponent<EdgeCollider2D>();

        edge.edgeRadius =
            edgeRadius;

        return edge;
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

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

        UpdateLeftHand();
        UpdateRightHand();
    }


    // =========================================================
    // DESACTIVAR
    // =========================================================

    private void DisableAllColliders()
    {
        DisableCollider(leftArmCollider);
        DisableCollider(shouldersCollider);
        DisableCollider(rightArmCollider);

        DisableCollider(leftLegCollider);
        DisableCollider(rightLegCollider);

        DisableCollider(headCollider);

        DisableCollider(leftHandCollider);
        DisableCollider(rightHandCollider);
    }


    private void DisableCollider(
        EdgeCollider2D collider
    )
    {
        if (collider != null)
        {
            collider.enabled = false;
        }
    }


    // =========================================================
    // VISIBILIDAD
    // =========================================================

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
            AreVisible(15, 13, 11);

        leftArmCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 wrist =
            points[15].transform.position;

        Vector2 elbow =
            points[13].transform.position;

        Vector2 shoulder =
            points[11].transform.position;

        Vector2 center =
            (wrist + elbow + shoulder) /
            3f;

        MovePlatform(
            leftArmCollider,
            center
        );

        leftArmCollider.points =
            new Vector2[]
            {
                wrist - center,
                elbow - center,
                shoulder - center
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
            AreVisible(11, 12);

        shouldersCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 left =
            points[11].transform.position;

        Vector2 right =
            points[12].transform.position;

        Vector2 center =
            (left + right) * 0.5f;

        MovePlatform(
            shouldersCollider,
            center
        );

        shouldersCollider.points =
            new Vector2[]
            {
                left - center,
                right - center
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
            AreVisible(12, 14, 16);

        rightArmCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 shoulder =
            points[12].transform.position;

        Vector2 elbow =
            points[14].transform.position;

        Vector2 wrist =
            points[16].transform.position;

        Vector2 center =
            (shoulder + elbow + wrist) /
            3f;

        MovePlatform(
            rightArmCollider,
            center
        );

        rightArmCollider.points =
            new Vector2[]
            {
                shoulder - center,
                elbow - center,
                wrist - center
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
            AreVisible(23, 25, 27);

        leftLegCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 hip =
            points[23].transform.position;

        Vector2 knee =
            points[25].transform.position;

        Vector2 ankle =
            points[27].transform.position;

        Vector2 center =
            (hip + knee + ankle) /
            3f;

        MovePlatform(
            leftLegCollider,
            center
        );

        leftLegCollider.points =
            new Vector2[]
            {
                hip - center,
                knee - center,
                ankle - center
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
            AreVisible(24, 26, 28);

        rightLegCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 hip =
            points[24].transform.position;

        Vector2 knee =
            points[26].transform.position;

        Vector2 ankle =
            points[28].transform.position;

        Vector2 center =
            (hip + knee + ankle) /
            3f;

        MovePlatform(
            rightLegCollider,
            center
        );

        rightLegCollider.points =
            new Vector2[]
            {
                hip - center,
                knee - center,
                ankle - center
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
            AreVisible(0, 7, 8);

        headCollider.enabled =
            visible;

        if (!visible)
            return;

        Vector2 leftEar =
            points[7].transform.position;

        Vector2 rightEar =
            points[8].transform.position;

        Vector2 center =
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

            // Ahora son coordenadas LOCALES
            // relativas al centro del Rigidbody.
            edgePoints[i] =
                new Vector2(
                    Mathf.Cos(angle) *
                    radiusX,

                    Mathf.Sin(angle) *
                    radiusY
                );
        }

        MovePlatform(
            headCollider,
            center
        );

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
    // SUPERFICIE DETALLADA DE MANO
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
            if (
                handPoints[i] == null ||
                !handPoints[i]
                    .activeInHierarchy
            )
            {
                continue;
            }

            Vector2 position =
                handPoints[i]
                    .transform
                    .position;

            worldPoints.Add(
                position
            );
        }

        if (worldPoints.Count < 3)
            return;


        List<Vector2> hull =
            CalculateConvexHull(
                worldPoints
            );

        if (hull.Count < 2)
            return;


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


        // El Rigidbody cinemático se mueve
        // hacia el centro de la mano.
        MovePlatform(
            handCollider,
            center
        );


        // Convertimos los puntos del mundo
        // en offsets locales respecto del centro.
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
            localSurface[i] =
                surface[i] -
                center;
        }

        handCollider.points =
            localSurface;


        // IMPORTANTE:
        //
        // aquí NO ponemos:
        // handCollider.enabled = true;
        //
        // HandPlatformController sigue siendo
        // quien decide Flat / Sideways.
    }


    // =========================================================
    // MOVER RIGIDBODY CINEMÁTICO
    // =========================================================

    private void MovePlatform(
        EdgeCollider2D collider,
        Vector2 targetPosition
    )
    {
        if (collider == null)
            return;

        Rigidbody2D rb =
            collider.GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        rb.MovePosition(
            targetPosition
        );
    }


    // =========================================================
    // CONVEX HULL MANO
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

                return
                    a.y.CompareTo(
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
    // CAMINO DEL CONTORNO
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

        int index =
            start;

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

            if (
                index >=
                hull.Count
            )
            {
                index = 0;
            }
            else if (
                index < 0
            )
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
}