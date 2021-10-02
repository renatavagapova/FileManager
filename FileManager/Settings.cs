using System;
using System.Collections.Generic;
using System.Text;

namespace FileManager
{
    class Settings
    {
            public int windowHeight { get; }
            public int windowWidth { get; }
            public int bufferHeight { get; }
            public int bufferWidth { get; }
            public int infoWindowHeight { get; }
            public int commandLineHeight { get; }
            public int pageLines { get; }
            public string settingsFile { get; }
            public int pageNumber { get; set; }
            public string lastPath { get; set; }
            public Settings()
            {
                infoWindowHeight = 5;
                commandLineHeight = 3;
                lastPath = "C:\\";
                settingsFile = "Settings.json";
                pageNumber = 1;
                pageLines = 30;
                windowHeight = pageLines + infoWindowHeight + commandLineHeight + 1;
                windowWidth = 100;
                bufferHeight = windowHeight;
                bufferWidth = 100;
            }
        
    }
}
