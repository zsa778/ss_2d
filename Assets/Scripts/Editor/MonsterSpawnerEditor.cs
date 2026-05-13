using UnityEngine;
using UnityEditor;
using SliceShoot.Monster;

[CustomEditor(typeof(MonsterSpawner))]
public class MonsterSpawnerEditor : Editor
{
    // Zone 1=red, 2=orange, 3=green, 4=blue, 5=yellow, 6=magenta
    private static readonly Color[] ZoneColors =
    {
        Color.clear,
        new Color(1f,  0.3f, 0.3f, 1f),  // 1 red
        new Color(1f,  0.6f, 0.2f, 1f),  // 2 orange
        new Color(0.3f,0.9f, 0.3f, 1f),  // 3 green
        new Color(0.3f,0.7f, 1f,   1f),  // 4 blue
        new Color(1f,  1f,   0.3f, 1f),  // 5 yellow
        new Color(0.3f,0.9f, 0.3f, 1f),  // 6 green (left of ring3, mirrors zone 3)
        new Color(1f,  1f,   0.3f, 1f),  // 7 yellow (right of ring3, mirrors zone 5)
    };

    private const float FILL_ALPHA = 0.15f;
    private static readonly Color DarkColor = new Color(0f, 0f, 0f, 0.4f);

    private void OnSceneGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float unit = cam.orthographicSize * 0.5f;
        var so = serializedObject;

        float r1i = so.FindProperty("_ring1Inner").floatValue * unit;
        float r1o = so.FindProperty("_ring1Outer").floatValue * unit;
        float r2i = so.FindProperty("_ring2Inner").floatValue * unit;
        float r2o = so.FindProperty("_ring2Outer").floatValue * unit;
        float r3i = so.FindProperty("_ring3Inner").floatValue * unit;
        float r3o = so.FindProperty("_ring3Outer").floatValue * unit;

        float aTR = so.FindProperty("_angleTopRight").floatValue;
        float aTL = so.FindProperty("_angleTopLeft").floatValue;
        float aBL = so.FindProperty("_angleBotLeft").floatValue;
        float aBR = so.FindProperty("_angleBotRight").floatValue;

        Vector3 c = Vector3.zero;
        Vector3 n = Vector3.back;

        // Draw outside-in: each disc overwrites the interior of the previous,
        // leaving only the annular band (or sector band) visible.

        // Zones 6 & 7: ring3 left and right sectors (top/bottom unzoned → dark)
        FillDisc(c, n, r3o, DarkColor);
        FillSector(c, n, r3o, aTL, aBL,  false, ZoneColors[6]); // zone 6 left
        FillSector(c, n, r3o, aBR, aTR,  true,  ZoneColors[7]); // zone 7 right
        FillDisc(c, n, r3i, DarkColor);        // gap ring2→ring3

        // Zones 2-5: ring2 sectors (pie slices from center to r2o, then erase interior)
        FillSector(c, n, r2o, aTR, aTL,  false, ZoneColors[2]); // top
        FillSector(c, n, r2o, aTL, aBL,  false, ZoneColors[3]); // left
        FillSector(c, n, r2o, aBL, aBR,  false, ZoneColors[4]); // bottom
        FillSector(c, n, r2o, aBR, aTR,  true,  ZoneColors[5]); // right (wraps)
        FillDisc(c, n, r2i, DarkColor);        // erase ring2 interior + gap ring1→ring2

        // Zone 1: ring1 full circle
        FillDisc(c, n, r1o, ZoneColors[1]);
        FillDisc(c, n, r1i, DarkColor);        // erase center (non-spawn)

        // Wire outlines
        Handles.color = new Color(1f, 1f, 1f, 0.5f);
        foreach (float r in new[] { r1i, r1o, r2i, r2o, r3i, r3o })
            Handles.DrawWireDisc(c, n, r);

        // Sector divider lines — all 4 from r2i outward, only left/right reach r3o
        Handles.color = new Color(1f, 1f, 1f, 0.4f);
        foreach (float a in new[] { aTR, aBR })   // right boundary lines stop at r2o (no zone in ring3 top/bot)
            Handles.DrawLine(c + AngleDir(a) * r2i, c + AngleDir(a) * r3o);
        foreach (float a in new[] { aTL, aBL })   // left boundary lines reach ring3
            Handles.DrawLine(c + AngleDir(a) * r2i, c + AngleDir(a) * r3o);

        // Zone labels
        GUIStyle style = new GUIStyle { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = Color.white;

        Handles.Label(c + Vector3.up * Mid(r1i, r1o), "1", style);
        Handles.Label(c + AngleDir(Mid(aTR, aTL))          * Mid(r2i, r2o), "2", style);
        Handles.Label(c + AngleDir(Mid(aTL, aBL))          * Mid(r2i, r2o), "3", style);
        Handles.Label(c + AngleDir(Mid(aBL, aBR))          * Mid(r2i, r2o), "4", style);
        Handles.Label(c + AngleDir(WrapMid(aBR, aTR))      * Mid(r2i, r2o), "5", style);
        Handles.Label(c + AngleDir(Mid(aTL, aBL))     * Mid(r3i, r3o), "6", style);
        Handles.Label(c + AngleDir(WrapMid(aBR, aTR)) * Mid(r3i, r3o), "7", style);
    }

    private void FillDisc(Vector3 c, Vector3 n, float r, Color color)
    {
        Handles.color = new Color(color.r, color.g, color.b, color.a * FILL_ALPHA);
        Handles.DrawSolidArc(c, n, Vector3.right, 360f, r);
    }

    private void FillSector(Vector3 c, Vector3 n, float r, float aMin, float aMax, bool wrap, Color color)
    {
        float span;
        Vector3 from;
        if (wrap)
        {
            span = (360f - aMin) + aMax;
            from = AngleDir(aMin);
        }
        else
        {
            span = aMax - aMin;
            from = AngleDir(aMin);
        }
        Handles.color = new Color(color.r, color.g, color.b, color.a * FILL_ALPHA);
        Handles.DrawSolidArc(c, n, from, span, r);
    }

    private float Mid(float a, float b) => (a + b) * 0.5f;

    private float WrapMid(float aMin, float aMax)
    {
        float span = (360f - aMin) + aMax;
        float mid  = aMin + span * 0.5f;
        return mid >= 360f ? mid - 360f : mid;
    }

    private Vector3 AngleDir(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }
}
