using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

public class AddressableBuilder
{
    [MenuItem("AddressableHelper/打包一条龙")]
    public static void BuildAASimpleForMenu()
    {
        BuildAAForMenu();
        GenerateFileListForMenu();
        UploadToRemoteForMenu();
    }
    
    [MenuItem("AddressableHelper/打包资源")]
    public static void BuildAAForMenu()
    {
        AddressablesPlayerBuildResult result = null;
        AddressableAssetSettings.BuildPlayerContent(out result);
        foreach (var filePath in result.FileRegistry.GetFilePaths())
        {
            XDebug.Log("GGYY", $"filePath = {filePath}");
        }
        XDebug.Log("GGYY", $"打包资源完成 耗时{result.Duration}s");
    }

    [MenuItem("AddressableHelper/创建清单")]
    public static void GenerateFileListForMenu()
    {
        //将文件复制到streamingassets目录
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.Delete(Application.streamingAssetsPath, true);
        }
        Directory.CreateDirectory(Application.streamingAssetsPath);
        //创建文件列表
        var localAAPath = $"{Application.streamingAssetsPath}/res";
        FileUtil.CopyFileOrDirectory(Addressables.BuildPath,  localAAPath);
        
        var totalFilePaths = new List<string>();
        XPackagerUtil.CollectFiles(ref totalFilePaths, localAAPath, "*.json");
        XPackagerUtil.CollectFiles(ref totalFilePaths, localAAPath, "*.xml");
        XPackagerUtil.CollectFiles(ref totalFilePaths, localAAPath, "*.bundle");
        //写入清单文件
        XPackagerUtil.GenerateFileList(localAAPath, totalFilePaths, $"{localAAPath}/gamefilelist.txt");

        XDebug.Log("GGYY", "创建清单完成");
    }

    [MenuItem("AddressableHelper/复制到模拟文件服务器")]
    public static void UploadToRemoteForMenu()
    {
        var localAAPath = $"{Application.streamingAssetsPath}/res";
        
        var dstDirectory = $"{Application.dataPath}/../SimulateFileService";
        if (Directory.Exists(dstDirectory))
        {
            Directory.Delete(dstDirectory, true);
        }
        FileUtil.CopyFileOrDirectory(localAAPath, dstDirectory);
        XDebug.Log("GGYY", "复制完成");
    }
}
