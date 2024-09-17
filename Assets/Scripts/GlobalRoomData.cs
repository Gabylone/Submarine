using UnityEngine;

[CreateAssetMenu(fileName = "GlobalRoomData", menuName = "Data/Global Room Data")]
public class GlobalRoomData : ScriptableObject {
    private static GlobalRoomData instance;
    public static GlobalRoomData Get {
        get {
            if (instance == null) {
                instance = Resources.Load<GlobalRoomData>("Data/Global");
            }

            return instance;
        }
    }

    public int seed;
    public Vector2Int levelCount;

    [Header("SIDES")]
    public float sideWidth = 5f;
    public float sideTension = 0.5f;

    [Header("CASE")]
    public float sideHeight;
    public float caseDepth;

    [Header("SHELF")]
    public float shelfHeight;
    public Vector2 shelf_padding;
    public Vector2 shelfMult;

    [Header("PLATFORMS")]
    public float platform_TowerChance = 0.7f;

    [Header("HULL")]
    public double hullConcavity;
    public int hullScale;
    public Vector2Int hexCount;
    public Vector2 size;
    public Vector2 hexRadius;
    public Vector2Int hexSideCount;
    public Vector3 sizeMult_max;

    [Header("BALCONY")]
    public float balconyDepth;
    public float postsHeight;
    public float balconyChance;
    public float balconyHeight;
    public float rampWidth;
    public float rampHeight;

    [Header("STAIRS")]
    public float angleToStairs;
    public float angleToLadder;
    public float stairWidth;
    public float ladderWidth;

    [Header("BRIDGE")]
    public float bridgeWidth;
    public float bridgeHeight;
    public float bridgeSideBuffer;
    public float bridgeLenghtBuffer;
    public float bridgeUpDecal;
    public float decalBetweenBridges;
    public float platform_maxRadius;
    public float platform_minRadius;
    public float platform_maxCount;
    public float platform_maxDot;
    public float platform_maxDis;

    [Header("DOOR")]
    public Vector2 doorScale;

    [Header("BOOK")]
    public float bookWidth;
    public Material[] bookMats;

    [Header("EXIT")]
    public float exitChance;
}
