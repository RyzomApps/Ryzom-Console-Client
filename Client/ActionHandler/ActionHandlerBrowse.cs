﻿using System;
using API;
using Client.Config;

namespace Client.ActionHandler
{
    /// <summary>
    /// Browse an URL
    /// </summary>
    public class ActionHandlerBrowse : ActionHandlerBase
    {
        public ActionHandlerBrowse(IClient client) : base(client) { }

        public override void Execute(object pCaller, string sParams)
        {
            string container = GetParam(sParams, "name");
            //InterfaceElement element = CWidgetManager.getInstance().getElementFromId(container);
            //InterfaceGroup elementGroup = element as CInterfaceGroup;

            string urls = GetParam(sParams, "url");

            bool show = GetParam(sParams, "show") != "0";

            bool localizePage = GetParam(sParams, "localize") == "1";

            // Action Handler?
            if (string.Compare(urls, 0, "ah:", 0, 3) == 0)
            {
                // Find next action handler
                int start = 3;
                int end = urls.IndexOf("&&", start);
                if (end == -1)
                {
                    end = urls.Length;
                }
                while (start < end)
                {
                    // Extract the url
                    string url = urls.Substring(start, end - start);

                    // Run an action handler
                    int index = url.IndexOfAny((Convert.ToString("&")).ToCharArray());
                    if (index == -1)
                    {
                        index = url.Length;
                    }
                    string action = url.Substring(0, index);
                    string @params = "";

                    if (index < url.Length)
                    {
                        @params = url.Substring(index + 1, url.Length - index - 1);
                    }

                    // Replace '&'
                    if (action == "command")
                    {
                        // Replace : by ' '
                        @params.Replace(':', ' ');
                    }
                    else
                    {
                        // Replace : by '|'
                        @params.Replace(':', '|');
                    }

                    //// TODO: Replace %HH encoding with ASCII values (AFTER '&' replacing, to possibly have reals '&')
                    //for (var i = 0; i < @params.Length; i++)
                    //{
                    //	if (@params[i] == '%' && i < @params.Length - 2)
                    //	{
                    //		if (StringFunctions.IsXDigit(@params[i + 1]) && StringFunctions.IsXDigit(@params[i + 2])) // FIXME: Locale dependent
                    //		{
                    //			// read value from heax decimal
                    //			byte val = 0;
                    //			@params = StringFunctions.ChangeCharacter(@params, i + 1, char.ToLower(@params[i + 1])); // FIXME: toLowerAscii
                    //			@params = StringFunctions.ChangeCharacter(@params, i + 2, char.ToLower(@params[i + 2])); // FIXME: toLowerAscii
                    //			if (char.IsDigit(@params[i + 1]))
                    //			{
                    //				val = (byte)@params[i + 1] - (byte)'0';
                    //			}
                    //			else
                    //			{
                    //				val = 10 + @params[i + 1] - 'a';
                    //			}
                    //			val *= 16;
                    //			if (char.IsDigit(@params[i + 2]))
                    //			{
                    //				val += @params[i + 2] - '0';
                    //			}
                    //			else
                    //			{
                    //				val += 10 + @params[i + 2] - 'a';
                    //			}
                    //
                    //			// write
                    //			@params = StringFunctions.ChangeCharacter(@params, i, val);
                    //			// erase heax value
                    //			@params = @params.Remove(i + 1, 2);
                    //		}
                    //	}
                    //}

                    // TODO: _client.GetInterfaceManager().parseTokens(@params);
                    // go. NB: the action handler himself may translate params from utf8
                    // TODO: _client.GetActionHandlerManager().runActionHandler(action, elementGroup, @params);

                    _client.GetLogger().Warn($"Cannot run action handler: {action}({@params}).");

                    // Next name
                    start = end + 2;
                    end = urls.IndexOf("&&", start);
                    if (end == -1)
                    {
                        end = urls.Length;
                    }
                }
            }
            else
            {
                // Get the WebTransfer
                if (_client.GetWebTransfer() != null)
                {
                    // localize if wanted
                    if (localizePage)
                    {
                        urls.Replace("_wk.", $"_{ClientConfig.LanguageCode}.");
                    }

                    // Browse the url
                    _client.GetLogger().Info($"Browsing {urls}...");
                    _client.GetWebTransfer().Get(urls);
                }
            }
        }
    }
}
