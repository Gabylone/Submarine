using AdvancedPeopleSystem;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {

    // prefab holder
    [System.Serializable]
    public class PoolData {
        public string name;
        public Transform prefab;
    }

    // request for specific group ( platforms, bridges, balconies, walls etc... )
    [System.Serializable]
    public class PoolRequest {
        public PoolRequest(string name) {
            this.name = name;
        }

        public string name;
        public int count;
        public List<Transform> list = new List<Transform>();
        public List<int> id = new List<int>();
        public Transform parent;

        public Transform GetParent {
            get {
                if (parent == null) {
                    parent = new GameObject().transform;
                    parent.SetParent(PoolManager.Instance.currentRoom.parent);
                    parent.name = name;
                }

                return parent;
            }
        }

    }

    [System.Serializable]
    public class PoolRoom {
        public string name;
        public Transform parent;
        public List<PoolRequest> poolRequests = new List<PoolRequest>();
    }

    public List<PoolRoom> poolRooms = new List<PoolRoom>();
    public PoolRoom currentRoom;
    public List<PoolData> prefabList;

    public void NewRoom(string name) {
        var poolRoom = new PoolRoom();
        poolRoom.name = name;
        poolRoom.parent = new GameObject().transform;
        poolRoom.parent.name = name;
        currentRoom = poolRoom;
        poolRooms.Add(poolRoom);
    }

    public void PlaceRoom(Vector3 p, Vector3 f) {
        currentRoom.parent.position = p;
        currentRoom.parent.forward = f;
    }

    public Transform NewObject(string prefabName, string groupName, int index ) {
        return NewObject(prefabName, groupName, index);
    }

    public void ClearGroup(string groupName) {
        var request = currentRoom.poolRequests.Find(x => x.name == groupName);
        if (request == null) {
            Debug.LogError($"No Pool Group Named : {groupName}");
            return;
        }

        foreach ( var tr in request.list ) {
            tr.gameObject.SetActive(false);
        }

        request.count = 0;
    }

    public Transform NewObject(string prefabName, string groupName, Transform _parent = null, int index = -1) {
        var request = currentRoom.poolRequests.Find(x => x.name == groupName);
        if (request == null) {
            request = new PoolRequest(groupName);
            currentRoom.poolRequests.Add(request);
        }

        PoolData pool = prefabList.Find(x => x.name == prefabName);
        if (pool == null) {
            Debug.Log($"error : no pool {prefabName}");
            return null;
        }

        Transform tr;

        if (request.count >= request.list.Count) {
            tr = Instantiate(pool.prefab);
            request.list.Add(tr);
        }

        tr = request.list[request.count];
        tr.gameObject.SetActive(true);
        tr.SetParent(_parent == null ? request.GetParent : _parent);

        request.count++;
        return tr;
    }

    public static PoolManager _instance;
    public static PoolManager Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<PoolManager>();
            }

            return _instance;
        }
    }
}
