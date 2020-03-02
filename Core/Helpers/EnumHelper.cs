﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}