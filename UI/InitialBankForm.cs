#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2010 GameTech
// International, Inc.
#endregion

// Rally TA7464

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form that allows the entering of the initial bank amount.
    /// </summary>
    internal partial class InitialBankForm : POSForm
    {
        #region Constants and Data Types
        // Normal Sizes
        protected const int NormalCurrencyNameXBuffer = 89;
        protected const int NormalCurrencyTotalXBuffer = 253;
        protected const int NormalCurrencyTotalImageXBuffer = 245;

        // Compact Sizes
        protected const int CompactCurrencyNameXBuffer = 20;
        protected const int CompactCurrencyTotalXBuffer = 180;
        protected const int CompactCurrencyTotalImageXBuffer = 172;

        protected const int SmallXDiff = 172;
        protected const int SmallYDiff = 139;

        // Shared
        protected const int ScrollDelta = 80;

        protected const int ControlStartY = 32;
        protected const int MaxVisibleY = 353;

        protected const int CurrencyYBuffer = 22;

        protected const int CurrencyNameYBuffer = 5;
        protected readonly Size CurrencyNameSize = new Size(125, 58);
        protected readonly Color CurrencyNameColor = Color.White;
        protected readonly Font CurrencyNameFont = new Font("Tahoma", 12.0f, FontStyle.Bold);

        protected const int CurrencyTotalYBuffer = 5;
        protected readonly Color CurrencyTotalColor = Color.Yellow;
        protected readonly Color CurrencyTotalBackColor = Color.FromArgb(19, 60, 96);
        protected readonly Font CurrencyTotalFont = new Font("Tahoma", 12.0f, FontStyle.Bold);
        protected readonly Size CurrencyTotalSize = new Size(196, 23);
        protected const string CurrencyTotalName = "m_currency{0}";

        protected Bitmap CurrencyTotalImage = Resources.TextBack;

        protected readonly Size CurrencyTotalImageSize = new Size(212, 29);

        protected readonly string DefaultValue = 0M.ToString("N", CultureInfo.CurrentCulture);
        protected const int MaxNumbers = 11;

        protected readonly Color ErrorColor = Color.Red;
        protected readonly Color NoErrorsColor = Color.Lime;
        #endregion

        #region Member Variables
        protected Bank m_bank;
        protected TextBox m_currentTextBox;
        protected int m_errorCount;
        protected int m_currencyNameXBuffer;
        protected int m_currencyTotalXBuffer;
        protected int m_currencyTotalImageXBuffer;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the InitialBankForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="bank">The Bank object that needs to be
        /// populated.</param>
        /// <exception cref="System.ArgumentNullException">parent,
        /// displayMode, or bank are a null reference.</exception>
        public InitialBankForm(PointOfSale parent, DisplayMode displayMode, Bank bank)
            : base(parent, displayMode)
        {
            if(bank == null)
                throw new ArgumentNullException("bank");

            m_bank = bank;

            InitializeComponent();
            ApplyDisplayMode();
            ArrangeForm();
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            base.ApplyDisplayMode();

            if(m_displayMode is CompactDisplayMode)
            {
                m_panelMain.Size = m_displayMode.FormSize;
                m_panelMain.BackgroundImage = Resources.InitialBankBack800;

                m_titleLabel.Location = new Point(13, 9);
                m_titleLabel.Size = new Size(775, 85);

                m_errorCountLabel.Location = new Point(515, 165);
                m_errorCountLabel.Size = new Size(263, 25);

                m_button0.Location = new Point(m_button0.Location.X - SmallXDiff, m_button0.Location.Y - SmallYDiff);
                m_button1.Location = new Point(m_button1.Location.X - SmallXDiff, m_button1.Location.Y - SmallYDiff);
                m_button2.Location = new Point(m_button2.Location.X - SmallXDiff, m_button2.Location.Y - SmallYDiff);
                m_button3.Location = new Point(m_button3.Location.X - SmallXDiff, m_button3.Location.Y - SmallYDiff);
                m_button4.Location = new Point(m_button4.Location.X - SmallXDiff, m_button4.Location.Y - SmallYDiff);
                m_button5.Location = new Point(m_button5.Location.X - SmallXDiff, m_button5.Location.Y - SmallYDiff);
                m_button6.Location = new Point(m_button6.Location.X - SmallXDiff, m_button6.Location.Y - SmallYDiff);
                m_button7.Location = new Point(m_button7.Location.X - SmallXDiff, m_button7.Location.Y - SmallYDiff);
                m_button8.Location = new Point(m_button8.Location.X - SmallXDiff, m_button8.Location.Y - SmallYDiff);
                m_button9.Location = new Point(m_button9.Location.X - SmallXDiff, m_button9.Location.Y - SmallYDiff);
                m_clearButton.Location = new Point(m_clearButton.Location.X - SmallXDiff, m_clearButton.Location.Y - SmallYDiff);
                m_decimalButton.Location = new Point(m_decimalButton.Location.X - SmallXDiff, m_decimalButton.Location.Y - SmallYDiff);

                m_currencyPanel.Location = new Point(16, 106);
                m_currencyPanel.Size = new Size(404, 381);

                m_upButton.Location = new Point(422, 107);
                m_downButton.Location = new Point(422, 433);

                m_okButton.Location = new Point(330, 515);

                m_currencyNameXBuffer = CompactCurrencyNameXBuffer;
                m_currencyTotalXBuffer = CompactCurrencyTotalXBuffer;
                m_currencyTotalImageXBuffer = CompactCurrencyTotalImageXBuffer;
            }
            else //normal or wide
            {
                if (!IsAtLeastWindows7)
                {
                    this.DrawAsGradient = true;
                    this.DrawBorderOuterEdge = true;
                }

                m_panelMain.Location = new Point(m_displayMode.EdgeAdjustmentForNormalToWideX, m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_currencyNameXBuffer = NormalCurrencyNameXBuffer;
                m_currencyTotalXBuffer = NormalCurrencyTotalXBuffer;
                m_currencyTotalImageXBuffer = NormalCurrencyTotalImageXBuffer;
            }
        }

        /// <summary>
        /// Removes all controls from the currency panel.
        /// </summary>
        protected void ClearControls()
        {
            // Reset focus.
            m_currentTextBox = null;

            // Remove all the controls from the panel and dispose of them.
            foreach(Control control in m_currencyPanel.Controls)
            {
                if(control is TextBox)
                {
                    control.GotFocus -= TextBoxFocused;
                    control.KeyDown -= EnterPressed;
                    control.Validating -= NumberValidate;
                }

                control.Dispose();
            }

            m_currencyPanel.Controls.Clear();
        }

        /// <summary>
        /// Arranges the controls on the form based on bank.
        /// </summary>
        protected void ArrangeForm()
        {
            ClearControls();

            bool focusSet = false;

            int currY = ControlStartY;
            int tabIndex = 1;

            foreach(BankCurrency currency in m_bank.Currencies)
            {
                // Loop through all the currencies and text boxes for them.
                // Create the currency name.
                Label currencyNameLabel = new Label();
                currencyNameLabel.AutoSize = false;
                currencyNameLabel.Location = new Point(m_currencyNameXBuffer, currY + CurrencyNameYBuffer);
                currencyNameLabel.Size = CurrencyNameSize;
                currencyNameLabel.Font = CurrencyNameFont;
                currencyNameLabel.ForeColor = CurrencyNameColor;
                currencyNameLabel.AutoEllipsis = true;
                currencyNameLabel.TextAlign = ContentAlignment.TopRight;
                currencyNameLabel.Text = currency.Name;

                m_currencyPanel.Controls.Add(currencyNameLabel);

                // Create the currency total text box.
                TextBox currencyTotalText = new TextBox();
                currencyTotalText.Location = new Point(m_currencyTotalXBuffer, currY + CurrencyTotalYBuffer);
                currencyTotalText.Font = CurrencyTotalFont;
                currencyTotalText.Size = CurrencyTotalSize;
                currencyTotalText.ForeColor = CurrencyTotalColor;
                currencyTotalText.BackColor = CurrencyTotalBackColor;
                currencyTotalText.BorderStyle = BorderStyle.None;
                currencyTotalText.TextAlign = HorizontalAlignment.Right;
                currencyTotalText.MaxLength = MaxNumbers;
                currencyTotalText.TabIndex = tabIndex++;
                currencyTotalText.GotFocus += TextBoxFocused;
                currencyTotalText.KeyDown += EnterPressed;
                currencyTotalText.Validating += NumberValidate;

                // Set the text box's name, text, and tag.
                currencyTotalText.Name = string.Format(CultureInfo.InvariantCulture, CurrencyTotalName, currency.ISOCode);
                currencyTotalText.Tag = currency;
                currencyTotalText.Text = DefaultValue;

                m_currencyPanel.Controls.Add(currencyTotalText);

                // Set the focus to the first text box.
                if(!focusSet)
                {
                    m_currentTextBox = currencyTotalText;
                    focusSet = true;
                }

                // Create the currency total background image.
                ImageLabel currencyTotalLabel = new ImageLabel(CurrencyTotalImage);
                currencyTotalLabel.Location = new Point(m_currencyTotalImageXBuffer, currY);
                currencyTotalLabel.Size = CurrencyTotalImageSize;

                m_currencyPanel.Controls.Add(currencyTotalLabel);

                currY += currencyNameLabel.Size.Height + CurrencyYBuffer;
            }

            if(currY >= MaxVisibleY)
            {
                m_upButton.Enabled = true;
                m_downButton.Enabled = true;
            }
            else
            {
                m_upButton.Enabled = false;
                m_downButton.Enabled = false;
            }
        }

        /// <summary>
        /// Handles when a total text box gets focus.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void TextBoxFocused(object sender, EventArgs e)
        {
            if(sender is TextBox)
            {
                m_currentTextBox = (TextBox)sender;
            }
        }

        /// <summary>
        /// Handles when the user presses enter while a text box has focus.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A KeyEventArgs object that contains the
        /// event data.</param>
        private void EnterPressed(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if(textBox != null && e.KeyCode == Keys.Enter)
            {
                SelectNextControl(textBox, true, true, true, false);
            }
        }

        /// <summary>
        /// Handles the up button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void UpButtonClick(object sender, EventArgs e)
        {
            int y = m_currencyPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) - ScrollDelta;

            if(y < 0)
                m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, 0);
            else
                m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, y);
        }

        /// <summary>
        /// Handles the down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void DownButtonClick(object sender, EventArgs e)
        {
            int y = m_currencyPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) + ScrollDelta;

            m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, y);
        }

        /// <summary>
        /// Handles a number button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void NumberClick(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;

            if(button != null && m_currentTextBox != null)
            {
                // Where do they want to insert the new text?
                int caret = m_currentTextBox.SelectionStart;
                string text = m_currentTextBox.Text ?? string.Empty;

                if(m_currentTextBox.SelectionLength > 0)
                {
                    text = text.Substring(0, m_currentTextBox.SelectionStart) + text.Substring(m_currentTextBox.SelectionStart + m_currentTextBox.SelectionLength);
                    caret = m_currentTextBox.SelectionStart;
                }

                m_currentTextBox.Text = text.Insert(caret, button.Text).Trim();

                // Make sure we aren't bigger than the max.
                if(m_currentTextBox.Text.Length > m_currentTextBox.MaxLength)
                    m_currentTextBox.Text = m_currentTextBox.Text.Substring(0, m_currentTextBox.MaxLength);

                // Set the focus back to the text box.
                m_currentTextBox.Focus();
                m_currentTextBox.Select(m_currentTextBox.Text.Length, 0);
            }
        }

        /// <summary>
        /// Handles the clear button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ClearClick(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;

            if(button != null && m_currentTextBox != null)
            {
                m_currentTextBox.Clear();

                // Set the focus back to the text box.
                m_currentTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles the TextBoxes' validate event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A CancelEventArgs object that contains the
        /// event data.</param>
        private void NumberValidate(object sender, CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Is the sender a text box?
            if(textBox != null)
            {
                // Make sure the text box only contains numbers.
                decimal result;

                if(!decimal.TryParse(textBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out result) || result < 0M)
                {
                    e.Cancel = true;
                    m_errorCount++;
                }
            }
        }

        /// <summary>
        /// Updates the error count label.
        /// </summary>
        protected void UpdateErrorCount()
        {
            if(m_errorCount > 0)
            {
                m_errorCountLabel.ForeColor = ErrorColor;
                m_errorCountLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.ErrorsDetected, m_errorCount);
            }
            else
            {
                m_errorCountLabel.ForeColor = NoErrorsColor;
                m_errorCountLabel.Text = Resources.NoErrorsDetected;
            }
        }

        /// <summary>
        /// Handles the ok button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void OkClick(object sender, EventArgs e)
        {
            m_errorCount = 0;

            bool valid = ValidateChildren(ValidationConstraints.Enabled | ValidationConstraints.Visible);

            UpdateErrorCount();

            if(valid)
            {
                // Update the totals of the bank.
                foreach(Control control in m_currencyPanel.Controls)
                {
                    if(control is TextBox)
                    {
                        TextBox textBox = control as TextBox;

                        ((BankCurrency)textBox.Tag).Total = decimal.Parse(textBox.Text, CultureInfo.CurrentCulture);
                    }
                }

                ClearControls();
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        #endregion
    }
}