using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GeneralUpdate.Common.Compress;
using GeneralUpdate.Common.HashAlgorithms;
using GeneralUpdate.Common.Models;
using GeneralUpdate.Differential;
using GeneralUpdate.Tool.Avalonia.Helpers;
using GeneralUpdate.Tool.Avalonia.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUpdate.Tool.Avalonia.ViewModels;

public partial class MakePacketVM : ObservableObject
{
    [ObservableProperty]
    private PacketConfigVM? _configVM;

    public ObservableCollection<AppTypeModel> AppTypes { get; set; } = new();

    public ObservableCollection<FormatModel> Formats { get; set; } =
    [
        new FormatModel { DisplayName = ".zip", Type = 1, Value = ".zip" }
    ];

    public ObservableCollection<EncodingModel> Encodings { get; set; } =
    [
        new EncodingModel { DisplayName = "Default", Value = Encoding.Default, Type = 1 },
        new EncodingModel { DisplayName = "UTF-8", Value = Encoding.UTF8, Type = 2 },
        new EncodingModel { DisplayName = "UTF-7", Value = Encoding.UTF7, Type = 3 },
        new EncodingModel { DisplayName = "Unicode", Value = Encoding.GetEncoding("Unicode"), Type = 4 },
        new EncodingModel { DisplayName = "UTF-32", Value = Encoding.UTF32, Type = 5 },
        new EncodingModel { DisplayName = "BigEndianUnicode", Value = Encoding.BigEndianUnicode, Type = 6 },
        new EncodingModel { DisplayName = "Latin1", Value = Encoding.GetEncoding("Latin1"), Type = 7 },
        new EncodingModel { DisplayName = "ASCII", Value = Encoding.ASCII, Type = 8 }
    ];

    public ObservableCollection<PlatformModel> Platforms { get; set; } =
    [
        new PlatformModel { DisplayName = "Windows", Value = 1 },
        new PlatformModel { DisplayName = "Linux", Value = 2 }
    ];

