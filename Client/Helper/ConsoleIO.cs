///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
        private static IAutoComplete _autocompleteEngine;
        private static readonly LinkedList<string> AutocompleteWords = new LinkedList<string>();
        private static readonly LinkedList<string> Previous = new LinkedList<string>();
        private static readonly object IoLock = new object();
        private static bool _reading;
        private static string _buffer = "";
        private static string _buffer2 = "";

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
        /// Set an auto-completion engine for TAB autocompletion.
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
            StringBuilder password = new StringBuilder();

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
                var prefix = RyzomClient.GetInstance().Channel.ToString()[0];
                Console.Write(prefix);
                _reading = true;
                _buffer = "";
                _buffer2 = "";
            }

            while (k.Key != ConsoleKey.Enter)
            {
                k = Console.ReadKey(true);
                lock (IoLock)
                {
                    if (k.Key == ConsoleKey.V && k.Modifiers == ConsoleModifiers.Control)
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
                            if (Console.CursorLeft == 0)
                            {
                                Console.CursorLeft = Console.BufferWidth - 1;
                                Console.CursorTop--;
                                Console.Write(@" ");
                                Console.CursorLeft = Console.BufferWidth - 1;
                                Console.CursorTop--;
                            }
                            else Console.Write("\b \b");

                            Console.Write(text);
                            _buffer = buf;
                            _buffer2 = buf2;

                            var prefix = RyzomClient.GetInstance().Channel.ToString()[0];

                            Console.Write(prefix + _buffer);
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
        /// Write a string to the standard output with a trailing newline
        /// </summary>
        public static void WriteLine(string line)
        {
            Write(line + '\n');
        }

        /// <summary>
        /// Write a single character to the standard output
        /// </summary>
        public static void Write(char c)
        {
            Write("" + c);
        }

        /// <summary>
        /// Write a Ryzom-Like formatted string to the standard output, using §c color codes
        /// See Ryzom.gamepedia.com/Classic_server_protocol#Color_Codes for more info
        /// </summary>
        /// <param name="str">String to write</param>
        /// <param name="acceptnewlines">If false, space are printed instead of newlines</param>
        /// <param name="displayTimestamp">
        /// If false, no timestamp is prepended.
        /// If true, "hh-mm-ss" timestamp will be prepended.
        /// If unspecified, value is retrieved from EnableTimestamps.
        /// </param>
        public static void WriteLineFormatted(string str, bool acceptnewlines = true, bool? displayTimestamp = null)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (!acceptnewlines)
                {
                    str = str.Replace('\n', ' ');
                }

                displayTimestamp ??= EnableTimestamps;

                if (displayTimestamp.Value)
                {
                    Write($"[{DateTime.Now:HH:mm:ss}] ");
                }

                if (BasicIo)
                {
                    if (BasicIoNoColor)
                    {
                        // TODO: Verbatim
                        //str = Misc.GetVerbatim(str);
                    }

                    Console.WriteLine(str);
                    return;
                }

                var parts = str.Split(new[] { '§' });

                if (parts[0].Length > 0)
                {
                    Write(parts[0]);
                }

                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0)
                    {
                        switch (parts[i][0])
                        {
                            case '0':
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break; //Should be Black but Black is non-readable on a black background
                            case '1':
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                break;
                            case '2':
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                break;
                            case '3':
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                break;
                            case '4':
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                break;
                            case '5':
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                break;
                            case '6':
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                break;
                            case '7':
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case '8':
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                break;
                            case '9':
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case 'a':
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case 'b':
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                break;
                            case 'c':
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case 'd':
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                break;
                            case 'e':
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case 'f':
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case 'r':
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                        }

                        if (parts[i].Length > 1)
                        {
                            Write(parts[i][1..]);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Write('\n');
        }

        /// <summary>
        /// Write a prefixed log line. Prefix is set in LogPrefix.
        /// </summary>
        /// <param name="text">Text of the log line</param>
        /// <param name="acceptnewlines">Allow line breaks</param>
        public static void WriteLogLine(string text, bool acceptnewlines = true)
        {
            if (!acceptnewlines)
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
            string clipdata = "";
            var staThread = new Thread(new ThreadStart(
                delegate
                {
                    // TODO Clipboard
                    //clipdata = Clipboard.GetText();
                }
            ));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipdata;
        }

        #endregion

        #region Subfunctions

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
            if (_buffer.Length > 0)
            {
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

                _buffer = _buffer[0..^1];

                if (_buffer2.Length > 0)
                {
                    Console.Write(_buffer2);
                    Console.Write(' ');
                    GoBack();
                    for (int i = 0; i < _buffer2.Length; i++)
                    {
                        GoBack();
                    }
                }
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

            _buffer2 = "" + _buffer[^1] + _buffer2;
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