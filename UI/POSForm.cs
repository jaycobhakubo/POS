#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The base class from which all POS forms should derive.
    /// </summary>
    internal partial class POSForm : EliteGradientForm
    {
        #region Member Variables
        protected PointOfSale m_parent;
        protected int m_kioskIdleLimitInSeconds = 30; //30 seconds
        protected int m_kioskShortIdleLimitInSeconds = 15; //15 seconds
        protected int m_kioskMessagePauseInMilliseconds = 10000; //10 seconds
        protected Timer m_GuardianCheckTimer = null;
        protected bool m_lastGuardianState = false;
        protected bool m_activeWhenMoved = false;
        private static int m_POSWindowCount = 0; //number of POSForm windows not closed
        private static int m_POSWindowsMoved = 0; //number of POSForm windows moved off the screen
        #endregion

        #region Constructors
        // TTP 50433
        /// <summary>
        /// Initalizes a new instance of the POSForm class.
        /// Required method for Designer support.
        /// </summary>
        protected POSForm()
        {
        }

        public POSForm(PointOfSale parent)
            : this(parent, parent == null ? new NormalDisplayMode() : parent.Settings == null ? new NormalDisplayMode() : parent.Settings.DisplayMode == null ? new NormalDisplayMode() : parent.Settings.DisplayMode)
        {
        }

        /// <summary>
        /// Initalizes a new instance of the POSForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public POSForm(PointOfSale parent, DisplayMode displayMode) : base(displayMode)
        {
            if(parent == null)
                throw new ArgumentNullException("parent");

            InitializeComponent();

            m_POSWindowCount++; //one more POSForm
            m_parent = parent;
            Text = Resources.POSName;

            if (parent.Settings != null) //set the Kiosk timeouts to the user settings
            {
                m_kioskIdleLimitInSeconds = parent.Settings.KioskIdleTimeout;
                m_kioskMessagePauseInMilliseconds = parent.Settings.KioskMessageTimeout;
                m_kioskShortIdleLimitInSeconds = parent.Settings.KioskShortIdleTimeout;
            }

            if (parent.WeAreAPOSKiosk && parent.WeHaveAGuardian) //we have a Guardian, set up a timer to monitor our suspended/active state
            {
                m_GuardianCheckTimer = new Timer();
                m_GuardianCheckTimer.Interval = 2000;
                m_GuardianCheckTimer.Tick += new EventHandler(m_GuardianCheckTimer_Tick);
                m_GuardianCheckTimer.Start();
            }
        }

        #endregion

        #region Member Methods
        
        private void POSForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(m_GuardianCheckTimer != null)
                m_GuardianCheckTimer.Stop();

            if (MovedOffScreen) //we are off screen and going away
                m_POSWindowsMoved--; //one less POSForm off screen

            m_POSWindowCount--; //one less POSForm
        }

        #endregion

        #region Guardian support

        void m_GuardianCheckTimer_Tick(object sender, EventArgs e)
        {
            bool doorIsOpen = m_parent.ClearScreenForGuardian; //see if the Guardian requested for us to suspend operation

            if (doorIsOpen != m_lastGuardianState) //our state changed
            {
                m_GuardianCheckTimer.Stop();
                m_lastGuardianState = doorIsOpen; //set new state

                //since hiding or minimizing a window will terminate ShowDialog, wee move the window off the screen to hide it
                //and move it back to show it.
                if (doorIsOpen) //we should be hidden
                {
                    if (MoveOffScreen()) //move our window off of the screen to hide it
                        m_POSWindowsMoved++; //one more POSForm off screen
                }
                else //should be visible
                {
                    if(MoveOffScreen(false)) //move our window back where it was before we moved it
                        m_POSWindowsMoved--; //one less POSForm off screen
                }
            }

            m_GuardianCheckTimer.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets if all of the POSForm windows are moved off the screen.
        /// </summary>
        public bool AllWindowsOffScreen
        {
            get
            {
                return m_POSWindowCount == m_POSWindowsMoved;
            }
        }

        /// <summary>
        /// Gets if all of the POSForm windows are on the screen.
        /// </summary>
        public bool AllWindowsOnScreen
        {
            get
            {
                return m_POSWindowsMoved == 0;
            }
        }

        public int KioskIdleLimitInSeconds
        {
            get
            {
                return m_kioskIdleLimitInSeconds;
            }
        }

        public int KioskShortIdleLimitInSeconds
        {
            get
            {
                return m_kioskShortIdleLimitInSeconds;
            }
        }

        public int KioskMessagePauseInMilliseconds
        {
            get
            {
                return m_kioskMessagePauseInMilliseconds;
            }
        }

        #endregion
    }
}