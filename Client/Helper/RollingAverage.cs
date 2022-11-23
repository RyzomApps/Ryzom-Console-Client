///////////////////////////////////////////////////////////////////
// This file contains modified code from 'SpigotMC'
// https://www.spigotmc.org/
// which is released under GPL-3.0 License.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2020 SpigotMC
///////////////////////////////////////////////////////////////////

namespace Client.Helper
{
    public class RollingAverage
    {
        public const int GameTps = 10;
        public const long SecInNano = 1000000000;

        private readonly int _size;

        private long _time;

        private double _total;

        private int _index;

        private double[] _samples;

        private long[] _times;

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
            _time = _size * SecInNano;
            _total = (double)_time * GameTps;

            _samples = new double[_size];
            _times = new long[_size];

            for (var i = 0; i < _size; i++)
            {
                _samples[i] = GameTps;
                _times[i] = SecInNano;
            }
        }

        /// <summary>
        /// Add a new sample and time.
        /// </summary>
        /// <param name="x">sample</param>
        /// <param name="t">time</param>
        public void Add(double x, long t)
        {
            _time -= _times[_index];
            _total -= _samples[_index] * _times[_index];

            _samples[_index] = x;
            _times[_index] = t;

            _time += t;
            _total += x * t;

            if (++_index == _size)
            {
                _index = 0;
            }
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
