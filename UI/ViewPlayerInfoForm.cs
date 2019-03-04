// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2007 GameTech
// International, Inc.

using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Controls;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
    // TODO Revisit ViewPlayerInfoForm.

    /// <summary>
    /// The form that displays player's info.
    /// </summary>
    internal partial class ViewPlayerInfoForm : POSForm
    {
        #region Member Variables
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ScanBarcodeForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        public ViewPlayerInfoForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            // No display mode to apply... one size fits all
        }

        /// <summary>
        /// Handles the close button click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains 
        /// the event data.</param>
        private void m_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Member Properties
        /*
        /// <summary>
        /// Get or Set the Player's First Name.
        /// </summary>
        public string FirstName
        {
            get
            {
                return m_firstName.Text;
            }
            set
            {
                m_firstName.ReadOnly = false;
                m_firstName.Text = value;
                m_firstName.ReadOnly = true;
            }
        }

        /// <summary>
        /// Get or Set the Player's Last Name.
        /// </summary>
        public string LastName
        {
            get
            {
                return m_lastName.Text;
            }
            set
            {
                m_lastName.ReadOnly = false;
                m_lastName.Text = value;
                m_lastName.ReadOnly = true;
            }
        }

        /// <summary>
        /// Get or Set the Player's picture.
        /// </summary>
        public Bitmap PlayerImage
        {
            get
            {
                return (Bitmap)m_noPic.Image;
            }
            set
            {
                m_noPic.Image = value;
            }
        }

        /// <summary>
        /// Get or Set the Player's comments
        /// </summary>
        public string Comments
        {
            get
            {
                return m_comments.Text;
            }
            set
            {
                m_comments.ReadOnly = false;
                m_comments.Text = value;
                m_comments.ReadOnly = true;
            }
        }
        */
        #endregion
    }
}