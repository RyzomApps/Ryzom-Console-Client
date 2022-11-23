///////////////////////////////////////////////////////////////////
// This file contains modified code from 'SpigotMC'
// https://www.spigotmc.org/
// which is released under GPL-3.0 License.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2020 SpigotMC
///////////////////////////////////////////////////////////////////

using System.Threading;

namespace Client.Helper
{
    public class RollingAverage
    {
        public const int GameTps = 10;

        public const double SecInNano = 1E9;

        private readonly int _size;

        private int _index;

        private double _time;

        private double _total;

        private double[] _samples;

        private double[] _times;

        private readonly Mutex _guard = new Mutex();

        /// <summary>
        /// Sum of data over time per time period.
        /// </summary>
        public RollingAverage(int size)
        {
            _size = size;

            Reset();
        }

        // Reset the counters
        internal void Reset()
        {
            _guard.WaitOne();

            _time = _size * SecInNano;
            _total = _time * GameTps;

            _samples = new double[_size];
            _times = new double[_size];

            for (var i = 0; i < _size; i++)
            {
                _samples[i] = GameTps;
                _times[i] = SecInNano;
            }

            _guard.ReleaseMutex();
        }

        /// <summary>
        /// Add a new sample and time.
        /// </summary>
        /// <param name="x">sample</param>
        /// <param name="t">time</param>
        public void Add(double x, long t)
        {
            _guard.WaitOne();

            // remove values from old index
            _time -= _times[_index];
            _total -= _samples[_index] * _times[_index];

            // update at index
            _samples[_index] = x;
            _times[_index] = t;

            // add new values
            _time += t;
            _total += x * t;

            // overflow
            if (++_index >= _size)
            {
                _index = 0;
            }

            _guard.ReleaseMutex();
        }

        /// <summary>
        /// Unweighted mean of the values.
        /// </summary>
        public double GetAverage()
        {
            return _total / _time;
        }
    }
}
