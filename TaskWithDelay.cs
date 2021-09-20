///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;

namespace RCC
{
    /// <summary>
    /// Holds a task with delay
    /// </summary>
    class TaskWithDelay
    {
        private Action _task;
        private int tickCounter;
        private DateTime dateToLaunch;

        public Action Task { get { return _task; } }

        public TaskWithDelay(Action task, int delayTicks)
        {
            _task = task;
            tickCounter = delayTicks;
            dateToLaunch = DateTime.MaxValue;
        }

        public TaskWithDelay(Action task, TimeSpan delay)
        {
            _task = task;
            tickCounter = int.MaxValue;
            dateToLaunch = DateTime.Now + delay;
        }

        /// <summary>
        /// Tick the counter
        /// </summary>
        /// <returns>Return true if the task should run now</returns>
        public bool Tick()
        {
            tickCounter--;
            if (tickCounter <= 0 || dateToLaunch < DateTime.Now)
                return true;
            return false;
        }
    }
}