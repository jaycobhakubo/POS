#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// The exception that is thrown when a non-fatal POS error occurs.
    /// </summary>
    internal class POSException : ModuleException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the POSException class.
        /// </summary>
        public POSException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSException class with a 
        /// specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public POSException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSException class with a 
        /// specified error message and a reference to the inner exception 
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of 
        /// the current exception. If the innerException parameter is not a 
        /// null reference, the current exception is raised in a catch block 
        /// that handles the inner exception.</param>
        public POSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }

    /// <summary>
    /// The exception that is thrown when a non-fatal Barcode scan error occurs.
    /// </summary>
    internal class BarcodeException : POSException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BarcodeException class.
        /// </summary>
        public BarcodeException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BarcodeException class with a 
        /// specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public BarcodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BarcodeException class with a 
        /// specified error message and a reference to the inner exception 
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of 
        /// the current exception. If the innerException parameter is not a 
        /// null reference, the current exception is raised in a catch block 
        /// that handles the inner exception.</param>
        public BarcodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }

    /// <summary>
    /// The exception that is thrown when the user cancels a process.
    /// </summary>
    internal class POSUserCancelException : POSException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the POSUserCancelException class.
        /// </summary>
        public POSUserCancelException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSUserCancelException class with 
        /// a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public POSUserCancelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSUserCancelException class with 
        /// a specified error message and a reference to the inner exception 
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of 
        /// the current exception. If the innerException parameter is not a 
        /// null reference, the current exception is raised in a catch block 
        /// that handles the inner exception.</param>
        public POSUserCancelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }

    /// <summary>
    /// The exception that is thrown when a non-fatal printing error occurs.
    /// </summary>
    internal class POSPrintException : POSException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the POSPrintException class.
        /// </summary>
        public POSPrintException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSPrintException class with 
        /// a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public POSPrintException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the POSPrintException class with 
        /// a specified error message and a reference to the inner exception 
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of 
        /// the current exception. If the innerException parameter is not a 
        /// null reference, the current exception is raised in a catch block 
        /// that handles the inner exception.</param>
        public POSPrintException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }
}
