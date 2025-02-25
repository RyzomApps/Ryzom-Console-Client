﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using API.Chat;
using TextCopy;

namespace Client.Helper
{
    /// <summary>
    /// Allows simultaneous console input and output without breaking user input
    /// (Without having this annoying behaviour : User inp[Some Console output]ut)
    /// Provide some fancy features such as formatted output, text pasting and tab-completion.
    /// By ORelio - (c) 2012-2018 - Available under the CDDL-1.0 license
    /// </summary>
    public static class ConsoleIO
    {
        private const string HistFile = "./.console_history";
        private const int HistSize = 500;

        private static IAutoComplete _autocompleteEngine;
        private static readonly LinkedList<string> AutocompleteWords = new LinkedList<string>();
        private static readonly LinkedList<string> Previous = new LinkedList<string>();
        private static readonly object IoLock = new object();
        private static bool _reading;
        private static string _buffer = "";
        private static string _buffer2 = "";
        private static int _prefixLength = 2;

        /// <summary>
        /// Determines whether to use interactive IO or basic IO.
        /// Set to true to disable interactive command prompt and use the default Console.Read|Write() methods.
        /// Color codes are printed as is when BasicIO is enabled.
        /// </summary>
        public static bool BasicIo = false;

        /// <summary>
        /// Determines whether not to print color codes in BasicIO mode.
        /// </summary>
        public static bool BasicIoNoColor = false;

        /// <summary>
        /// Determine whether WriteLineFormatted() should prepend lines with timestamps by default.
        /// </summary>
        public static bool EnableTimestamps = false;

        /// <summary>
        /// Specify a generic log line prefix for WriteLogLine()
        /// </summary>
        public static string LogPrefix = "§8[Log] ";

        /// <summary>
        /// Initialization - Loads the command line history
        /// </summary>
        static ConsoleIO()
        {
            if (!File.Exists(HistFile)) return;
            var lines = File.ReadAllLines(HistFile);
            Previous = new LinkedList<string>(lines.Skip(Math.Max(0, lines.Length - HistSize)));
        }

        /// <summary>
        /// Saves the command line history
        /// </summary>
        private static void SaveHistory()
        {
            new Thread(() => { try { File.WriteAllLines(HistFile, Previous.ToArray().Skip(Math.Max(0, Previous.Count - HistSize))); } catch { /* ignored */ } }).Start();
        }

        /// <summary>
        /// Reset the IO mechanism and clear all buffers
        /// </summary>
        public static void Reset()
        {
            lock (IoLock)
            {
                if (!_reading) return;

                ClearLineAndBuffer();
                _reading = false;
                Console.Write("\b \b");
            }
        }

        /// <summary>
        /// Set an auto-completion engine for TAB auto completion.
        /// </summary>
        /// <param name="engine">Engine implementing the IAutoComplete interface</param>
        public static void SetAutoCompleteEngine(IAutoComplete engine)
        {
            _autocompleteEngine = engine;
        }

