using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// class that instantiates Cases & Balconies
/// </summary>
public static class Case {

    public static void NewCases(Side side) {
        GlobalRoomData global = GlobalRoomData.Get;

        // set main parameters of cases
        float depth = GlobalRoomData.Get.caseDepth;
        float width = side.BaseWidth;
        float y = GlobalRoomData.Get.caseHeight / 2f;
        float height = GlobalRoomData.Get.caseHeight;
        float x = (width / 2);
        Vector3 pos = side.GetBasePoint(0) + side.BaseDirection * x + Vector3.up * y - side.Normal * depth / 2f;

        // if the side is an exit,  create 3 cases that make up a door
        if (side.exit) {
            for (int j = 0; j < 3; ++j) {
                Vector3 tPos = pos;
                float tW = width;
                float tH = height;

                // entrance side generation
                switch (j) {
                    case 0:
                    case 2:
                        // entrance sides
                        tW = (tW / 2f) - (global.doorScale.x / 2);
                        var entranceSide = side.BaseDirection * ((tW / 2f) + (global.doorScale.x / 2f));
                        tPos = tPos + (j==0 ? -entranceSide : entranceSide);
                        break;
                    case 1:
                        // entrance top
                        tW = global.doorScale.x;
                        tH = height - global.doorScale.y;
                        tPos.y += (global.doorScale.y / 2f);
                        break;
                    default:
                        break;
                }

                NewCase(tPos, side.BaseDirection, new Vector3(tW, tH, depth));
            }
        }
        // create the default case
        else {
            NewCase(pos, side.BaseDirection, new Vector3(width, height, depth));
        }
    }

    public static Transform NewCase(Vector3 pos, Vector3 dir, Vector3 scale, int shelfCount = 1) {
        Transform _case = PoolManager.Instance.RequestObject("case");
        _case.right = dir;
        _case.position = pos;
        _case.localScale = scale;

        // no shelves for now, gonna turn it into a city first

        /*for (int side = 0; side < shelfCount; side++)
            NewShelves(pos, dir, scale, side);*/

        return _case;
    }


    public static void NewShelves(Vector3 pos, Vector3 dir, Vector3 range, int side) {
        GlobalRoomData global = GlobalRoomData.Get;

        int linesPerCase = Mathf.Clamp((int)(range.y / (global.shelfHeight + global.shelf_padding.y)), 1, 100);
        for (int shelf_Y = 0; shelf_Y < linesPerCase; shelf_Y++) {
            // calculate shelf amount
            int shelvesPerLine = Mathf.Clamp((int)(range.x * global.shelfMult.x), 1, 100);

            for (int shelf_X = 0; shelf_X < shelvesPerLine; shelf_X++) {
                // set shelf width according to case width
                float shelfWidth = (range.x - (global.shelf_padding.x * shelvesPerLine) - global.shelf_padding.x) / shelvesPerLine;
                // calculate x value
                float x = global.shelf_padding.x + (global.shelf_padding.x * shelf_X) + (shelfWidth / 2f) + shelfWidth * shelf_X;
                // break when reach end of case
                if (x >= range.x)
                    break;

                // request from pool
                Transform shelf = PoolManager.Instance.RequestObject("shelf");
                // apply scale
                shelf.localScale = new Vector3(shelfWidth, global.shelfHeight, 0.05f);

                // place shelf in middle
                shelf.position = pos;
                shelf.right = dir;

                // set origin decal
                shelf.Translate(new Vector3(-range.x, -range.y, range.z) / 2f);

                // set decal in case
                float y = global.shelf_padding.y + (global.shelf_padding.y * shelf_Y) + (global.shelfHeight / 2f) + (shelf_Y * global.shelfHeight);
                Vector3 decal = new Vector3(x, y, side == 0 ? 0f : -global.caseDepth);
                shelf.Translate(decal);

                CreateBooks(shelf, shelfWidth);
            }
        }
    }
    public static void CreateBooks(Transform shelf, float shelfWidth) {
        GlobalRoomData global = GlobalRoomData.Get;

        int books_ColorIndex = 0;
        float bX = -(shelfWidth / 2);

        int break_safe = 0;
        while (bX < shelfWidth / 2f) {
            // scale
            float randomBookHeight = UnityEngine.Random.Range(0.5f, 1f);
            float bookHeight = global.shelfHeight * randomBookHeight;

            // pos
            float bY = -global.shelfHeight / 2 + bookHeight / 2;

            Transform book = PoolManager.Instance.RequestObject("book");

            float bookWidth = GlobalRoomData.Get.bookWidth;

            // rot
            book.right = shelf.right;
            book.localScale = new Vector3(bookWidth * 0.95f, bookHeight, 0.1f);

            book.position = shelf.position;
            book.Translate(Vector3.right * ((bookWidth / 2f) + bX));
            book.Translate(Vector3.up * bY);

            book.GetComponentInChildren<Renderer>().material = global.bookMats[books_ColorIndex % global.bookMats.Length];
            ++books_ColorIndex;

            ++break_safe;
            bX += bookWidth;

            if (break_safe > 100) {
                Debug.LogError("too many books");
                break;
            }

            if (bX + bookWidth >= shelfWidth / 2f) {
                break;
            }
        }
    }

