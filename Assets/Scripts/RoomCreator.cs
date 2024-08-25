using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// contains tools for actual room generation ( geometry etc... )
/// à faire : bien séparer la data de la génération
/// la génération prend la data et calcule toutes ses variables
/// </summary>
public class RoomGenerator {
    public static RoomData _handledData;
    public static Transform room_Parent;
}
