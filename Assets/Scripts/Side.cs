using System.Collections.Generic;
using UnityEngine;

public class Side {
    public int id;
    public int lvl;
    public bool exit;
    public bool balcony;
    private Vector3[] points = new Vector3[2];
    private Vector3[] innerPoints = new Vector3[2];
    public void AddBridgeSide(Bridge.Side bo) {
        if (bridgeSides == null)
            bridgeSides = new List<Bridge.Side>();

        bridgeSides.Add(bo);
    }
    public List<Bridge.Side> bridgeSides;

    public Vector3 Get(int i) {
        return points[i];
    }
    public void Set(int i, Vector3 v) {
        points[i] = v;
    }
    public Vector3 GetInner(int i) {
        return innerPoints[i];
    }
    public void SetInner(int i, Vector3 v) {
        innerPoints[i] = v;
    }

    public Vector3 Normal {
        get {
            return Vector3.Cross(Dir, Vector3.up);
        }
    }

    public Vector3 Dir {
        get {
            return (Get(1) - Get(0)).normalized;
        }
    }

    public Vector3 InnerDir {
        get {
            return (GetInner(1) - GetInner(0)).normalized;
        }
    }

    public float InnerDis {
        get {
            return (InnerMid - Mid).magnitude;
        }
    }

    public float InnerLenght {
        get {
            return (GetInner(1) - GetInner(0)).magnitude;
        }
    }

    public float Lenght {
        get {
            return (Get(1) - Get(0)).magnitude;
        }
    }

    public Vector3 Mid {
        get {
            return Get(0) + Dir * Lenght / 2f;
        }
    }

    public Vector3 InnerMid {
        get {
            return GetInner(0) + InnerDir * InnerLenght / 2f;
        }
    }

}