    public static void NewBalcony(RoomData data, Side side) {
        Transform balcony = PoolManager.Instance.RequestObject("balcony");
        float balconyHeight = GlobalRoomData.Get.balconyHeight;

        // mesh
        MeshFilter meshFilter = balcony.GetComponentInChildren<MeshFilter>();
        Vector3[] vertices = new Vector3[8]
        {
            side.GetBalconyPoint(1) - Vector3.up * balconyHeight,
            side.GetBalconyPoint(0)- Vector3.up * balconyHeight,
            side.GetBalconyPoint(1),
            side.GetBalconyPoint(0),
            side.GetBasePoint(1),
            side.GetBasePoint(0),
            side.GetBasePoint(1) - Vector3.up * balconyHeight,
            side.GetBasePoint(0)- Vector3.up * balconyHeight,
        };
        MeshControl.Update(meshFilter, vertices);

        // RAMPS AND BRIDGES
        float rWidth = GlobalRoomData.Get.rampWidth;

        int l = data.Sides[0].Length;

        // side ramps
        for (int i = 0; i < 2; ++i) {
            float sideRampHeight = 1.3f;

            Transform sideRamp = PoolManager.Instance.RequestObject("ramp");
            Vector3 rDir = side.GetBalconyPoint(i) - side.GetBasePoint(i);
            sideRamp.forward = rDir;
            sideRamp.position = side.GetBalconyPoint(i);
            sideRamp.localScale = new Vector3(rWidth, sideRampHeight, rWidth);
        }

        if (!data.Sides[side.lvl][(side.id + 1) % l].balcony)
            NewRamp(side.GetBalconyPoint(1), side.GetBasePoint(1));

        int pi = side.id == 0 ? l - 1 : side.id - 1;
        if (!data.Sides[side.lvl][pi].balcony)
            NewRamp(side.GetBalconyPoint(0), side.GetBasePoint(0));
    }


    public static void NewRamp(Vector3 origin, Vector3 end) {
        float rWidth = GlobalRoomData.Get.rampWidth;
        float rHeight = GlobalRoomData.Get.rampHeight;



        Transform ramp = PoolManager.Instance.RequestObject("ramp");

        Vector3 dir = end - origin;
        ramp.position = origin + dir / 2f + Vector3.up * rHeight;
        ramp.LookAt(end + Vector3.up * rHeight, Vector3.up);
        ramp.localScale = new Vector3(rWidth, rWidth, dir.magnitude);
        NewPosts(origin, end);

        Transform rampCollider = PoolManager.Instance.RequestObject("ramp collider");
        rampCollider.position = origin + dir / 2f;
        rampCollider.LookAt(end , Vector3.up);
        rampCollider.localScale = new Vector3(rWidth, rHeight, dir.magnitude);

    }

    public static void NewPosts(Vector3 p1, Vector3 p2) {
        float width = GlobalRoomData.Get.rampWidth;
        float height = GlobalRoomData.Get.rampHeight;

        Vector3 dir = p2 - p1;
        int c = (int)(dir.magnitude * 2);
        for (int k = 0; k < (c + 1); k++) {
            Transform tr = PoolManager.Instance.RequestObject("ramp");
            float lerp = (float)k / c;
            Vector3 p;
            if (k == c)
                p = p2;
            else
                p = Vector3.LerpUnclamped(p1, p2, lerp);
            tr.position = p;
            /*tr.LookAt(p1, Vector3.up);*/
            tr.localScale = new Vector3(width, height, width);
        }

    }

}
