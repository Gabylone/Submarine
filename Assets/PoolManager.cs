using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Timeline;

public class PoolManager : MonoBehaviour
{
    public static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PoolManager>();
            }

            return _instance;
        }
    }

    [System.Serializable]
    public class Pool
    {
        public string name;
        public Transform parent;
        public Transform prefab;
        public List<Transform> list = new List<Transform>();
    }

    public static int requestCount;
    [System.Serializable]
    public class PoolRequest
    {
        public static int scale;

        public PoolRequest(string name, int id, Transform parent)
        {
            this.name = name;
            this.id = id;
            this.parent = parent;
        }

        public string name;
        public Transform parent;
        public int id;
        public int index;
        public List<Transform> list = new List<Transform>();
    }

    public List<PoolRequest> poolRequests = new List<PoolRequest>();
    public List<Pool> poolList;

    public void ResetRequest(string name, int id)
    {
        PoolRequest request = poolRequests.Find(x => x.id == id && x.name == name);
        if (request == null)
        {
            Debug.LogError($"no request {name} {id}");
            return;
        }

        for (int i = request.index; i < request.list.Count; i++)
        {
            Add(request.list[i], request.name);
            request.list.RemoveAt(i);
        }

        request.index = 0;
    }

    public Transform RequestObject(string _name, int id, Transform parent)
    {
        PoolRequest request = poolRequests.Find(x => x.id == id && x.name == _name);
        if ( request == null)
        {
            request = new PoolRequest(_name, id , parent);
            poolRequests.Add(request);
            Debug.Log($"new request {_name} {id}");
        }
        else
        {
        }

        Pool pool = poolList.Find(x => x.name == _name);
        if (pool == null)
        {
            Debug.Log($"error : no pool {_name}");
            return null;
        }

        Transform tr;

        if ( request.index >= request.list.Count)
        {
            if (pool.list.Count == 0)
            {
                // new item to the pool
                tr = Instantiate(pool.prefab, pool.parent);
                pool.list.Add(tr);
            }
            else
            {
                tr = pool.list[0];
            }
            pool.list.RemoveAt(0);
            request.list.Add(tr);
        }

        tr = request.list[request.index]; 
        tr.gameObject.SetActive(true);
        tr.SetParent(request.parent);

        request.index++;
        return tr;
    }

    public void Add(Transform tr, string _name)
    {
        Pool item = poolList.Find(x => x.name == _name);
        if (item == null)
        {
            Debug.LogError("ADD ITEM no pool by name " + _name);
            return;
        }

        tr.gameObject.SetActive(false);
        tr.SetParent(item.parent);
        item.list.Add(tr);

    }
}