    [RelayCommand]
    private void LoadedAction()
    {
        AppTypes.Clear();
        AppTypes.Add(new AppTypeModel { DisplayName = "ClientApp", Value = 1 });
        AppTypes.Add(new AppTypeModel { DisplayName = "UpgradeApp", Value = 2 });

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "PacketConfig.json");
            if (File.Exists(path))
            {
                var model = IOHelper.Instance.ReadContentFromLocal<PacketConfigM>(path);

                ConfigVM = model.ToVM();
                ConfigVM!.Format = Formats[model.FormatIndex];
                ConfigVM!.Encoding = Encodings[model.EncodingIndex];
                ConfigVM!.Platform = Platforms[0];
            }
            else
            {
                ConfigVM = new PacketConfigVM();
            }
        }
        catch (Exception ex)
        {
            App.NotifyHelper.ShowErrorMessage($"Load fail => {ex}");
        }
    }

    [RelayCommand]
    private void ResetAction()
    {
        //ConfigVM!.Name = GenerateFileName("1.0.0.0");
        ConfigVM!.Version = "1.0.0.0";
        ConfigVM.ReleaseDirectory = GetPlatformSpecificPath();
        ConfigVM.AppDirectory = GetPlatformSpecificPath();
        ConfigVM.PatchDirectory = GetPlatformSpecificPath();
        ConfigVM.Encoding = Encodings.First();
        ConfigVM.Format = Formats.First();
    }

    /// <summary>Choose a path</summary>
    /// <param name="value"></param>
    [RelayCommand]
    private async Task SelectFolderAction(string value)
    {
        try
        {
            var folders = await Storage.Instance.SelectFolderDialog();
            if (!folders.Any()) return;

            var folder = folders.First();
            switch (value)
            {
                case "App":
                    ConfigVM.AppDirectory = folder.Path.LocalPath;
                    break;

                case "Release":
                    ConfigVM.ReleaseDirectory = folder!.Path.LocalPath;
                    break;

                case "Patch":
                    ConfigVM.PatchDirectory = folder!.Path.LocalPath;
                    break;
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.Message);
        }
    }

    [RelayCommand]
    private async Task BuildPacketAction()
    {
        try
        {
            //生成补丁文件[不能包含文件名相同但扩展名不同的文件]。

            if (ConfigVM.IsPatch)
            {
                await DifferentialCore.Instance.Clean(ConfigVM!.AppDirectory, ConfigVM.ReleaseDirectory, ConfigVM.PatchDirectory);
            }
            else
            {
                var ignoreFiles = new[]
                {
                    "*.pdb",
                    "*.log",
                    "*.tmp",
                    "*.id"
                };

                var ignoreDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Logs",
                    "obj",
                    ".git"
                };

                CopyDirectory(
                     ConfigVM.ReleaseDirectory,
                    ConfigVM.PatchDirectory,
                    ignoreFiles,
                    ignoreDirs
                );
            }

            var directoryInfo = new DirectoryInfo(ConfigVM.PatchDirectory);
            var parentDirectory = directoryInfo.Parent!.FullName;
            var operationType = ConfigVM.Format.Value;
            var encoding = ConfigVM.Encoding.Value;

            var buildTime = GenerateDateTime();
            var packName = $"packet_{buildTime}_V{ConfigVM.Version}"; ;
            var packPath = Path.Combine(parentDirectory, packName + ConfigVM.Format.Value);
            CompressProvider.Compress(operationType, ConfigVM.PatchDirectory, packPath, false, encoding);

            if (Directory.Exists(ConfigVM.PatchDirectory))
                DeleteDirectoryRecursively(ConfigVM.PatchDirectory);

            var packetInfo = new FileInfo(Path.Combine(parentDirectory, $"{packName}{ConfigVM.Format.Value}"));
            if (packetInfo.Exists)
            {
                ConfigVM.Path = packetInfo.FullName;

                App.NotifyHelper.ShowInfoMessage("Build success");
                var versionInfo = new FileInfo(Path.Combine(parentDirectory, $"VersionInfo_V{ConfigVM.Version}.json"));

                VersionInfoM versionInfoM = new VersionInfoM();
                versionInfoM.PacketName = packName;
                versionInfoM.Format = ConfigVM.Format.Value;
                Sha256HashAlgorithm hashAlgorithm = new();
                versionInfoM.Hash = hashAlgorithm.ComputeHash(packPath);

                versionInfoM.Version = ConfigVM.Version;
                versionInfoM.BuildTime = buildTime;

                IOHelper.Instance.WriteContentTolocal(versionInfoM, versionInfo.FullName);

                ConfigVM.Name = packName;
            }
            else
            {
                App.NotifyHelper.ShowErrorMessage("Build fail");
            }

            var model = ConfigVM.ToModel();
            model.PlatformIndex = Platforms.IndexOf(ConfigVM.Platform);
            model.FormatIndex = Formats.IndexOf(ConfigVM.Format);
            model.EncodingIndex = Encodings.IndexOf(ConfigVM.Encoding);

            IOHelper.Instance.WriteContentTolocal(model, Path.Combine(AppContext.BaseDirectory, "PacketConfig.json"));
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.Message);

            App.NotifyHelper.ShowErrorMessage(e.Message);
        }
    }

    private string GetPlatformSpecificPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows-specific path, defaulting to C: drive
            return @"C:\";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux-specific path, defaulting to /home/user
            return "/home";
        }

        throw new PlatformNotSupportedException("Unsupported OS");
    }

    private string GenerateDateTime()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMddHH_mmssfff");
        return timestamp;
        //return $"packet_{timestamp}_{version}";
    }

    private void DeleteDirectoryRecursively(string targetDir)
    {
        foreach (var file in Directory.GetFiles(targetDir))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(targetDir))
        {
            DeleteDirectoryRecursively(dir);
        }
        Directory.Delete(targetDir, false);
    }

    static bool WildcardMatch(string text, string pattern)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            text,
            "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    static void CopyDirectory(
     string sourceDir,
     string destinationDir,
     IEnumerable<string>? ignoreFilePatterns = null,
     ISet<string>? ignoreDirectoryNames = null)
    {
        ignoreFilePatterns ??= Array.Empty<string>();
        ignoreDirectoryNames ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var sourceInfo = new DirectoryInfo(sourceDir);

        // 目录名全匹配过滤
        if (ignoreDirectoryNames.Contains(sourceInfo.Name))
            return;

        Directory.CreateDirectory(destinationDir);

        // 复制文件（支持通配符）
        foreach (var file in sourceInfo.GetFiles())
        {
            bool ignored = ignoreFilePatterns.Any(p => WildcardMatch(file.Name, p));
            if (ignored)
                continue;

            string destPath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(destPath, true);
        }

        // 递归子目录
        foreach (var subdir in sourceInfo.GetDirectories())
        {
            if (ignoreDirectoryNames.Contains(subdir.Name))
                continue;

            CopyDirectory(
                subdir.FullName,
                Path.Combine(destinationDir, subdir.Name),
                ignoreFilePatterns,
                ignoreDirectoryNames);
        }
    }
}