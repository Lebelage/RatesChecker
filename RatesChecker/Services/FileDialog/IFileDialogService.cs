using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesChecker.Services.FileDialog
{
    internal interface IFileDialogService
    {
        string OpenFile(string filter = "All files (*.*)|*.*", string defaultExt = "");
        string SaveFile(string filter = "All files (*.*)|*.*", string defaultExt = "");
    }
}
