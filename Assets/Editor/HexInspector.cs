using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

[CustomEditor(typeof(Hex))]
public class HexInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Hex hex = (Hex)target;

        if (GUILayout.Button("Change Books Colors"))
        {
            foreach (var item in hex.mats)
            {
                item.color = Color.Lerp(Random.ColorHSV(), Color.gray, 0.5f);
            }
        }
    
        if (GUILayout.Button("Clear"))
        {
            hex.booksPerShelf = 0;
            hex.caseCount = 0;
            hex.shelfPerCase = 0;

            PoolManager poolManager = FindObjectOfType<PoolManager>();
            foreach (var item in poolManager.poolList)
            {
                foreach (var i in item.list)
                {
                    if (i && i != null)
                    {
                        DestroyImmediate(i.gameObject);
                    }
                }
                item.list.Clear();
            }

            foreach (var item in poolManager.poolRequests)
            {
                foreach (var i in item.list)
                {
                    if (i && i != null)
                    {
                        DestroyImmediate(i.gameObject);
                    }
                }
                item.list.Clear();
            }

            poolManager.poolRequests.Clear();
        }

        base.OnInspectorGUI();

    }
}
