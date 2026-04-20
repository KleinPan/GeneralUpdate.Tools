using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeneralUpdate.Tool.Avalonia;

public class ClipboardUtility
{
    private static IClipboard? _clipboard = null;

    public static async Task SetText(string content) 
    {
        //var dataObject = new DataObject();
        //dataObject.Set(DataFormats.Text, content);
        //await _clipboard?.SetDataObjectAsync(dataObject);

        var item = new DataTransferItem();
        item.Set(DataFormat.Text, content);
         var data = new DataTransfer();
        data.Add(item);
        await _clipboard?.SetDataAsync(data);
    }

    public static void CreateClipboard(Visual visual)
    {
        _clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
    }
}