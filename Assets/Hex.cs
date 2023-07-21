using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Hex : MonoBehaviour
{
    public float radius = 70f;
    public float depth = 1.0f;
    public int caseCount = 6;

    public float shelfHeight = 0.5f;
    public int shelfPerCase = 5;
    public int shelfPerLine = 2;
    public Vector3 shelf_padding = new Vector3();

    public Vector3 books_padding;
    public int booksPerShelf = 1;

    public Material[] mats;

    public Transform parent;
    

    private void OnDrawGizmos()
    {
        if ( caseCount < 1)
        {
            return;
        }
        Vector3[] points = new Vector3[caseCount];

        Gizmos.color = Color.red;

        if ( parent == null)
        {
            parent = new GameObject().transform;
            parent.name = "Hex";
        }

        for (int a = 0; a < caseCount; a++)
        {
            float angle = 360f / caseCount;

            points[a] = transform.position +new Vector3(
                radius * (float)Mathf.Cos(a * angle * Mathf.PI / 180f),
                0f, radius * (float)Mathf.Sin(a * angle * Mathf.PI / 180f));
        }

        for (int a = 0; a < caseCount; a++)
        {
            Gizmos.DrawSphere(points[a], .1f);

        }
        for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
        {
            Gizmos.matrix = Matrix4x4.identity;

            Vector3 currPoint = points[caseIndex];
            Vector3 nextPoint = caseIndex == caseCount - 1 ? points[0] : points[caseIndex + 1];
            Vector3 mid = currPoint + (nextPoint - currPoint) / 2f;
            Gizmos.DrawLine(currPoint, nextPoint);

            Transform _case = PoolManager.Instance.RequestObject("case", GetHashCode(), parent);

            float depth = this.depth / 2f;
            _case.right = nextPoint - currPoint;
            float caseWidth = Vector3.Distance(currPoint, nextPoint);

            float height = shelf_padding.y + (shelfPerCase * (shelfHeight + shelf_padding.y));
            _case.localScale = new Vector3(caseWidth, height, this.depth);

            _case.position = mid + new Vector3(0f,height/2f,0f);
            _case.Translate(-Vector3.forward * depth);

            int colorIndex = 0;
            for (int i = 0; i < shelfPerCase; i++)
            {
                for (int a = 0; a < shelfPerLine; a++)
                {
                    Transform shelf = PoolManager.Instance.RequestObject("shelf", GetHashCode(), parent);

                    float shelfWidth = (caseWidth-(shelf_padding.x*shelfPerLine)-shelf_padding.x)/ shelfPerLine;
                    shelf.localScale = new Vector3(shelfWidth, shelfHeight, 0.05f);

                    float x = shelf_padding.x + (shelf_padding.x*a) + (shelfWidth/2f) + shelfWidth * a;
                    float y = shelf_padding.y + (shelf_padding.y*i) + (shelfHeight / 2f) + (i * shelfHeight);
                    Vector3 decal = new Vector3(x, y , 0f);
                    shelf.position = currPoint;
                    shelf.Translate(decal);
                    shelf.right = _case.transform.right;

                     for (int bI = 0; bI < booksPerShelf; bI++)
                    {
                        Transform book = PoolManager.Instance.RequestObject("book", GetHashCode(), parent);

                        // rot
                        book.right = _case.transform.right;

                        float bookWidth = shelfWidth / booksPerShelf;
                        // scale
                        float r = Random.Range(0.5f, 1f);
                        float bookHeight = shelfHeight * r;
                        book.localScale = new Vector3(bookWidth * 0.95f, bookHeight, 0.2f);

                        // pos
                        float bX = -bookWidth + (bI * (shelfWidth / booksPerShelf));
                        float bY = shelfHeight / 2f;
                        float bD = -(shelfWidth / 2) + (bookWidth/2f) + (bookWidth*bI);
                        book.position = shelf.position + shelf.right * bD + Vector3.up * (-shelfHeight/2 + bookHeight/2);

                        book.GetComponentInChildren<Renderer>().material = mats[colorIndex % mats.Length];
                        ++colorIndex;
                        
                    }
                }

                


            }

        }

        PoolManager.Instance.ResetRequest("case", GetHashCode());
        PoolManager.Instance.ResetRequest("shelf", GetHashCode());
        PoolManager.Instance.ResetRequest("book", GetHashCode());

    }
}
