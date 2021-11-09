using System.Collections.Generic;
using System.Linq;

namespace Client.Network
{
    /// <summary>
    /// Network statistics
    /// </summary>
    public class MeanComputer
    {
        /// <remarks>in ms</remarks>
        public uint MeanPeriod;

        public float Content;

        readonly Queue<KeyValuePair<long, float>> _values = new Queue<KeyValuePair<long, float>>();

        public MeanComputer()
        {
            MeanPeriod = 2000;
            Content = 0.0f;
        }

        public MeanComputer(uint meanPeriod)
        {
            MeanPeriod = meanPeriod;
            Content = 0.0f;
        }

        /// <summary>
        /// checks all values have valid time (inside the mean period)
        /// </summary>
        public void Check(long time)
        {
            while (_values.Count > 0 && _values.First().Key < time - MeanPeriod)
            {
                Content -= _values.First().Value;
                _values.Dequeue();
            }
        }

        /// <summary>
        /// updates the mean with a new value and time
        /// </summary>
        public void Update(float value, long time)
        {
            if (_values.Count > 0 && _values.Last().Key > time)
                return;

            Check(time);

            _values.Enqueue(new KeyValuePair<long, float>(time, value));
            Content += value;
        }

        /// <summary>
        /// gets the mean
        /// </summary>
        public float Mean(long time)
        {
            Check(time);
            return Content * 1000.0f / MeanPeriod;
        }

        public float Dpk()
        {
            return _values.Count == 0 ? 0.0f : _values.Last().Value - _values.First().Value + 1;
        }
    }
}
