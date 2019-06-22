using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utils
{
    public static bool OnlyContainsAlphabetsAndSpaces(this string str)
    {
        return Regex.IsMatch(str, "^[a-zA-Z ]*$");
    }

    // Cache off Camera.main because it's searches through every single GameObject in the scene
    // until it finds one tagged "MainCamera", and is ridiculously expensive
    private static Camera _mainCam = null;
    public static Camera MainCam
    {
        get
        {
            if(_mainCam == null)
            {
                _mainCam = Camera.main;
            }
            return _mainCam;
        }
    }
    
    private static Plane[] _mainCamFrustrumPlanes = null;
    public static bool IsVisibleInMainCam(this Bounds bounds)
    {
        // If the bounds are within the camera's frustrum planes, it's visible
        if(_mainCamFrustrumPlanes == null)
        {
            _mainCamFrustrumPlanes = GeometryUtility.CalculateFrustumPlanes(MainCam);
        }
        return GeometryUtility.TestPlanesAABB(_mainCamFrustrumPlanes, bounds);
    }

    public static Vector2 GetScreenExtents()
    {
        return MainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }
    
    public static Vector3 GetRandomPositionOnScreen()
    {
        var screenExtents = GetScreenExtents();
        return new Vector3(Random.Range(-screenExtents.x, screenExtents.x),
                           Random.Range(-screenExtents.y, screenExtents.y),
                           0f);
    }

    public static Vector3 GetRandomUnitVector()
    {
        float angleRadians = UnityEngine.Random.Range(0, Mathf.PI * 2);
        return new Vector2(Mathf.Sin(angleRadians), Mathf.Cos(angleRadians));
    }
}
