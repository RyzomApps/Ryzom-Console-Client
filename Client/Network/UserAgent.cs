///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace Client.Network
{
    public static class UserAgent
    {
        internal static string GetUserAgent()
        {
            return $"{GetUserAgentName()}/{GetUserAgentVersion()}";
        }

        private static string GetUserAgentName()
        {
            return "Ryzom";
        }

        private static string _getUserAgentVersionSUserAgent = "";

        private static string GetUserAgentVersion()
        {
            if (_getUserAgentVersionSUserAgent.Length == 0)
            {
                _getUserAgentVersionSUserAgent = $"{GetVersion()}-{GetSystem()}-{GetArchitecture()}";
            }

            return _getUserAgentVersionSUserAgent;
        }

        private static object GetArchitecture()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm",
                _ => "unknown"
            };
        }

        private static object GetSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "mac";

            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "unix" : "unknown";
        }

        private static string GetVersion()
        {
            return Program.Version;
        }
    }
}