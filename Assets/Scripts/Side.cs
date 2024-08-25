using System.Collections.Generic;
using UnityEngine;

public class Side {
    public int id;
    public int lvl;
    public bool exit;
    public bool balcony;
    private Vector3[] _basePoints = new Vector3[2];
    private Vector3[] _balconyPoints = new Vector3[2];
    public List<Bridge.Side> bridgeSides = new List<Bridge.Side>();
    

    public Vector3 GetBasePoint(int i) {
        return _basePoints[i];
    }
    public void SetBasePoint(int i, Vector3 v) {
        _basePoints[i] = v;
    }
    public Vector3 GetBalconyPoint(int i) {
        return _balconyPoints[i];
    }
    public void SetBalconyPoint(int i, Vector3 v) {
        _balconyPoints[i] = v;
    }

    public Vector3 Normal {
        get {
            return Vector3.Cross(BaseDirection, Vector3.up);
        }
    }

    public Vector3 BaseDirection {
        get {
            return (GetBasePoint(1) - GetBasePoint(0)).normalized;
        }
    }

    public Vector3 BalconyDirection {
        get {
            return (GetBalconyPoint(1) - GetBalconyPoint(0)).normalized;
        }
    }

    public Vector3 BaseBalconyDir{
        get {
            return (BalconyMid - BaseMid).normalized;
        }
    }

    public float BaseBalconyDistance {
        get {
            return (BalconyMid - BaseMid).magnitude;
        }
    }

    public float BalconyWidth {
        get {
            return (GetBalconyPoint(1) - GetBalconyPoint(0)).magnitude;
        }
    }

    public float BaseWidth {
        get {
            return (GetBasePoint(1) - GetBasePoint(0)).magnitude;
        }
    }

    public Vector3 BaseMid {
        get {
            return GetBasePoint(0) + BaseDirection * BaseWidth / 2f;
        }
    }

    public Vector3 BalconyMid {
        get {
            return GetBalconyPoint(0) + BalconyDirection * BalconyWidth / 2f;
        }
    }

}
