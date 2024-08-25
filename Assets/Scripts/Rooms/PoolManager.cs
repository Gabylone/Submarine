using AdvancedPeopleSystem;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {
    public static PoolManager _instance;
    public static PoolManager Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<PoolManager>();
            }

            return _instance;
        }
    }

    [System.Serializable]
    public class PoolData {
        public string name;
        public Transform prefab;
    }

    [System.Serializable]
    public class PoolRequest {
        public PoolRequest(string name) {
            this.name = name;
        }

        public string name;
        public int index;
        public List<Transform> list = new List<Transform>();
        public Transform parent;

        public Transform GetParent {
            get {
                if (parent == null) {
                    parent = new GameObject().transform;
                    parent.SetParent(PoolManager.Instance.currentGroup.parent);
                    parent.name = name;
                }

                return parent;
            }
        }

    }

    [System.Serializable]
    public class PoolGroup {
        public string name;
        public Transform parent;
        public List<PoolRequest> poolRequests = new List<PoolRequest>();
    }

    public List<PoolGroup> poolGroups = new List<PoolGroup>();
    public PoolGroup currentGroup;
    public List<PoolData> poolList;

    public void NewGroup(string name) {
        var newPoolGroup = new PoolGroup();
        newPoolGroup.name = name;
        newPoolGroup.parent = new GameObject().transform;
        newPoolGroup.parent.name = name;
        currentGroup = newPoolGroup;
        poolGroups.Add(newPoolGroup);
    }

    public void PlaceGroup(Vector3 p, Vector3 f) {
        currentGroup.parent.position = p;
        currentGroup.parent.forward = f;
    }

    public Transform RequestObject(string _name, Transform _parent = null) {
        var request = currentGroup.poolRequests.Find(x => x.name == _name);
        if (request == null) {
            request = new PoolRequest(_name);
            currentGroup.poolRequests.Add(request);
        }

        PoolData pool = poolList.Find(x => x.name == _name);
        if (pool == null) {
            Debug.Log($"error : no pool {_name}");
            return null;
        }

        Transform tr;

        if (request.index >= request.list.Count) {
            tr = Instantiate(pool.prefab);
            request.list.Add(tr);
        }

        tr = request.list[request.index];
        tr.gameObject.SetActive(true);
        tr.SetParent(_parent == null ? request.GetParent : _parent);

        request.index++;
        return tr;
    }
}
