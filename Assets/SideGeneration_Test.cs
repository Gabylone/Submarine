using Cinemachine.Utility;
using ConcaveHull;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class SideGeneration_Test : MonoBehaviour {
    public float sideWidth = 2f;
    [Range(0,1)]
    public float tension = 0f;
    Color[] randomColors = new Color[6] {
        Color.yellow,
        Color.cyan,
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
    };
    public SplineContainer spline;
    public float mult = 1f;
    [Range(0,1)]
    public float lerpSpeed = 0f;
    public float concaveLenght = 0f;
    public float test;
    public int knobCount = 0;

    private void OnDrawGizmos() {
        var points = GetComponentsInChildren<Transform>().Select(x => x.position).ToList();
        points.RemoveAt(0);
        spline.Spline.Clear();
        concaveLenght = 0;
        for (int i = 0; i < points.Count; i++) {
            var knot = new BezierKnot(new Unity.Mathematics.float3(points[i].x, 0f, points[i].z));
            spline.Spline.Add(knot, TangentMode.AutoSmooth, tension);
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            concaveLenght += (points[(i + 1) % points.Count]- points[i]).magnitude;
        }

        test = sideWidth / concaveLenght;

        var sides = new List<Vector3>();
        var lerp = 0f;
        while ( lerp < 1) {
            if (lerpSpeed == 0)
                break;
            var result = spline.Spline.EvaluatePosition(lerp);
            sides.Add(result);
            Gizmos.DrawSphere(result, 0.1f);
            lerp += test;
        }

        for (int i = 0; i < sides.Count; i++) {
            Gizmos.color = randomColors[i % randomColors.Length];
            Gizmos.DrawLine(sides[i], sides[(i+1)%sides.Count]);
        }

        /*var index = 0;
        for (int i = 0; i < points.Count; i++) {
            var start = points[i];
            var prout = points[(i + 2)%points.Count];
            var end = points[(i + 1)%points.Count];
            var dir = (end - start);
            int sideCount = (int)(dir.magnitude / sideWidth);
            if (sideCount < 1) sideCount = 1;
            var dif = dir.magnitude - (sideWidth * sideCount);
            var w = dif / sideCount;
            Debug.LogFormat($"side count : {sideCount}");
            // maybe add like a change to have a squigly / sincos side ? like a arc side ? zigzag side? 

            for (int sideIndex = 0; sideIndex < sideCount; sideIndex++) {

                var splineLerp = ((float)i / points.Count) + ((float)sideIndex / sideCount);
                Gizmos.color = Color.white;
                
                var a = start + dir.normalized * (sideWidth + w) * sideIndex;
                var bb = a + dir.normalized * (sideWidth + w);
                var mid = a + (bb - a) / 2f;
                Gizmos.DrawSphere(mid, 1f);
                //Handles.Label((mid+Vector3.up * 0.3f), index.ToString());
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(a, bb);
                ++index;
            }
        }*/

    }
}