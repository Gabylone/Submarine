using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// contains tools for actual room generation ( geometry etc... )
/// � faire : bien s�parer la data de la g�n�ration
/// la g�n�ration prend la data et calcule toutes ses variables
/// </summary>
public class RoomGenerator {
    public static RoomData _handledData;
    public static Transform room_Parent;
}
