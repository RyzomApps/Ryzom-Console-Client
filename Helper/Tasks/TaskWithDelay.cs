///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;

namespace RCC.Helper.Tasks
{
    /// <summary>
    /// Holds a task with delay
    /// </summary>
    internal class TaskWithDelay
    {
        private int _tickCounter;
        private readonly DateTime _dateToLaunch;

        public Action Task { get; }

        public TaskWithDelay(Action task, int delayTicks)
        {
            Task = task;
            _tickCounter = delayTicks;
            _dateToLaunch = DateTime.MaxValue;
        }

        public TaskWithDelay(Action task, TimeSpan delay)
        {
            Task = task;
            _tickCounter = int.MaxValue;
            _dateToLaunch = DateTime.Now + delay;
        }

        /// <summary>
        /// Tick the counter
        /// </summary>
        /// <returns>Return true if the task should run now</returns>
        public bool Tick()
        {
            _tickCounter--;
            return _tickCounter <= 0 || _dateToLaunch < DateTime.Now;
        }
    }
}