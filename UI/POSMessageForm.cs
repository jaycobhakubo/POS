// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2007 GameTech
// International, Inc.

using System;
using System.Drawing;
using System.Windows.Forms;
using GTI.Controls;
//using GTI.Modules.Shared.Properties;
using System.Drawing.Drawing2D;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using System.Threading;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Enumerates the different types of message forms.
    /// </summary>
    public enum POSMessageFormTypes
    {
        OK,
        YesNo,
        YesCancel,
        YesNoCancel,
        YesNo_regular,
        Pause,
        YesCancelComp,
        YesNo_FlatDesign,
        OK_FlatDesign,
        RetryAbort,
        RetryAbortIgnore,
        RetryAbortForce,

        YesNo_DefNO,
        YesCancel_DefCancel,
        YesNoCancel_DefNo,
        YesNoCancel_DefCancel,
        YesNo_regular_DefNo,
        YesCancelComp_DefCancel,
        YesNo_FlatDesign_DefNo,
        RetryAbort_DefAbort,
        RetryAbortIgnore_DefAbort,
        RetryAbortIgnore_DefIgnore,
        RetryAbortForce_DefAbort,
        RetryAbortForce_DefForce,
        ContinueCancel,
        ContinueCancel_DefCancel,
        Custom1Button,
        Custom2Button,
        Custom3Button,
        CloseWithCashDrawer,

        ColorButtons = 1000
    }
    /// <summary>
    /// A form that displays messages.
    /// </summary>
    internal partial class POSMessageForm : POSForm
    {
        #region Constants and Data Types
        protected readonly Size TouchYesNoCancelSize = new Size(365, 240);
        protected readonly Size TouchButtonSize = new Size(133, 50);
        #endregion

        #region Member Variables
        protected string m_button1Text = string.Empty;
        protected string m_button2Text = string.Empty;
        protected string m_button3Text = string.Empty;
        protected int m_defaultButton = 0;
        private Thread m_cashDrawerThread = null;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MessageForm class in 
        /// touchscreen mode.
        /// </summary>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="text">The message to be displayed on the 
        /// form.</param>
        /// <param name="alignment">The string alignment of the text.</param>
        /// <param name="type">The type of MessageForm to show.</param>
        /// <param name="pause">If the type is Pause, then this is how long to 
        /// show the dialog (in milliseconds).  It cannot be less 
        /// than one.</param>
        /// <exception cref="System.ArgumentNullException">displayMode is a 
        /// null reference.</exception>
        /// <exception cref="System.ArgumentException">type is 
        /// MessageFormTypes.Pause and pause is less than one.</exception>
        protected POSMessageForm(PointOfSale parent, string text, ContentAlignment alignment, POSMessageFormTypes type, int pause)
            : base(parent)
        {
            InitializeComponent();
            ArrangeForm(text, alignment, null, type, pause);
        }

        protected POSMessageForm(PointOfSale parent, string text, string caption, ContentAlignment alignment, POSMessageFormTypes type, int pause)
            : base(parent)
        {
            InitializeComponent();
            ArrangeForm(text, alignment, caption, type, pause);
        }

        protected POSMessageForm(PointOfSale parent, string text, string caption, POSMessageFormTypes type, int defaultButton, string button1Text, string button2Text, string button3Text, int pause)
            : base(parent)
        {
            DefaultButton = defaultButton;
            Button1Text = button1Text;
            Button2Text = button2Text;
            Button3Text = button3Text;

            InitializeComponent();
            ArrangeForm(text, ContentAlignment.MiddleCenter, caption, type, pause);
        }

        #endregion

        #region Member Methods

        /// <summary>
        /// Arranges the controls on the form based on mode.
        /// </summary>
        /// <param name="text">The message to be displayed on the 
        /// form.</param>
        /// <param name="alignment">The string alignment of the text.</param>
        /// <param name="caption">The text to be displayed on the title 
        /// bar.</param>
        /// <param name="type">The type of MessageForm to show.</param>
        /// <param name="pause">If the type is Pause, then this is how long to 
        /// show the dialog (in milliseconds).  It cannot be less 
        /// than one.</param>
        /// <exception cref="System.ArgumentException">type is 
        /// MessageFormTypes.Pause and pause is less than one.</exception>
        protected void ArrangeForm(string text, ContentAlignment alignment, string caption, POSMessageFormTypes type, int pause = 0)
        {
            bool weAreAKiosk = m_parent.WeAreAPOSKiosk;

            if(weAreAKiosk)
                DrawAsBurst = true;

            bool UseColorButtons = type >= POSMessageFormTypes.ColorButtons || weAreAKiosk;

            if (type >= POSMessageFormTypes.ColorButtons)
                type -= POSMessageFormTypes.ColorButtons;

            ImageButton pressAfterPause = null;

            // Set the message.
            m_messageLabel.Text = text;
            m_messageLabel.TextAlign = alignment;

            FormBorderStyle = FormBorderStyle.None;
            Font = m_displayMode.MenuButtonFont;
            Text = "";

            // Create the panel to hold the buttons.
            TableLayoutPanel innerPanel = new TableLayoutPanel();
            innerPanel.Dock = DockStyle.Fill;
            innerPanel.BackColor = Color.Transparent;

            // Create columns and buttons based on button mode.
            if(type == POSMessageFormTypes.YesNo || type == POSMessageFormTypes.YesNo_DefNO)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                yesButton.Text = Resources.MessageFormYes;
                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormNo;
                noButton.DialogResult = DialogResult.No;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesNo)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.ContinueCancel || type == POSMessageFormTypes.ContinueCancel_DefCancel)
            {
                //Returns OK for Continue and Cancel for Cancel

                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                yesButton.Text = Resources.MessageFormContinue;
                yesButton.DialogResult = DialogResult.OK;
                noButton.Text = Resources.MessageFormCancel;
                noButton.DialogResult = DialogResult.Cancel;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesNo)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.RetryAbort || type == POSMessageFormTypes.RetryAbort_DefAbort)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
                
                yesButton.Text = Resources.MessageFormRetry;
                yesButton.DialogResult = DialogResult.Retry;
                noButton.Text = Resources.MessageFormAbort;
                noButton.DialogResult = DialogResult.Abort;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.RetryAbort)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.YesNo_FlatDesign || type == POSMessageFormTypes.YesNo_FlatDesign_DefNo)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
                
                yesButton.Text = Resources.MessageFormYes;
                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormNo;
                noButton.DialogResult = DialogResult.No;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesNo_FlatDesign)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                //Use flat background
                yesButton.ForeColor = System.Drawing.Color.Black;
                noButton.ForeColor = System.Drawing.Color.Black;         
                DrawAsGradient = false;
                System.Drawing.Color defaultBackground = System.Drawing.ColorTranslator.FromHtml("#44658D");
                this.BackColor = defaultBackground;
                this.ForeColor = System.Drawing.Color.White;
            }
            else if (type == POSMessageFormTypes.YesNo_regular || type == POSMessageFormTypes.YesNo_regular_DefNo)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
                
                m_messageLabel.Font = new Font(m_messageLabel.Font, FontStyle.Regular);
                yesButton.Text = Resources.MessageFormYes;
                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormNo;
                noButton.DialogResult = DialogResult.No;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 1;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesNo_regular)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                
                yesButton.Font = new Font(yesButton.Font, FontStyle.Regular);
                noButton.Font = new Font(noButton.Font, FontStyle.Regular);
                noButton.Focus();
                noButton.Select();

            }
            else if (type == POSMessageFormTypes.YesCancelComp || type == POSMessageFormTypes.YesCancelComp_DefCancel)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
                
                m_messageLabel.Font = new Font(m_messageLabel.Font, FontStyle.Regular);
                yesButton.Text = "&Award";//Resources.MessageFormYes;

                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormCancel;
                noButton.DialogResult = DialogResult.Cancel;

                //Use flat background 
                DrawAsGradient = false;
                System.Drawing.Color defaultBackground = System.Drawing.ColorTranslator.FromHtml("#44658D");
                this.BackColor = defaultBackground;
                this.ForeColor = System.Drawing.Color.White;
                yesButton.ForeColor = Color.Black;
                noButton.ForeColor = Color.Black;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesCancelComp)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                yesButton.Font = new Font(yesButton.Font, FontStyle.Regular);
                noButton.Font = new Font(noButton.Font, FontStyle.Regular);
            }
            else if (type == POSMessageFormTypes.YesCancel || type == POSMessageFormTypes.YesCancel_DefCancel)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 5;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                m_messageLabel.Font = new Font(m_messageLabel.Font, FontStyle.Regular);
                yesButton.Text = Resources.MessageFormYes;
              
                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormCancel;
                noButton.DialogResult = DialogResult.Cancel;
       
                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesCancel)
                    pressAfterPause = yesButton;
                else
                    pressAfterPause = noButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                yesButton.Font = new Font(yesButton.Font, FontStyle.Regular);
                noButton.Font = new Font(noButton.Font, FontStyle.Regular);
            }
            else if(type == POSMessageFormTypes.YesNoCancel || type == POSMessageFormTypes.YesNoCancel_DefNo || type == POSMessageFormTypes.YesNoCancel_DefCancel)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 7;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();
                ImageButton cancelButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                yesButton.Text = Resources.MessageFormYes;
                yesButton.DialogResult = DialogResult.Yes;
                noButton.Text = Resources.MessageFormNo;
                noButton.DialogResult = DialogResult.No;
                cancelButton.Text = Resources.MessageFormCancel;
                cancelButton.DialogResult = DialogResult.Cancel;

                Size = TouchYesNoCancelSize;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    cancelButton.ImageNormal = Resources.YellowButtonUp;
                    cancelButton.ImagePressed = Resources.YellowButtonDown;
                }
                else
                {
                    cancelButton.ImageNormal = Resources.BigBlueButtonUp;
                    cancelButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                cancelButton.ShowFocus = false;
                cancelButton.TabIndex = 0;
                cancelButton.TabStop = false;
                cancelButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;
                cancelButton.Text = cancelButton.Text.Replace("&", string.Empty);
                cancelButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.YesNoCancel)
                    pressAfterPause = yesButton;
                else if (type == POSMessageFormTypes.YesNoCancel_DefNo)
                    pressAfterPause = noButton;
                else //default to Cancel
                    pressAfterPause = cancelButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);
                innerPanel.Controls.Add(cancelButton, 5, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.RetryAbortIgnore || type == POSMessageFormTypes.RetryAbortIgnore_DefAbort || type == POSMessageFormTypes.RetryAbortIgnore_DefIgnore)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 7;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();
                ImageButton cancelButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                yesButton.Text = Resources.MessageFormRetry;
                yesButton.DialogResult = DialogResult.Retry;
                noButton.Text = Resources.MessageFormAbort;
                noButton.DialogResult = DialogResult.Abort;
                cancelButton.Text = Resources.MessageFormIgnore;
                cancelButton.DialogResult = DialogResult.Ignore;

                Size = TouchYesNoCancelSize;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    cancelButton.ImageNormal = Resources.YellowButtonUp;
                    cancelButton.ImagePressed = Resources.YellowButtonDown;
                }
                else
                {
                    cancelButton.ImageNormal = Resources.BigBlueButtonUp;
                    cancelButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                cancelButton.ShowFocus = false;
                cancelButton.TabIndex = 0;
                cancelButton.TabStop = false;
                cancelButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;
                cancelButton.Text = cancelButton.Text.Replace("&", string.Empty);
                cancelButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.RetryAbortIgnore)
                    pressAfterPause = yesButton;
                else if (type == POSMessageFormTypes.RetryAbortIgnore_DefAbort)
                    pressAfterPause = noButton;
                else //default to Ignore
                    pressAfterPause = cancelButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);
                innerPanel.Controls.Add(cancelButton, 5, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.RetryAbortForce || type == POSMessageFormTypes.RetryAbortForce_DefAbort || type == POSMessageFormTypes.RetryAbortForce_DefForce)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 7;

                ImageButton yesButton = new ImageButton();
                ImageButton noButton = new ImageButton();
                ImageButton cancelButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    yesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    noButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                yesButton.Text = Resources.MessageFormRetry;
                yesButton.DialogResult = DialogResult.Retry;
                noButton.Text = Resources.MessageFormAbort;
                noButton.DialogResult = DialogResult.Abort;
                cancelButton.Text = Resources.MessageFormForce;
                cancelButton.DialogResult = DialogResult.Ignore;

                Size = TouchYesNoCancelSize;

                if (UseColorButtons)
                {
                    yesButton.ImageNormal = Resources.GreenButtonUp;
                    yesButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    yesButton.ImageNormal = Resources.BigBlueButtonUp;
                    yesButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                yesButton.ShowFocus = false;
                yesButton.TabIndex = 0;
                yesButton.TabStop = false;
                yesButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    noButton.ImageNormal = Resources.RedButtonUp;
                    noButton.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    noButton.ImageNormal = Resources.BigBlueButtonUp;
                    noButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                noButton.ShowFocus = false;
                noButton.TabIndex = 0;
                noButton.TabStop = false;
                noButton.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    cancelButton.ImageNormal = Resources.YellowButtonUp;
                    cancelButton.ImagePressed = Resources.YellowButtonDown;
                }
                else
                {
                    cancelButton.ImageNormal = Resources.BigBlueButtonUp;
                    cancelButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                cancelButton.ShowFocus = false;
                cancelButton.TabIndex = 0;
                cancelButton.TabStop = false;
                cancelButton.Size = TouchButtonSize;

                // Remove Mnemonics
                yesButton.Text = yesButton.Text.Replace("&", string.Empty);
                yesButton.UseMnemonic = false;
                noButton.Text = noButton.Text.Replace("&", string.Empty);
                noButton.UseMnemonic = false;
                cancelButton.Text = cancelButton.Text.Replace("&", string.Empty);
                cancelButton.UseMnemonic = false;

                if (type == POSMessageFormTypes.RetryAbortForce)
                    pressAfterPause = yesButton;
                else if (type == POSMessageFormTypes.RetryAbortForce_DefAbort)
                    pressAfterPause = noButton;
                else //default to Force
                    pressAfterPause = cancelButton;

                innerPanel.Controls.Add(yesButton, 1, 0);
                innerPanel.Controls.Add(noButton, 3, 0);
                innerPanel.Controls.Add(cancelButton, 5, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else if (type == POSMessageFormTypes.Custom1Button || type == POSMessageFormTypes.Custom2Button || type == POSMessageFormTypes.Custom3Button)
            {
                innerPanel.RowCount = 1;

                if (type == POSMessageFormTypes.Custom1Button)
                    innerPanel.ColumnCount = 3;
                else if (type == POSMessageFormTypes.Custom2Button)
                    innerPanel.ColumnCount = 5;
                else
                    innerPanel.ColumnCount = 7;

                ImageButton button1 = new ImageButton();
                ImageButton button2 = new ImageButton();
                ImageButton button3 = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                {
                    button1.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    button2.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    button3.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                button1.Text = Button1Text;
                button1.DialogResult = (DialogResult)1;
                button2.Text = Button2Text;
                button2.DialogResult = (DialogResult)2;
                button3.Text = Button3Text;
                button3.DialogResult = (DialogResult)3;

                if(type == POSMessageFormTypes.Custom3Button)
                    Size = TouchYesNoCancelSize;

                if (UseColorButtons)
                {
                    button1.ImageNormal = Resources.GreenButtonUp;
                    button1.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    button1.ImageNormal = Resources.BigBlueButtonUp;
                    button1.ImagePressed = Resources.BigBlueButtonDown;
                }

                button1.ShowFocus = false;
                button1.TabIndex = 0;
                button1.TabStop = false;
                button1.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    button2.ImageNormal = Resources.RedButtonUp;
                    button2.ImagePressed = Resources.RedButtonDown;
                }
                else
                {
                    button2.ImageNormal = Resources.BigBlueButtonUp;
                    button2.ImagePressed = Resources.BigBlueButtonDown;
                }

                button2.ShowFocus = false;
                button2.TabIndex = 0;
                button2.TabStop = false;
                button2.Size = TouchButtonSize;

                if (UseColorButtons)
                {
                    button3.ImageNormal = Resources.YellowButtonUp;
                    button3.ImagePressed = Resources.YellowButtonDown;
                }
                else
                {
                    button3.ImageNormal = Resources.BigBlueButtonUp;
                    button3.ImagePressed = Resources.BigBlueButtonDown;
                }

                button3.ShowFocus = false;
                button3.TabIndex = 0;
                button3.TabStop = false;
                button3.Size = TouchButtonSize;

                // Remove hotkeys
                button1.Text = button1.Text.Replace("&", string.Empty);
                button1.UseMnemonic = false;
                button2.Text = button2.Text.Replace("&", string.Empty);
                button2.UseMnemonic = false;
                button3.Text = button3.Text.Replace("&", string.Empty);
                button3.UseMnemonic = false;

                if (DefaultButton == 0 || DefaultButton ==1)
                    pressAfterPause = button1;
                else if (DefaultButton == 2)
                    pressAfterPause = button2;
                else //default to 3
                    pressAfterPause = button3;

                if (type == POSMessageFormTypes.Custom1Button)
                {
                    innerPanel.Controls.Add(button1, 1, 0);

                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                }
                else if (type == POSMessageFormTypes.Custom2Button)
                {
                    innerPanel.Controls.Add(button1, 1, 0);
                    innerPanel.Controls.Add(button2, 3, 0);

                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                }
                else
                {
                    innerPanel.Controls.Add(button1, 1, 0);
                    innerPanel.Controls.Add(button2, 3, 0);
                    innerPanel.Controls.Add(button3, 5, 0);

                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                    innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                }
            }
            else if (type == POSMessageFormTypes.OK_FlatDesign)
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 3;

                ImageButton okButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                    okButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                okButton.Text = Resources.MessageFormOk;
                okButton.DialogResult = DialogResult.OK;

                //Use flat background 
                DrawAsGradient = false;
                System.Drawing.Color defaultBackground = System.Drawing.ColorTranslator.FromHtml("#44658D");
                this.BackColor = defaultBackground;
                this.ForeColor = System.Drawing.Color.White;
                okButton.ForeColor = Color.Black;
                //yesButton.ForeColor = Color.Black;
                //noButton.ForeColor = Color.Black;

                if (UseColorButtons)
                {
                    okButton.ImageNormal = Resources.GreenButtonUp;
                    okButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    okButton.ImageNormal = Resources.BigBlueButtonUp;
                    okButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                okButton.ShowFocus = false;
                okButton.TabIndex = 0;
                okButton.TabStop = false;
                okButton.Size = TouchButtonSize;

                // Remove Mnemonic
                okButton.Text = okButton.Text.Replace("&", string.Empty);
                okButton.UseMnemonic = false;

                pressAfterPause = okButton;

                if (type == POSMessageFormTypes.Pause || type == POSMessageFormTypes.CloseWithCashDrawer)
                    okButton.Visible = false;

                innerPanel.Controls.Add(okButton, 1, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            else
            {
                innerPanel.RowCount = 1;
                innerPanel.ColumnCount = 3;

                ImageButton okButton = new ImageButton();

                if (m_parent.WeAreAPOSKiosk)
                    okButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                okButton.Text = Resources.MessageFormOk;
                okButton.DialogResult = DialogResult.OK;

                if (UseColorButtons)
                {
                    okButton.ImageNormal = Resources.GreenButtonUp;
                    okButton.ImagePressed = Resources.GreenButtonDown;
                }
                else
                {
                    okButton.ImageNormal = Resources.BigBlueButtonUp;
                    okButton.ImagePressed = Resources.BigBlueButtonDown;
                }

                okButton.ShowFocus = false;
                okButton.TabIndex = 0;
                okButton.TabStop = false;
                okButton.Size = TouchButtonSize;

                // Remove Mnemonic
                okButton.Text = okButton.Text.Replace("&", string.Empty);
                okButton.UseMnemonic = false;

                pressAfterPause = okButton;

                if (type == POSMessageFormTypes.Pause || type == POSMessageFormTypes.CloseWithCashDrawer)
                    okButton.Visible = false;

                innerPanel.Controls.Add(okButton, 1, 0);

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133F));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }

            m_outerPanel.Controls.Add(innerPanel, 0, 1);
            innerPanel.Click += SomethingWasClicked;

            if (type != POSMessageFormTypes.CloseWithCashDrawer)
            {
                if (type == POSMessageFormTypes.Pause)
                {
                    if (pause == 0)
                        pause = 5000;

                    if (pause < 1000)
                        pause = 1000;
                }

                if (weAreAKiosk && pause == 0)
                    pause = KioskMessagePauseInMilliseconds;

                if (pause > 0)
                {
                    m_pauseProgress.Minimum = 0;
                    m_pauseProgress.Maximum = pause;
                    m_pauseProgress.Value = 0;
                    m_pauseProgress.Visible = true;

                    m_pauseTimer.Interval = 100;
                    m_pauseTimer.Tag = pressAfterPause;
                    pressAfterPause.Pulse = m_parent.WeAreAPOSKiosk && m_parent.Settings.KioskTimeoutPulseDefaultButton && innerPanel.ColumnCount > 3;
                    m_pauseTimer.Start();
                }

                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
            else //wait for the cash drawer to close
            {
                m_cashDrawerThread = new Thread(CashDrawerChecker);
                m_cashDrawerThread.Start(pressAfterPause);
            }
        }

        private void CashDrawerChecker(object OKButton)
        {
            while (m_parent.CashDrawerIsOpen())
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            this.Invoke(new MethodInvoker(delegate()
            {
                ((ImageButton)OKButton).Visible = true;
                ((ImageButton)OKButton).PerformClick();
            }));
        }

        /// <summary>
        /// Handles the timer's Tick event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the event 
        /// data.</param>
        private void PauseTimerTick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                SomethingWasClicked(sender, e);
                return;
            }

            m_pauseProgress.Increment(m_pauseTimer.Interval);

            if (m_pauseProgress.Value < m_pauseProgress.Maximum)
                return;

            m_pauseTimer.Stop();

            //m_pauseProgress.Visible = false;

            if (!((ImageButton)m_pauseTimer.Tag).Visible) //need to see the button to click it
                ((ImageButton)m_pauseTimer.Tag).Visible = true;

            ((ImageButton)m_pauseTimer.Tag).PerformClick();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Displays a touchscreen OK message form in front of the specified 
        /// object with specified text.
        /// </summary>
        /// <param name="owner">An implementation of IWin32Window that will 
        /// own the modal dialog box.</param>
        /// <param name="parent">The PointOfSale class used as the parent of the created POSForm.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the DialogResult values.</returns>
        /// <exception cref="System.ArgumentNullException">displayMode is a 
        /// null reference.</exception>
        public static DialogResult Show(IWin32Window owner, PointOfSale parent, string text)
        {
            POSMessageForm form = new POSMessageForm(parent, text, ContentAlignment.MiddleCenter, POSMessageFormTypes.OK, 0);
            return DoTheDialog(form, owner);
        }

        /// <summary>
        /// Displays a touchscreen message form in front of the specified object 
        /// with specified text.
        /// </summary>
        /// <param name="owner">An implementation of IWin32Window that will 
        /// own the modal dialog box.</param>
        /// <param name="parent">The PointOfSale class used as the parent of the created POSForm.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="type">The type of MessageForm to show.</param>
        /// <param name="pause">If the type is Pause, then this is how long to 
        /// show the dialog (in milliseconds).  It cannot be less 
        /// than one.</param>
        /// <exception cref="System.ArgumentException">type is 
        /// MessageFormTypes.Pause and pause is less than one.</exception>
        /// <returns>One of the DialogResult values.</returns>
        public static DialogResult Show(IWin32Window owner, PointOfSale parent, string text, POSMessageFormTypes type, int pause = 0)
        {
            POSMessageForm form = new POSMessageForm(parent, text, ContentAlignment.MiddleCenter, type, pause);
            return DoTheDialog(form, owner);
        }

        public static DialogResult Show(IWin32Window owner, PointOfSale parent, string text, string caption, POSMessageFormTypes type, int pause = 0)
        {
            POSMessageForm form = new POSMessageForm(parent, text, caption, ContentAlignment.MiddleCenter, type, pause);
            return DoTheDialog(form, owner);
        }

        /// <summary>
        /// Shows a single button message dialog with user defined button text.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent">The PointOfSale class used as the parent of the created POSForm.</param>
        /// <param name="text">Message text.</param>
        /// <param name="title">Caption text (if there is a caption).</param>
        /// <param name="color">Should the button be green?</param>
        /// <param name="button1Text">Text for button.</param>
        /// <param name="pause">Milliseconds to show dialog before terminating.  Timeout will return defaultButton.</param>
        /// <returns>Button pressed = 1.</returns>
        public static int ShowCustomOneButton(IWin32Window owner, PointOfSale parent, string text, string title, bool color, string button1Text, int pause = 0)
        {
            POSMessageFormTypes type = POSMessageFormTypes.Custom1Button;

            if (color)
                type = (POSMessageFormTypes)((int)type + (int)POSMessageFormTypes.ColorButtons);

            POSMessageForm form = new POSMessageForm(parent, text, title, type, 1, button1Text, "", "", pause);
            return (int)DoTheDialog(form, owner);
        }

        /// <summary>
        /// Shows a two button message dialog with user defined button text.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent">The PointOfSale class used as the parent of the created POSForm.</param>
        /// <param name="text">Message text.</param>
        /// <param name="title">Caption text (if there is a caption).</param>
        /// <param name="color">Should the buttons be green and red?</param>
        /// <param name="defaultButton">Which button is the default, 1 or 2?</param>
        /// <param name="button1Text">Text for button 1.</param>
        /// <param name="button2Text">Text for button 2.</param>
        /// <param name="pause">Milliseconds to show dialog before terminating.  Timeout will return defaultButton.</param>
        /// <returns>Button pressed = 1 or 2.</returns>
        public static int ShowCustomTwoButton(IWin32Window owner, PointOfSale parent, string text, string title, bool color, int defaultButton, string button1Text, string button2Text, int pause = 0)
        {
            POSMessageFormTypes type = POSMessageFormTypes.Custom2Button;

            if (color)
                type = (POSMessageFormTypes)((int)type + (int)POSMessageFormTypes.ColorButtons);

            POSMessageForm form = new POSMessageForm(parent, text, title, type, defaultButton, button1Text, button2Text, "", pause);
            return (int)DoTheDialog(form, owner);
        }

        /// <summary>
        /// Shows a three button message dialog with user defined button text.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent">The PointOfSale class used as the parent of the created POSForm.</param>
        /// <param name="text">Message text.</param>
        /// <param name="title">Caption text (if there is a caption).</param>
        /// <param name="color">Should the buttons be green, red, and yellow?</param>
        /// <param name="defaultButton">Which button is the default, 1, 2, or 3?</param>
        /// <param name="button1Text">Text for button 1.</param>
        /// <param name="button2Text">Text for button 2.</param>
        /// <param name="button3Text">Text for button 3.</param>
        /// <param name="pause">Milliseconds to show dialog before terminating.  Timeout will return defaultButton.</param>
        /// <returns>Button pressed = 1, 2, or 3.</returns>
        public static int ShowCustomThreeButton(IWin32Window owner, PointOfSale parent, string text, string title, bool color, int defaultButton, string button1Text, string button2Text, string button3Text, int pause = 0)
        {
            POSMessageFormTypes type = POSMessageFormTypes.Custom3Button;

            if (color)
                type = (POSMessageFormTypes)((int)type + (int)POSMessageFormTypes.ColorButtons);

            POSMessageForm form = new POSMessageForm(parent, text, title, type, defaultButton, button1Text, button2Text, button3Text, pause);
            return (int)DoTheDialog(form, owner);
        }

        private static DialogResult DoTheDialog(POSMessageForm form, IWin32Window owner)
        {
            DialogResult result = form.ShowDialog(owner);
            Application.DoEvents();

            form.Dispose();

            return result;
        }

        private void SomethingWasClicked(object sender, EventArgs e)
        {
            m_pauseProgress.Value = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the text for button 1 for a custom message form.
        /// </summary>
        public string Button1Text
        {
            get
            {
                return m_button1Text;
            }

            set
            {
                m_button1Text = value;
            }
        }

        /// <summary>
        /// Gets/sets the text for button 2 for a custom message form.
        /// </summary>
        public string Button2Text
        {
            get
            {
                return m_button2Text;
            }

            set
            {
                m_button2Text = value;
            }
        }

        /// <summary>
        /// Gets/sets the text for button 3 for a custom message form.
        /// </summary>
        public string Button3Text
        {
            get
            {
                return m_button3Text;
            }

            set
            {
                m_button3Text = value;
            }
        }

        /// <summary>
        /// Gets/sets the default button for a cutom message form.
        /// 0 = none
        /// </summary>
        public int DefaultButton
        {
            get
            {
                return m_defaultButton;
            }

            set
            {
                m_defaultButton = value;
            }
        }
        #endregion
    }
}