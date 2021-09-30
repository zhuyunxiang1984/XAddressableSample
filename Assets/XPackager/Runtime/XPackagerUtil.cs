using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class XPackagerUtil
{
    public static string LocalResPath
    {
        get
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return $"{Application.dataPath}/../Documents/res";
#endif
            return $"{Application.persistentDataPath}/../Documents/res";
        }
    }
    
    public struct FileListInfo
    {
        public string fileName;
        public string fileMD5;
        public long fileSize;
    }
    
    public class DataOfCheckUpdateFiles
    {
        public string RemoteUrl { get; set; }
        public string LocalPath { get; set; }
        public string StreamingAssetsPath { get; set; }
        public string FileListName { get; set; }
        public long TotalDownloadSize { get; set; }
        public List<FileListInfo> DownloadFiles { get; protected set; } = new List<FileListInfo>();
        public List<FileListInfo> RemoveFiles { get; protected set; } = new List<FileListInfo>();
        public List<FileListInfo> DecompressFiles { get; protected set; } = new List<FileListInfo>();

        public new string ToString()
        {
            string text = string.Empty;
            text += $"总共需要下载:{TotalDownloadSize}字节\n";
            if (DownloadFiles.Count > 0)
            {
                text += $"|--需要下载文件列表\n";
                foreach (var fileInfo in DownloadFiles)
                {
                    text += $"|----{fileInfo.fileName} {fileInfo.fileMD5} {fileInfo.fileSize}\n";
                }
            }
            if (RemoveFiles.Count > 0)
            {
                text += $"|--需要删除文件列表\n";
                foreach (var fileInfo in RemoveFiles)
                {
                    text += $"|----{fileInfo.fileName} {fileInfo.fileMD5} {fileInfo.fileSize}\n";
                }
            }
            if (DecompressFiles.Count > 0)
            {
                text += $"|--需要解压文件列表\n";
                foreach (var fileInfo in DecompressFiles)
                {
                    text += $"|----{fileInfo.fileName} {fileInfo.fileMD5} {fileInfo.fileSize}\n";
                }
            }

            return text;
        }
    }
    //检测文件服务器是否有更新
    public static IEnumerator CheckUpdateFiles(DataOfCheckUpdateFiles data, string remoteUrl, string localPath, string streamingAssetsPath, string fileListName)
    {
        data.RemoteUrl = remoteUrl;
        data.LocalPath = localPath;
        data.StreamingAssetsPath = streamingAssetsPath;
        data.FileListName = fileListName;
        data.TotalDownloadSize = 0;
        data.DownloadFiles.Clear();
        data.RemoveFiles.Clear();
        data.DecompressFiles.Clear();

        var remoteFileListUrl = $"{remoteUrl}/{fileListName}";;
        XDebug.Log("GGYY", $"下载远程服务器上的文件清单... {remoteFileListUrl}");
        var request = UnityWebRequest.Get(remoteFileListUrl);
        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError)
        {
            XDebug.LogError($"request error {request.error}");
            yield break;
        }
        //--
        XDebug.Log("GGYY", $"下载完成... \n{request.downloadHandler.text}");
        var remoteAAFileContent = request.downloadHandler.text;
        var remoteFileInfoMap = _ParseFileListContent(remoteAAFileContent);
        
        //--
        var localFileListPath = $"{localPath}/{fileListName}";
        var localFileInfoMap = _LoadFileList(localFileListPath);
        
        //--
        var streamingAssetsFileListPath = $"{streamingAssetsPath}/{fileListName}";
        var streamingAssetsFileInfoMap = _LoadFileList(streamingAssetsFileListPath);
        
        /*
         * 需要下载或者解压的文件列表
         * 远程有,本地没有 or 远程有,本地有,但md5不同
         * |--跟包有,md5与远程相同(解压)
         * |--跟包有,md5与远程不同(下载)
         */
        foreach (var remoteFileInfoPairs in remoteFileInfoMap)
        {
            var remoteFileName = remoteFileInfoPairs.Key;
            var remoteFileInfo = remoteFileInfoPairs.Value;
            if (localFileInfoMap.ContainsKey(remoteFileName) &&
                localFileInfoMap[remoteFileName].fileMD5 == remoteFileInfo.fileMD5)
            {
                continue;
            }
            if (streamingAssetsFileInfoMap.ContainsKey(remoteFileName) &&
                streamingAssetsFileInfoMap[remoteFileName].fileMD5 == remoteFileInfo.fileMD5)
            {
                data.DecompressFiles.Add(remoteFileInfo);
            }
            else
            {
                data.DownloadFiles.Add(remoteFileInfo);
                data.TotalDownloadSize += remoteFileInfo.fileSize;
            }
        }
        
        /*
         * 需要删除的文件列表
         * 远程没有,本地有(删除)
         */
        foreach (var localFileInfoPairs in localFileInfoMap)
        {
            var localFileName = localFileInfoPairs.Key;
            var localFileInfo = localFileInfoPairs.Value;
            if (!remoteFileInfoMap.ContainsKey(localFileName))
            {
                data.RemoveFiles.Add(localFileInfo);
            }
        }
    }
    
    /*
     * 解压文件
     * onProgress(当前解压文件名,已解压数,解压总数,进度条)
     */
    public static IEnumerator DecompressFiles(DataOfCheckUpdateFiles dataOfCheckUpdateFiles, Action<string, int, int, float> onProgress)
    {
        var remoteUrl = dataOfCheckUpdateFiles.RemoteUrl;
        var localPath = dataOfCheckUpdateFiles.LocalPath;
        var streamingAssetsPath = dataOfCheckUpdateFiles.StreamingAssetsPath;
        var fileListName = dataOfCheckUpdateFiles.FileListName;
        
        var localFileListPath = $"{localPath}/{fileListName}";
        var localFileInfoMap = _LoadFileList(localFileListPath);
        
        //解压
        foreach (var fileInfo in dataOfCheckUpdateFiles.DecompressFiles)
        {
            //TODO: 这里稍后实现
        }
        yield return null;
    }
    
    /*
     * 下载文件
     * onProgress(当前下载文件名,已下载数,下载总数,进度条,下载速度)
     */
    public static IEnumerator DownloadFiles(DataOfCheckUpdateFiles dataOfCheckUpdateFiles, Action<string, int, int, float, float> onProgress)
    {
        var remoteUrl = dataOfCheckUpdateFiles.RemoteUrl;
        var localPath = dataOfCheckUpdateFiles.LocalPath;
        var streamingAssetsPath = dataOfCheckUpdateFiles.StreamingAssetsPath;
        var fileListName = dataOfCheckUpdateFiles.FileListName;
        
        var localFileListPath = $"{localPath}/{fileListName}";
        var localFileInfoMap = _LoadFileList(localFileListPath);

        var totalNum = dataOfCheckUpdateFiles.DownloadFiles.Count;
        var downloadedNum = 0;//已下载数量
        var downloadedSize = 0f;//已下载大小
        var percentOfPerFile = 1.0f / totalNum;
        var lastRecordTime = Time.realtimeSinceStartup;//上一次记录时间;
        var lastRecordDownloadedSize = 0f;//上一次记录下次大小;
        var downloadSpeed = 0f;
        //下载
        foreach (var fileInfo in dataOfCheckUpdateFiles.DownloadFiles)
        {
            var downloadFileUrl = $"{remoteUrl}/{fileInfo.fileName}";
            var request = UnityWebRequest.Get(downloadFileUrl);
            var requestOperation = request.SendWebRequest();
            while (!requestOperation.isDone)
            {
                //计算下载进度
                var currentDownloadSize = downloadedSize + fileInfo.fileSize * requestOperation.progress;
                var progress = (downloadedNum + requestOperation.progress) * percentOfPerFile;
                //计算下载速度
                downloadSpeed = (currentDownloadSize - lastRecordDownloadedSize) /
                                    (Time.realtimeSinceStartup - lastRecordTime);
                lastRecordTime = Time.realtimeSinceStartup;
                lastRecordDownloadedSize = currentDownloadSize;
                
                onProgress?.Invoke(fileInfo.fileName, downloadedNum, totalNum, progress, downloadSpeed);
                yield return null;
                //yield return new WaitForSeconds(1f);
            }
            if (request.isHttpError || request.isNetworkError)
            {
                XDebug.LogError($"{downloadFileUrl} 下载失败!");
                yield break;
            }
            
            var localFilePath = $"{localPath}/{fileInfo.fileName}";
            var directory = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            try
            {
                //写入本地文件
                File.WriteAllBytes(localFilePath, request.downloadHandler.data);
                //更新清单文件
                if (localFileInfoMap.ContainsKey(fileInfo.fileName))
                {
                    var temp = localFileInfoMap[fileInfo.fileName];
                    temp.fileMD5 = fileInfo.fileMD5;
                    temp.fileSize = fileInfo.fileSize;
                    localFileInfoMap[fileInfo.fileName] = temp;
                }
                else
                {
                    localFileInfoMap.Add(fileInfo.fileName, fileInfo);
                }
                _SaveFileList(localFileInfoMap, localFileListPath);
                XDebug.Log("GGYY", $"下载完成 {localFilePath}");
            }
            catch (IOException e)
            {
                XDebug.LogError($"{localFilePath} 写入失败!");
                yield break;
            }
            ++downloadedNum;
            downloadedSize += fileInfo.fileSize;
            onProgress?.Invoke(fileInfo.fileName, downloadedNum, totalNum, downloadedSize * 1.0f / dataOfCheckUpdateFiles.TotalDownloadSize, downloadSpeed);
        }
    }
    
    /*
     * 删除文件
     * 
     */
    public static void RemoveFiles(DataOfCheckUpdateFiles dataOfCheckUpdateFiles)
    {
        var localPath = dataOfCheckUpdateFiles.LocalPath;
        var fileListName = dataOfCheckUpdateFiles.FileListName;
        
        var localFileListPath = $"{localPath}/{fileListName}";
        var localFileInfoMap = _LoadFileList(localFileListPath);
        
        //删除
        foreach (var fileInfo in dataOfCheckUpdateFiles.RemoveFiles)
        {
            var filePath = $"{localPath}/{fileInfo.fileName}";
            XDebug.Log("GGYY", $"删除文件 {filePath}");
            localFileInfoMap.Remove(filePath);
            File.Delete(filePath);
        }
        _SaveFileList(localFileInfoMap, localFileListPath);
    }
    
    //收集文件列表
    public static void CollectFiles(ref List<string> totalFileList, string folder, string pattern = "*.*", List<string> ignoreNames = null)
    {
        var filePaths = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);
        foreach (var filePath in filePaths)
        {
            if (totalFileList.Contains(filePath))
                continue;
            var isIgnore = false;
            if (ignoreNames != null)
            {
                foreach (var ignoreName in ignoreNames)
                {
                    if (filePath.EndsWith(ignoreName))
                    {
                        isIgnore = true;
                    }
                }
            }
            if (isIgnore)
                continue;
            totalFileList.Add(filePath);
        }
    }
    
    //根据文件列表生成MD5
    public static Dictionary<string, string> GenerateMD5Map(List<string> fileList)
    {
        var fileMD5Map = new Dictionary<string, string>();
        //生成MD5码
        foreach (var filePath in fileList)
        {
            if (fileMD5Map.ContainsKey(filePath))
            {
                var md5 = MakeMD5(filePath);
                XDebug.LogError($"already:{fileMD5Map[filePath]} new:{md5} ");
                continue;
            }
            fileMD5Map.Add(filePath, MakeMD5(filePath));
        }
        return fileMD5Map;
    }
    
    //写入清单文件
    public static void GenerateFileList(string rootPath, List<string> fileList, string output)
    {
        var fileMD5Map = GenerateMD5Map(fileList);
        var fs = new FileStream(output, FileMode.Create);
        var sw = new StreamWriter(fs);
        foreach (var filePath in fileList)
        {
            var fileName = filePath.Replace(rootPath + "\\", string.Empty);
            var fileMd5 = fileMD5Map[filePath];
            var fileSize = GetFileSize(filePath);
            sw.WriteLine($"{fileName}|{fileMd5}|{fileSize}");
        }
        sw.Close();
        fs.Close();
    }
    
    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string MakeMD5(string filePath)
    {
        try
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    /// <summary>
    /// 计算文件的Size
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        try
        {
            var fs = new FileInfo(filePath);
            return fs.Length;
        }
        catch (Exception ex)
        {
            throw new Exception("sizefile() fail, error:" + ex.Message);
        }
    }
    
    //读取清单文件
    private static Dictionary<string, FileListInfo> _LoadFileList(string filePath)
    {
        var content = string.Empty;
        if (File.Exists(filePath))
        {
            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                XDebug.LogError(e.ToString());
            }
        }
        return _ParseFileListContent(content);
    }
    
    //写入清单文件
    private static bool _SaveFileList(Dictionary<string, FileListInfo> fileInfoMap, string filePath)
    {
        var content = _MakeFileListContent(fileInfoMap);
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception e)
        {
            XDebug.LogError(e.ToString());
            return false;
        }
        return true;
    }
    
    //拼接清单文件
    private static string _MakeFileListContent(Dictionary<string, FileListInfo> fileInfoMap)
    {
        var content = string.Empty;
        var firstLine = true;
        foreach (var fileInfo in fileInfoMap.Values)
        {
            var line = $"{fileInfo.fileName}|{fileInfo.fileMD5}|{fileInfo.fileSize}";
            if (!firstLine)
            {
                content += "\n";
            }
            firstLine = false;
            content += line;
        }
        return content;
    }
    
    //解析清单文件
    private static Dictionary<string, FileListInfo> _ParseFileListContent(string content)
    {
        var fileInfoMap = new Dictionary<string, FileListInfo>();
        if (!string.IsNullOrEmpty(content))
        {
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var splits = line.Split('|');
                if (splits.Length != 3)
                    continue;
                var fileName = splits[0];
                var fileMd5 = splits[1];
                var fileSize = long.Parse(splits[2]);
                fileInfoMap.Add(fileName, new FileListInfo()
                {
                    fileName = fileName,
                    fileMD5 = fileMd5,
                    fileSize = fileSize,
                });
            }
        }
        return fileInfoMap;
    }
}