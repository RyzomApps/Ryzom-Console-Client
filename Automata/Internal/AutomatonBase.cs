///////////////////////////////////////////////////////////////////
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
using RCC.Chat;
using RCC.Helper.Tasks;
using RCC.Network;

namespace RCC.Automata.Internal
{
    ///
    /// Welcome to the automaton API file !
    /// The virtual class "AutomatonBase" contains anything you need for creating chat automatons
    /// Inherit from this class while adding your automaton class to the "automatons" folder.
    /// Override the methods you want for handling events: Initialize, Update, GetText.
    ///
    /// For testing your automaton you can add it in McClient.cs (see comment at line ~199).
    /// Your automaton will be loaded everytime MCC is started so that you can test/debug.
    ///
    /// Once your automaton is fully written and tested, you can export it a standalone script.
    /// This way it can be loaded in newer MCC builds, without modifying MCC itself.
    /// See config/sample-script-with-automaton.cs for a AutomatonBase script example.
    ///

    /// <summary>
    /// The virtual class containing anything you need for creating chat automatons.
    /// </summary>
    public abstract class AutomatonBase
    {
        public enum DisconnectReason { InGameKick, LoginRejected, ConnectionLost, UserLogout };

        private RyzomClient _handler;

        private readonly List<string> _registeredCommands = new List<string>();
        private readonly object _delayTasksLock = new object();
        private readonly List<TaskWithDelay> _delayedTasks = new List<TaskWithDelay>();

        /// <summary>
        /// Handler will be automatically set on automaton loading, don't worry about this
        /// </summary>
        public void SetHandler(RyzomClient handler) { _handler = handler; }

        protected RyzomClient Handler
        {
            get
            {
                if (_handler != null)
                    return _handler;
                throw new InvalidOperationException("exception.automaton.init");
            }
        }

        /// <summary>
        /// Will be called every ~100ms.
        /// </summary>
        /// <remarks>
        /// <see cref="Update"/> method can be overridden by child class so need an extra update method
        /// </remarks>
        public void UpdateInternal()
        {
            lock (_delayTasksLock)
            {
                if (_delayedTasks.Count > 0)
                {
                    List<int> tasksToRemove = new List<int>();
                    for (int i = 0; i < _delayedTasks.Count; i++)
                    {
                        if (_delayedTasks[i].Tick())
                        {
                            _delayedTasks[i].Task();
                            tasksToRemove.Add(i);
                        }
                    }
                    if (tasksToRemove.Count > 0)
                    {
                        tasksToRemove.Sort((a, b) => b.CompareTo(a)); // descending sort
                        foreach (int index in tasksToRemove)
                        {
                            _delayedTasks.RemoveAt(index);
                        }
                    }
                }
            }
        }

        /* ================================================== */
        /*   Main methods to override for creating your automaton   */
        /* ================================================== */

        /// <summary>
        /// Anything you want to initialize your automaton, will be called on load by MinecraftCom
        /// This method is called only once, whereas OnGameJoined() is called once per server join.
        ///
        /// NOTE: Chat messages cannot be sent at this point in the login process.
        /// If you want to send a message when the automaton is loaded, use OnGameJoined.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Called after the server has been joined successfully and chat messages are able to be sent.
        /// This method is called again after reconnecting to the server, whereas Initialize() is called only once.
        ///
        /// NOTE: This is not always right after joining the server - if the automaton was loaded after logging
        /// in this is still called.
        /// </summary>
        public virtual void OnGameJoined() { }

        /// <summary>
        /// Called after an internal MCC command has been performed
        /// </summary>
        /// <param name="commandName">MCC CommandBase Name</param>
        /// <param name="commandParams">MCC CommandBase Parameters</param>
        /// <param name="result">MCC command result</param>
        public virtual void OnInternalCommand(string commandName, string commandParams, string result) { }

        public virtual void OnGameTeamContactStatus(uint contactId, CharConnectionState online) { }

