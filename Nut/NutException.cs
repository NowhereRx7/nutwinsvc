﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutWinSvc.Nut
{
    public class NutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NutException"/> class.
        /// </summary>
        public NutException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NutException"/> class with a specified error message.
        /// </summary>
        public NutException(string? message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NutException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public NutException(string? message, Exception? innerException = null) : base(message, innerException) { }
    }
}
