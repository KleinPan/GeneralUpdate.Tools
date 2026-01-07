using CommunityToolkit.Mvvm.ComponentModel;

using System;

namespace GeneralUpdate.Tool.Avalonia.ViewModels;

public partial class OSSConfigVM : ObservableObject
{
    /// <summary>为了PubTime</summary>
    [ObservableProperty]
    private DateTime _date;

    /// <summary>为了PubTime</summary>
    [ObservableProperty]
    private TimeSpan _time;

    public DateTime PubTime
    {
        get => Date + Time;
    }

    [ObservableProperty]
    private string _packetName;

    [ObservableProperty]
    private string _hash;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private string _url;

    /// <summary>未使用</summary>
    [ObservableProperty]
    private string _jsonContent;
}