        /// <summary>
        /// Read a password from the standard input
        /// </summary>
        public static string ReadPassword()
        {
            var password = new StringBuilder();

            ConsoleKeyInfo k;
            while ((k = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                switch (k.Key)
                {
                    case ConsoleKey.Backspace:
                        if (password.Length > 0)
                        {
                            Console.Write("\b \b");
                            password.Remove(password.Length - 1, 1);
                        }

                        break;

                    case ConsoleKey.Escape:
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Home:
                    case ConsoleKey.End:
                    case ConsoleKey.Delete:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.Tab:
                        break;

                    default:
                        if (k.KeyChar != 0)
                        {
                            Console.Write('*');
                            password.Append(k.KeyChar);
                        }

                        break;
                }
            }

            Console.WriteLine();
            return password.ToString();
        }

        /// <summary>
        /// Read a line from the standard input
        /// </summary>
        public static string ReadLine()
        {
            if (BasicIo)
            {
                return Console.ReadLine();
            }

            var k = new ConsoleKeyInfo();

            lock (IoLock)
            {
                WritePrefix();
                _reading = true;
                _buffer = "";
                _buffer2 = "";
            }

            while (k.Key != ConsoleKey.Enter)
            {
                k = Console.ReadKey(true);
                lock (IoLock)
                {
                    if (k is { Key: ConsoleKey.V, Modifiers: ConsoleModifiers.Control })
                    {
                        var clip = ReadClipboard();
                        foreach (var c in clip)
                            AddChar(c);
                    }
                    else
                    {
                        switch (k.Key)
                        {
                            case ConsoleKey.Escape:
                                ClearLineAndBuffer();
                                break;
                            case ConsoleKey.Backspace:
                                RemoveOneChar();
                                break;
                            case ConsoleKey.Enter:
                                Console.Write('\n');
                                break;
                            case ConsoleKey.LeftArrow:
                                GoLeft();
                                break;
                            case ConsoleKey.RightArrow:
                                GoRight();
                                break;
                            case ConsoleKey.Home:
                                while (!string.IsNullOrEmpty(_buffer))
                                {
                                    GoLeft();
                                }

                                break;
                            case ConsoleKey.End:
                                while (_buffer2.Length > 0)
                                {
                                    GoRight();
                                }

                                break;
                            case ConsoleKey.Delete:
                                if (_buffer2.Length > 0)
                                {
                                    GoRight();
                                    RemoveOneChar();
                                }

                                break;
                            case ConsoleKey.DownArrow:
                                if (Previous.Count > 0)
                                {
                                    ClearLineAndBuffer();
                                    _buffer = Previous.First?.Value;
                                    Previous.AddLast(_buffer);
                                    Previous.RemoveFirst();
                                    SaveHistory();
                                    Console.Write(_buffer);
                                }

                                break;
                            case ConsoleKey.UpArrow:
                                if (Previous.Count > 0)
                                {
                                    ClearLineAndBuffer();
                                    _buffer = Previous.Last?.Value;
                                    Previous.AddFirst(_buffer);
                                    Previous.RemoveLast();
                                    SaveHistory();
                                    Console.Write(_buffer);
                                }

                                break;
                            case ConsoleKey.Tab:
                                if (_buffer != null && AutocompleteWords.Count == 0 && _autocompleteEngine != null && _buffer.Length > 0)
                                    foreach (var result in _autocompleteEngine.AutoComplete(_buffer))
                                        AutocompleteWords.AddLast(result);
                                string wordAutocomplete = null;
                                if (AutocompleteWords.Count > 0)
                                {
                                    wordAutocomplete = AutocompleteWords.First?.Value;
                                    AutocompleteWords.RemoveFirst();
                                    AutocompleteWords.AddLast(wordAutocomplete);
                                }

                                if (!string.IsNullOrEmpty(wordAutocomplete) && wordAutocomplete != _buffer)
                                {
                                    while (!string.IsNullOrEmpty(_buffer) && _buffer[^1] != ' ')
                                    {
                                        RemoveOneChar();
                                    }

                                    foreach (var c in wordAutocomplete)
                                    {
                                        AddChar(c);
                                    }
                                }

                                break;
                            default:
                                if (k.KeyChar != 0)
                                    AddChar(k.KeyChar);
                                break;
                        }
                    }

                    if (k.Key != ConsoleKey.Tab)
                        AutocompleteWords.Clear();
                }
            }

            lock (IoLock)
            {
                _reading = false;
                Previous.AddLast(_buffer + _buffer2);
                SaveHistory();
                return _buffer + _buffer2;
            }
        }

        /// <summary>
        /// Debug routine: print all keys pressed in the console
        /// </summary>
        public static void DebugReadInput()
        {
            while (true)
            {
                var k = Console.ReadKey(true);
                Console.WriteLine(@"Key: {0}	Char: {1}	Modifiers: {2}", k.Key, k.KeyChar, k.Modifiers);
            }
        }

        /// <summary>
        /// Write a string to the standard output, without newline character
        /// </summary>
        public static void Write(string text)
        {
            if (!BasicIo)
            {
                lock (IoLock)
                {
                    if (_reading)
                    {
                        try
                        {
                            var buf = _buffer;
                            var buf2 = _buffer2;
                            ClearLineAndBuffer();

                            for (var i = 0; i < _prefixLength; i++)
                            {
                                if (Console.CursorLeft == 0)
                                {
                                    Console.CursorLeft = Console.BufferWidth - 1;
                                    if (Console.CursorTop > 0)
                                        Console.CursorTop--;

                                    Console.Write(" \b\n");

                                    Console.CursorLeft = Console.BufferWidth - 1;
                                    if (Console.CursorTop > 0)
                                        Console.CursorTop--;
                                }
                                else
                                {
                                    Console.Write("\b \b");
                                }
                            }

                            Console.Write(text);
                            _buffer = buf;
                            _buffer2 = buf2;

                            WritePrefix();

                            Console.Write(_buffer);

                            if (_buffer2.Length > 0)
                            {
                                Console.Write($"{_buffer2} \b");
                                for (var i = 0; i < _buffer2.Length; i++)
                                {
                                    GoBack();
                                }
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            //Console resized: Try again
                            Console.Write('\n');
                            Write(text);
                        }
                    }
                    else Console.Write(text);
                }
            }
            else Console.Write(text);
        }

        /// <summary>
        /// Output the custom prefix to the console
        /// </summary>
        public static void WritePrefix()
        {
            const string sepStart = "<";
            const string sepEnd = "> ";
            var prefix = RyzomClient.GetInstance().Channel.ToString();

            _prefixLength = sepStart.Length + prefix.Length + sepEnd.Length;

            var color = Console.ForegroundColor;

            if (!BasicIoNoColor)
                Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(sepStart);

            // get the channel color
            try
            {
                if (!BasicIoNoColor)
                    Console.ForegroundColor = ChatColor.GetConsoleColorFromMinecraftColor(ChatColor.GetMinecraftColorForChatGroupType(RyzomClient.GetInstance().Channel)[1]);
            }
            catch
            {
                // ignored
            }

            Console.Write(prefix);

            if (!BasicIoNoColor)
                Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(sepEnd);

            // reset the color
            if (!BasicIoNoColor)
                Console.ForegroundColor = color;
        }

        /// <summary>
        /// Write a string to the standard output with a trailing newline
        /// </summary>
        public static void WriteLine(string line)
        {
            Write($"{line}\n");
        }

        /// <summary>
        /// Write a single character to the standard output
        /// </summary>
        public static void Write(char c)
        {
            Write($"{c}");
        }

        /// <summary>
        /// Write a Ryzom-Like formatted string to the standard output, using §c color codes
        /// See Ryzom.gamepedia.com/Classic_server_protocol#Color_Codes for more info
        /// </summary>
        /// <param name="str">String to write</param>
        /// <param name="acceptNewLines">If false, space are printed instead of newlines</param>
        /// <param name="displayTimeStamp">
        /// If false, no timestamp is prepended.
        /// If true, "hh-mm-ss" timestamp will be prepended.
        /// If unspecified, value is retrieved from EnableTimestamps.
        /// </param>
        public static void WriteLineFormatted(string str, bool acceptNewLines = true, bool? displayTimeStamp = null)
        {
            var noColorRet = "";

            if (!string.IsNullOrEmpty(str))
            {
                if (!acceptNewLines)
                {
                    str = str.Replace('\n', ' ');
                }

                displayTimeStamp ??= EnableTimestamps;

                if (displayTimeStamp.Value)
                {
                    Write($"[{DateTime.Now:HH:mm:ss}] ");
                }

                var parts = str.Split(new[] { '§' });

                if (parts[0].Length > 0)
                {
                    if (BasicIoNoColor)
                        noColorRet += parts[0];
                    else
                        Write(parts[0]);
                }

                for (var i = 1; i < parts.Length; i++)
                {
                    if (parts[i].Length <= 0) continue;

                    if (!BasicIoNoColor)
                        Console.ForegroundColor = ChatColor.GetConsoleColorFromMinecraftColor(parts[i][0]);

                    if (parts[i].Length <= 1) continue;
                    if (BasicIoNoColor)
                        noColorRet += parts[i][1..];
                    else
                        Write(parts[i][1..]);
                }

                if (!BasicIoNoColor)
                    Console.ForegroundColor = ConsoleColor.Gray;
            }

            if (BasicIoNoColor)
                Write(noColorRet);
            else
                Write('\n');
        }

        /// <summary>
        /// Write a prefixed log line. Prefix is set in LogPrefix.
        /// </summary>
        /// <param name="text">Text of the log line</param>
        /// <param name="acceptNewLines">Allow line breaks</param>
        public static void WriteLogLine(string text, bool acceptNewLines = true)
        {
            if (!acceptNewLines)
                text = text.Replace('\n', ' ');

            WriteLineFormatted(LogPrefix + text);
        }

        #region Clipboard management

        /// <summary>
        /// Read a string from the Windows clipboard
        /// </summary>
        /// <returns>String from the Windows clipboard</returns>
        private static string ReadClipboard()
        {
            var clipdata = "";
            var staThread = new Thread(new ThreadStart(
                delegate
                {
                    clipdata = ClipboardService.GetText();
                }
            ));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipdata;
        }

        #endregion

        #region Sub functions

        /// <summary>
        /// Clear all text inside the input prompt
        /// </summary>
        private static void ClearLineAndBuffer()
        {
            while (_buffer2.Length > 0)
            {
                GoRight();
            }

            while (_buffer.Length > 0)
            {
                RemoveOneChar();
            }
        }

        /// <summary>
        /// Remove one character on the left of the cursor in input prompt
        /// </summary>
        private static void RemoveOneChar()
        {
            if (_buffer.Length <= 0) 
                return;

            try
            {
                GoBack();
                Console.Write(' ');
                GoBack();
            }
            catch (ArgumentOutOfRangeException)
            {
                /* Console was resized!? */
            }

            _buffer = _buffer[..^1];

            if (_buffer2.Length <= 0)
                return;

            Console.Write(_buffer2);
            Console.Write(' ');
            GoBack();

            for (var i = 0; i < _buffer2.Length; i++)
            {
                GoBack();
            }
        }

        /// <summary>
        /// Move the cursor one character to the left inside the console, regardless of input prompt state
        /// </summary>
        private static void GoBack()
        {
            try
            {
                if (Console.CursorLeft == 0)
                {
                    // TODO: reverse line break error here
                    Console.CursorLeft = Console.BufferWidth - 1;
                    if (Console.CursorTop > 0)
                        Console.CursorTop--;
                }
                else
                {
                    Console.CursorLeft -= 1;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                /* Console was resized!? */
            }
        }

        /// <summary>
        /// Move the cursor one character to the left in input prompt, adjusting buffers accordingly
        /// </summary>
        private static void GoLeft()
        {
            if (_buffer.Length <= 0) return;

            _buffer2 = $"{_buffer[^1]}{_buffer2}";
            _buffer = _buffer[..^1];
            GoBack();
        }

        /// <summary>
        /// Move the cursor one character to the right in input prompt, adjusting buffers accordingly
        /// </summary>
        private static void GoRight()
        {
            if (_buffer2.Length <= 0) return;

            _buffer += _buffer2[0];
            Console.Write(_buffer2[0]);
            _buffer2 = _buffer2[1..];
        }

        /// <summary>
        /// Insert a new character in the input prompt
        /// </summary>
        /// <param name="c">New character</param>
        private static void AddChar(char c)
        {
            Console.Write(c);
            _buffer += c;
            Console.Write(_buffer2);

            for (var i = 0; i < _buffer2.Length; i++)
            {
                GoBack();
            }
        }

        #endregion
    }
}