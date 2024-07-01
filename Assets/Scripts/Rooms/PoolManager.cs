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

    public Transform parent;

    [System.Serializable]
    public class Pool {
        public string name;
        private Transform parent;
        public Transform prefab;
        public List<Transform> list = new List<Transform>();
        public Transform GetParent {
            get {
                if (parent == null) {
                    parent = new GameObject().transform;
                    parent.SetParent(Instance.transform);
                    parent.name = name;
                }

                return parent;
            }
        }
    }

    [System.Serializable]
    public class PoolRequest {
        public PoolRequest(string name) {
            this.name = name;
        }

        public string name;
        public int index;
        public List<Transform> list = new List<Transform>();

        public void Clear() {
            for (int i = index; i < list.Count; i++) {
                PoolManager.Instance.Add(list[i], name);
            }
            list.RemoveRange(index, list.Count - index);
            index = 0;
        }
    }

    private List<PoolRequest> poolRequests = new List<PoolRequest>();
    public List<Pool> poolList;

    public void ClearAll() {
        foreach (var request in poolRequests) {
            request.Clear();
        }
    }
    public void Clear(string name) {
        PoolRequest request = poolRequests.Find(x => x.name == name);
        if (request == null) {
            //Debug.LogError($"no request {name} {id}");
            return;
        }

        request.Clear();
    }

    public void DebugClear() {
        foreach (var item in poolList) {
            DestroyImmediate(item.GetParent.gameObject);
            item.list.Clear();
        }

        foreach (var item in poolRequests) {
            foreach (var i in item.list) {
                if (i && i != null) {
                    DestroyImmediate(i.gameObject);
                }
            }
            item.list.Clear();
        }

        poolRequests.Clear();
    }
    public Transform RequestObject(string _name) {
        PoolRequest request = poolRequests.Find(x => x.name == _name);
        if (request == null) {
            request = new PoolRequest(_name);
            poolRequests.Add(request);
        }

        Pool pool = poolList.Find(x => x.name == _name);
        if (pool == null) {
            Debug.Log($"error : no pool {_name}");
            return null;
        }

        Transform tr;

        if (request.index >= request.list.Count) {
            if (pool.list.Count == 0) {
                // new item to the pool
                tr = Instantiate(pool.prefab);
                pool.list.Add(tr);
            } else {
                tr = pool.list[0];
            }
            pool.list.RemoveAt(0);
            request.list.Add(tr);
        }

        tr = request.list[request.index];
        tr.gameObject.SetActive(true);
        tr.SetParent(parent);

        request.index++;
        return tr;
    }

    public void Add(Transform tr, string _name) {
        Pool item = poolList.Find(x => x.name == _name);
        if (item == null) {
            Debug.LogError("ADD ITEM no pool by name " + _name);
            return;
        }

        tr.gameObject.SetActive(false);
        //tr.SetParent(item.parent);
        item.list.Add(tr);

    }
}
