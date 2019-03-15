using System;
using System.Collections.Generic;
using System.Text;

namespace phirSOFT.SettingsService.IniSettingsService
{
    enum ReaderNodes
    {
        EmptyLine,
        SectionHeading,
        Key,
        Value,
        Comment,
    }
}
