using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RCC.Entity
{
    class UserEntity
    {
        private static int _pitch;

        public static Vector3 Pos { get; set; }

        public static Vector3 Front { get; set; }

        public static Vector3 Dir { get; set; }

        public static void SetHeadPitch(int pitch)
        {
            _pitch = pitch;
        }
    }
}
