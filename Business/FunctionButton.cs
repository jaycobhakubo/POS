#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Windows.Forms;
using GTI.Modules.POS.Data;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a button that performs a function.
    /// </summary>
    internal class FunctionButton : MenuButton
    {
        #region Member Variables
        protected int m_functionId;
        protected string m_call;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FunctionButton class.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale class.</param>
        /// <param name="functionId">The id of the function.</param>
        /// <param name="call">The function call this button uses.</param>
        /// <exception cref="System.ArgumentNullException">pos is a null 
        /// reference.</exception>
        public FunctionButton(PointOfSale pos, int functionId, string call)
            : base(pos)
        {
            m_functionId = functionId;
            m_call = call;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles when the function button is clicked.
        /// </summary>
        /// <param name="sender">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="argument">Any user defined data to pass to 
        /// the function.</param>
        public override void Click(IWin32Window sender, object argument)
        {
            switch((Function)m_functionId)
            {
                case Function.TransferUnit:
                    m_pos.TransferUnit();
                    break;
            }
        }

        /// <summary>
        /// Checks to see if this function button should be enabled.
        /// </summary>
        /// <returns>true if the button should be enabled; otherwise 
        /// false.</returns>
        public bool CheckEnabled()
        {
            bool returnVal;

            switch((Function)m_functionId)
            {
                case Function.TransferUnit:
                    returnVal = (m_pos.Settings.AllowElectronicSales && m_pos.IsUnitMgmtInitialized);
                    break;

                default:
                    returnVal = false;
                    break;
            }

            return returnVal;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the function's id.
        /// </summary>
        public int FunctionId
        {
            get
            {
                return m_functionId;
            }
            set
            {
                m_functionId = value;
            }
        }

        /// <summary>
        /// Gets or sets the function call this button uses.
        /// </summary>
        public string Call
        {
            get
            {
                return m_call;
            }
            set
            {
                m_call = value;
            }
        }
        #endregion
    }
}
