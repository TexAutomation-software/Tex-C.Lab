using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRA.Services;

public class AppSettingsService
{
    public string OutputFolder { get; set; }
        = @"C:\PCRA\Output";
}
