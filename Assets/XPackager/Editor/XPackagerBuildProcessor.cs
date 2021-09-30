using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class XPackagerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder
    {
        //因为Addressable的内建编译处理优先级为1,所以我们优先级设置为2
        get { return 2; }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        //清除Addressable的东西
        CleanTemporaryPlayerBuildData();

        AddressableBuilder.GenerateFileListForMenu();
        AddressableBuilder.UploadToRemoteForMenu();
    }
    
    void CleanTemporaryPlayerBuildData()
    {
        if (Directory.Exists(Addressables.PlayerBuildDataPath))
        {
            Directory.Move(Addressables.PlayerBuildDataPath, Addressables.BuildPath);
        }
    }
    
    public void OnPostprocessBuild(BuildReport report)
    {
        
    }
}
