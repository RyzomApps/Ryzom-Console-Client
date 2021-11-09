﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using API.Logger;
using API.Plugins;

namespace API
{
    public interface IClient
    {
        ILogger GetLogger();

        IPluginManager GetPluginManager();
    }
}