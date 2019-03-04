#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

namespace GTI.Modules.POS.Business
{
    // TODO Use Button's Color.

    /// <summary>
    /// Represents a button on a menu.
    /// </summary>
    internal abstract class MenuButton
    {
        #region Member Variables
        protected PointOfSale m_pos;
        protected byte m_page;
        protected byte m_position;
        protected string m_text;
        protected Color m_color = Color.Empty;
        protected int m_graphicId; // PDTS 964
        protected bool m_isLocked;
        protected bool m_isPlayerRequired; // Rally DE129
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MenuButton class.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale class.</param>
        /// <exception cref="System.ArgumentNullException">pos is a null 
        /// reference.</exception>
        protected MenuButton(PointOfSale pos)
        {
            if(pos == null)
                throw new ArgumentNullException("pos");

            m_pos = pos;
        }
        #endregion


        #region Member Methods
        /// <summary>
        /// Handles when the menu button is clicked.
        /// </summary>
        /// <param name="sender">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="argument">Any user defined data to pass to 
        /// the function.</param>
        public abstract void Click(IWin32Window sender, object argument);
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets which page the button is on.  This value may be
        /// different than the actual value if a different screen
        /// resolution is used.
        /// </summary>
        public byte Page
        {
            get
            {
                return m_page;
            }
            set
            {
                m_page = value;
            }
        }

        /// <summary>
        /// Gets or sets which position the button occupies.  This value may be
        /// different than the actual value if a different screen
        /// resolution is used.
        /// </summary>
        public byte Position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }

        /// <summary>
        /// Gets or sets the text on the button.
        /// </summary>
        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the button.
        /// </summary>
        public Color Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        // PDTS 964
        /// <summary>
        /// Gets or sets the id of the graphic to use for the button or 0 for 
        /// no graphic.
        /// </summary>
        public int GraphicId
        {
            get
            {
                return m_graphicId;
            }
            set
            {
                m_graphicId = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the button is disabled.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return m_isLocked;
            }
            set
            {
                m_isLocked = value;
            }
        }

        // Rally DE129
        /// <summary>
        /// Gets or sets whether this button requires a player in order to
        /// press.
        /// </summary>
        public bool IsPlayerRequired
        {
            get
            {
                return m_isPlayerRequired;
            }
            set
            {
                m_isPlayerRequired = value;
            }
        }
        #endregion
    }
}
