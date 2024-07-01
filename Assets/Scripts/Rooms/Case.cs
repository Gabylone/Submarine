using UnityEngine;

public static class Case {

    public static void NewCases(RoomData data, Side side) {
        GlobalRoomData global = GlobalRoomData.Get;

        //int sideCasesPerLine = (int)(dir.magnitude * global.shelfMult.x / 6f);
        int sideCasesPerLine = 0;
        int l = sideCasesPerLine + 1;
        int exitIndex = Random.Range(0, l);
        float depth = GlobalRoomData.Get.caseDepth;
        for (int i = 0; i < l; ++i) {
            // width
            float width = (side.Lenght / l) - (depth) + (depth / l);

            // pos
            float y = GlobalRoomData.Get.caseHeight / 2f;
            float x = (width / 2) + (depth * i) + width * i;
            Vector3 pos = side.Get(0) + side.Dir * x + Vector3.up * y - side.Normal * depth / 2f;

            // ENTRANCE
            float height = GlobalRoomData.Get.caseHeight;
            int e = side.exit && i == exitIndex ? 3 : 1;
            for (int j = 0; j < e; ++j) {
                Vector3 tPos = pos;
                float tW = width;
                float tH = height;

                if (e == 1)
                    goto create;

                switch (j) {
                    case 0:
                        tW = (tW / 2f) - (global.doorScale.x / 2);
                        tPos -= side.Dir * ((tW / 2f) + (global.doorScale.x / 2f));
                        break;
                    case 1:
                        //CreateExit();
                        tW = global.doorScale.x;
                        tH = height - global.doorScale.y;
                        tPos.y += (global.doorScale.y / 2f);
                        break;
                    case 2:
                        tW = (tW / 2f) - (global.doorScale.x / 2);
                        //tW = (tW - global.doorScale.x) / 2;
                        tPos += side.Dir * ((tW / 2f) + (global.doorScale.x / 2f));
                        break;
                    default:
                        break;
                }

                // scale 

                create:
                Vector3 scale = new Vector3(tW, tH, depth);
                NewCase(data, tPos, side.Dir, scale);
            }

            // creating side case
            if (i > 0) {
                float sx = (width / 2f) + (depth / 2f);
                float sideCaseWidth = 5f;
                float z = (sideCaseWidth + depth) / 2f;
                Vector3 sPos = pos - side.Dir * sx + side.Normal * z;
                Vector3 sScale = new Vector3(sideCaseWidth, height, depth);
                NewCase(data, sPos, side.Dir, sScale, 2);
            }

        }
    }

    public static Transform NewCase(RoomData data, Vector3 pos, Vector3 dir, Vector3 scale, int shelfCount = 1) {
        Transform _case = PoolManager.Instance.RequestObject("case");
        _case.right = dir;
        _case.position = pos;
        _case.localScale = scale;

        for (int side = 0; side < shelfCount; side++)
            NewShelves(data.parent, pos, dir, scale, side);

        return _case;
    }


    public static void NewShelves(Transform _parent, Vector3 pos, Vector3 dir, Vector3 range, int side) {
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

                CreateBooks(_parent, shelf, shelfWidth);
            }
        }
    }
    public static void CreateBooks(Transform _parent, Transform shelf, float shelfWidth) {
        GlobalRoomData global = GlobalRoomData.Get;

        int books_ColorIndex = 0;
        float bX = -(shelfWidth / 2);

        int booksPerShelf = (int)(shelfWidth / 5);
        int br = 0;
        while (bX < shelfWidth / 2f) {
            // scale
            float r = UnityEngine.Random.Range(0.5f, 1f);
            float bookHeight = global.shelfHeight * r;

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

            ++br;
            bX += bookWidth;

            if (br > 100) {
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
            side.GetInner(1) - Vector3.up * balconyHeight,
            side.GetInner(0)- Vector3.up * balconyHeight,
            side.GetInner(1),
            side.GetInner(0),
            side.Get(1),
            side.Get(0),
            side.Get(1) - Vector3.up * balconyHeight,
            side.Get(0)- Vector3.up * balconyHeight,
        };
        MeshControl.Update(meshFilter, vertices);

        // RAMPS AND BRIDGES
        float bridgeWidth = 2f;

        float rWidth = GlobalRoomData.Get.rampWidth;
        float rHeight = GlobalRoomData.Get.rampHeight;
        float range = (side.GetInner(1) - side.GetInner(0)).magnitude;

        /*Vector3 origin = side.GetInner(0);
        Vector3 end = side.GetInner(1);
        Vector3 bridge_Left;
        Vector3 bridge_Right;
        for (int i = 0; i < 1; i++)
        {
            float x = Random.Range(bridgeWidth, range - bridgeWidth);
            bridge_Left = origin + side.Dir * (x - bridgeWidth / 2f);
            bridge_Right = origin + side.Dir * (x + bridgeWidth / 2f);
            
            Side.BridgePart part = new Side.BridgePart (bridge_Left, bridge_Right, side.id);
            side.bridgeParts = new List<Side.BridgePart>() { part };


            NewRamp(data.parent, origin, bridge_Left);
            origin = bridge_Right;
        }
        NewRamp(data.parent, origin, side.GetInner(1));*/

        int l = data.Sides[0].Length;

        // side ramps
        for (int i = 0; i < 2; ++i) {
            float sideRampHeight = 1.3f;

            Transform sideRamp = PoolManager.Instance.RequestObject("ramp");
            Vector3 rDir = side.GetInner(i) - side.Get(i);
            sideRamp.forward = rDir;
            sideRamp.position = side.GetInner(i);
            sideRamp.localScale = new Vector3(rWidth, sideRampHeight, rWidth);
        }

        if (!data.Sides[side.lvl][(side.id + 1) % l].balcony)
            NewRamp(side.GetInner(1), side.Get(1));

        int pi = side.id == 0 ? l - 1 : side.id - 1;
        if (!data.Sides[side.lvl][pi].balcony)
            NewRamp(side.GetInner(0), side.Get(0));
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
