using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class XAddressableHelper
{
    public static void Initialize()
    {
        Addressables.InternalIdTransformFunc = _InternalIdTransformFunc;
    }
    public static IEnumerator ReloadCatalogAsync()
    {
        Addressables.ClearResourceLocators();
        yield return Addressables.LoadContentCatalogAsync($"{_AAPath}/catalog.json");
    }
    private static string _InternalIdTransformFunc(IResourceLocation location)
    {
        XDebug.Log("GGYY", $"原始路径 = {location.InternalId}");
        var extendName = Path.GetExtension(location.InternalId);
        //XDebug.Log("GGYY", extendName);
        if (extendName == ".json")
        {
            var result = $"{_AAPath}/{Path.GetFileName(location.InternalId)}";
            XDebug.Log("GGYY", $"最终路径 = {result}");
            return result;
        }

        if (extendName == ".bundle")
        {
            var result = $"{_AABundlePath}/{Path.GetFileName(location.InternalId)}";
            XDebug.Log("GGYY", $"最终路径 = {result}");
            return result;
        }

        XDebug.Log("GGYY", $"最终路径 = {location.InternalId}");
        return location.InternalId;
    }

    private static string _AAPath
    {
        get { return XPackagerUtil.LocalResPath; }
    }

    private static string _AABundlePath
    {
        get
        {
#if UNITY_ANDROID
            return $"{_AAPath}/Android";
#endif
#if UNITY_IOS
            return $"{_AAPath}/iOS";
#endif
            return $"{_AAPath}/StandaloneWindows64";
        }
    }

}