        public virtual void OnGameTeamContactInit(List<uint> vFriendListName, List<CharConnectionState> vFriendListOnline, List<string> vIgnoreListName) { }

        public virtual void OnTeamContactCreate(in uint contactId, in uint nameId, CharConnectionState online, in byte nList) { }

        public virtual void OnChat(in uint compressedSenderIndex, string ucstr, string rawMessage, ChatGroupType mode, in uint dynChatId, string senderName, in uint bubbleTimer) { }

        public virtual void OnTell(string ucstr, string senderName) { }

        /// <summary>
        /// Will be called every ~100ms (10fps) if loaded in MinecraftCom
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Is called when the client has been disconnected fom the server
        /// </summary>
        /// <param name="reason">Disconnect Reason</param>
        /// <param name="message">Kick message, if any</param>
        /// <returns>Return TRUE if the client is about to restart</returns>
        public virtual bool OnDisconnect(DisconnectReason reason, string message) { return false; }

        /* ======================================================================= */
        /*  ToolBox - Methods below might be useful while creating your automaton. */
        /*  You should not need to interact with other classes of the program.     */
        /*  All the methods in this AutomatonBase class should do the job for you. */
        /* ======================================================================= */

        /// <summary>
        /// Send text to the server. Can be anything such as chat messages or commands
        /// </summary>
        /// <param name="text">Text to send to the server</param>
        /// <param name="sendImmediately">Bypass send queue (Deprecated, still there for compatibility purposes but ignored)</param>
        /// <returns>TRUE if successfully sent (Deprectated, always returns TRUE for compatibility purposes with existing scripts)</returns>
        protected bool SendText(string text, bool sendImmediately = false)
        {
            //LogToConsole("Sending '" + text + "'");
            Handler.SendText(text);
            return true;
        }

        /// <summary>
        /// Perform an internal MCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>TRUE if the command was indeed an internal MCC command</returns>
        protected bool PerformInternalCommand(string command, Dictionary<string, object> localVars = null)
        {
            string temp = "";
            return Handler.PerformInternalCommand(command, ref temp, localVars);
        }

        /// <summary>
        /// Perform an internal MCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="responseMsg">May contain a confirmation or error message after processing the command, or "" otherwise.</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>TRUE if the command was indeed an internal MCC command</returns>
        protected bool PerformInternalCommand(string command, ref string responseMsg, Dictionary<string, object> localVars = null)
        {
            return Handler.PerformInternalCommand(command, ref responseMsg, localVars);
        }

        /// <summary>
        /// Remove color codes ("§c") from a text message received from the server
        /// </summary>
        public static string GetVerbatim(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            int idx = 0;
            var data = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
                if (text[i] != '§')
                    data[idx++] = text[i];
                else
                    i++;

            return new string(data, 0, idx);
        }

        /// <summary>
        /// Disconnect from the server and exit the program
        /// </summary>
        protected void DisconnectAndExit()
        {
            Program.Exit();
        }

        /// <summary>
        /// Unload the automaton, and release associated memory.
        /// </summary>
        protected void UnloadAutomaton()
        {
            foreach (var cmdName in _registeredCommands)
            {
                Handler.UnregisterCommand(cmdName);
            }
            Handler.Automata.UnloadAutomaton(this);
        }

        /// <summary>
        /// Load an additional AutomatonBase
        /// </summary>
        /// <param name="automaton">AutomatonBase to load</param>
        protected void LoadAutomaton(AutomatonBase automaton) { 
            Handler.Automata.UnloadAutomaton(automaton); 
            Handler.Automata.LoadAutomaton(automaton);
        }


        /// <summary>
        /// Get a Y-M-D h:m:s timestamp representing the current system date and time
        /// </summary>
        protected static string GetTimestamp()
        {
            var time = DateTime.Now;
            return $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }

