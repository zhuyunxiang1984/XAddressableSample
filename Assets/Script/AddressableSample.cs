using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using TMPro;

public class AddressableSample : MonoBehaviour
{
    //远程文件服务器
    public TMP_InputField inputRemoteUrl;
    //更新进度条
    public GameObject objDownloadProgressBar;
    public Image imgDownloadProgressBar;
    public TextMeshProUGUI txtDownloadProgressBar;
    public TextMeshProUGUI txtDownloadDesc;
    //检查更新按钮
    public Button btnUpdate;

    public TMP_InputField inputAssetName;
    public Toggle toggleAttachUI;
    public Button btnGenerate;
    
    public Transform worldOrigin;
    public RectTransform uiAssetOrigin;
    
    private List<GameObject> _assetObjs = new List<GameObject>();

    public string RemoteUrl
    {
        get { return inputRemoteUrl.text; }
    }

    private void Awake()
    {
    }

    private IEnumerator Start()
    {
        objDownloadProgressBar.gameObject.SetActive(false);
        btnUpdate.onClick.AddListener(UpdateFiles);
        btnGenerate.onClick.AddListener(GenerateAssetObj);

        XAddressableHelper.Initialize();
        yield return _UpdateFilesAsync();
    }
    // Update is called once per frame
    private void Update()
    {
        
    }
    
    //
    public void UpdateFiles()
    {
        ClearAssetObjs();
        StartCoroutine(_UpdateFilesAsync());
    }

    //资源热更新流程
    private IEnumerator _UpdateFilesAsync()
    {
        var fileListName = "gamefilelist.txt";
        var dataOfCheckUpdateFiles = new XPackagerUtil.DataOfCheckUpdateFiles();
        yield return XPackagerUtil.CheckUpdateFiles(
            dataOfCheckUpdateFiles, 
            RemoteUrl, 
            XPackagerUtil.LocalResPath, 
            Application.streamingAssetsPath, 
            fileListName);

        if (dataOfCheckUpdateFiles.DecompressFiles.Count > 0)
        {
            objDownloadProgressBar.gameObject.SetActive(true);
            imgDownloadProgressBar.fillAmount = 1f;
            txtDownloadProgressBar.text = string.Empty;
            txtDownloadDesc.text = string.Empty;
            yield return XPackagerUtil.DecompressFiles(dataOfCheckUpdateFiles,
                (fileName, cur, max, progress) =>
                {
                    imgDownloadProgressBar.fillAmount = progress;
                    txtDownloadProgressBar.text = $"{cur}/{max}";
                    txtDownloadDesc.text = $"正在解压{fileName}...";
                });
            txtDownloadDesc.text = string.Empty;
        }

        if (dataOfCheckUpdateFiles.DownloadFiles.Count > 0)
        {
            objDownloadProgressBar.gameObject.SetActive(true);
            imgDownloadProgressBar.fillAmount = 1f;
            txtDownloadProgressBar.text = string.Empty;
            txtDownloadDesc.text = string.Empty;
            yield return XPackagerUtil.DownloadFiles(dataOfCheckUpdateFiles,
                (fileName, cur, max, progress, speed) =>
                {
                    imgDownloadProgressBar.fillAmount = progress;
                    txtDownloadProgressBar.text = $"{cur}/{max}";
                    txtDownloadDesc.text = $"正在下载{fileName}... 下载速度{speed/1024}KB/s";
                });
            txtDownloadDesc.text = string.Empty;
        }

        if (dataOfCheckUpdateFiles.RemoveFiles.Count > 0)
        {
            XPackagerUtil.RemoveFiles(dataOfCheckUpdateFiles);
        }
        yield return XAddressableHelper.ReloadCatalogAsync();
    }
    
    //创建资源实例
    public void GenerateAssetObj()
    {
        ClearAssetObjs();
        
        var assetName = inputAssetName.text;
        if (string.IsNullOrEmpty(assetName))
            return;
        Addressables.InstantiateAsync(assetName).Completed += (loadHandle) =>
        {
            var obj = loadHandle.Result;
            if (toggleAttachUI)
            {
                obj.transform.SetParent(uiAssetOrigin, false);
            }
            else
            {
                obj.transform.SetParent(worldOrigin, false);
            }
            _assetObjs.Add(obj);
        };
    }
    //清空实例
    public void ClearAssetObjs()
    {
        foreach (var assetObj in _assetObjs)
        {
            Addressables.ReleaseInstance(assetObj);
        }
        _assetObjs.Clear();
    }
    
}