        /// <summary>
        /// Load entries from a file as a string array, removing duplicates and empty lines
        /// </summary>
        /// <param name="file">File to load</param>
        /// <returns>The string array or an empty array if failed to load the file</returns>
        protected string[] LoadDistinctEntriesFromFile(string file)
        {
            if (File.Exists(file))
            {
                //Read all lines from file, remove lines with no text, convert to lowercase,
                //remove duplicate entries, convert to a string array, and return the result.
                return File.ReadAllLines(file, Encoding.UTF8)
                        .Where(line => !String.IsNullOrWhiteSpace(line))
                        .Select(line => line.ToLower())
                        .Distinct().ToArray();
            }
            else
            {
                //LogToConsole("File not found: " + System.IO.Path.GetFullPath(file));
                return new string[0];
            }
        }

        /// <summary>
        /// Invoke a task on the main thread, wait for completion and retrieve return value.
        /// </summary>
        /// <param name="task">Task to run with any type or return value</param>
        /// <returns>Any result returned from task, result type is inferred from the task</returns>
        /// <example>bool result = InvokeOnMainThread(methodThatReturnsAbool);</example>
        /// <example>bool result = InvokeOnMainThread(() => methodThatReturnsAbool(argument));</example>
        /// <example>int result = InvokeOnMainThread(() => { yourCode(); return 42; });</example>
        /// <typeparam name="T">Type of the return value</typeparam>
        protected T InvokeOnMainThread<T>(Func<T> task)
        {
            return Handler.InvokeOnMainThread(task);
        }

        /// <summary>
        /// Invoke a task on the main thread and wait for completion
        /// </summary>
        /// <param name="task">Task to run without return value</param>
        /// <example>InvokeOnMainThread(methodThatReturnsNothing);</example>
        /// <example>InvokeOnMainThread(() => methodThatReturnsNothing(argument));</example>
        /// <example>InvokeOnMainThread(() => { yourCode(); });</example>
        protected void InvokeOnMainThread(Action task)
        {
            Handler.InvokeOnMainThread(task);
        }

        /// <summary>
        /// Schedule a task to run on the main thread, and do not wait for completion
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="delayTicks">Run the task after X ticks (1 tick delay = ~100ms). 0 for no delay</param>
        /// <example>
        /// <example>InvokeOnMainThread(methodThatReturnsNothing, 10);</example>
        /// <example>InvokeOnMainThread(() => methodThatReturnsNothing(argument), 10);</example>
        /// <example>InvokeOnMainThread(() => { yourCode(); }, 10);</example>
        /// </example>
        protected void ScheduleOnMainThread(Action task, int delayTicks = 0)
        {
            lock (_delayTasksLock)
            {
                _delayedTasks.Add(new TaskWithDelay(task, delayTicks));
            }
        }

        /// <summary>
        /// Register a command in command prompt. CommandBase will be automatically unregistered when unloading AutomatonBase
        /// </summary>
        /// <param name="cmdName">Name of the command</param>
        /// <param name="cmdDesc">Description/usage of the command</param>
        /// <param name="cmdUsage">Usage example</param>
        /// <param name="callback">Method for handling the command</param>
        /// <returns>True if successfully registered</returns>
        protected bool RegisterAutomatonCommand(string cmdName, string cmdDesc, string cmdUsage, CommandRunner callback)
        {
            bool result = Handler.RegisterCommand(cmdName, cmdDesc, cmdUsage, callback);
            if (result)
                _registeredCommands.Add(cmdName.ToLower());
            return result;
        }

        /// <summary>
        /// Schedule a task to run on the main thread, and do not wait for completion
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="delay">Run the task after the specified delay</param>
        protected void ScheduleOnMainThread(Action task, TimeSpan delay)
        {
            lock (_delayTasksLock)
            {
                _delayedTasks.Add(new TaskWithDelay(task, delay));
            }
        }

        /// <summary>
        /// CommandBase runner definition.
        /// Returned string will be the output of the command
        /// </summary>
        /// <param name="command">Full command</param>
        /// <param name="args">Arguments in the command</param>
        /// <returns>CommandBase result to display to the user</returns>
        public delegate string CommandRunner(string command, string[] args);
    }
}
