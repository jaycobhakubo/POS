#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2010 GameTech
// International, Inc.
#endregion

// TTP 50137
// Update bank amount on POS.
// Rally TA7464

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using System.ComponentModel;
using System.Collections.Generic;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form that allows the adjusting of the initial bank amount.
    /// </summary>
    internal partial class AdjustBankForm : POSForm
    {
        #region Constants and Data Types
        // Normal or Wide Sizes
        protected const int NormalOrWideCurrencyNameXBuffer = 89;
        protected const int NormalOrWideCurrencyTotalX = 245;
        protected const int NormalOrWideCurrencyAdjustXBuffer = 253;
        protected const int NormalOrWideCurrencyAdjustImageXBuffer = 245;

        // Compact Sizes
        protected const int CompactCurrencyNameXBuffer = 20;
        protected const int CompactCurrencyTotalX = 172;
        protected const int CompactCurrencyAdjustXBuffer = 180;
        protected const int CompactCurrencyAdjustImageXBuffer = 172;

        protected const int SmallXDiff = 173;
        protected const int SmallYDiff = 144;

        // Shared
        protected const int ScrollDelta = 80;

        protected const int ControlStartY = 32;
        protected const int MaxVisibleY = 353;

        protected const int CurrencyYBuffer = 22;

        protected const int CurrencyNameYBuffer = 5;
        protected readonly Size CurrencyNameSize = new Size(125, 58);
        protected readonly Color CurrencyNameColor = Color.White;
        protected readonly Font CurrencyNameFont = new Font("Tahoma", 12.0f, FontStyle.Bold);

        protected readonly Size CurrencyTotalSize = new Size(206, 29);
        protected readonly Color CurrencyTotalColor = Color.White;
        protected readonly Font CurrencyTotalFont = new Font("Tahoma", 12.0f, FontStyle.Bold);

        protected const int CurrencyAdjustYBuffer = 34;
        protected readonly Color CurrencyAdjustColor = Color.Yellow;
        protected readonly Color CurrencyAdjustBackColor = Color.FromArgb(19, 60, 96);
        protected readonly Font CurrencyAdjustFont = new Font("Tahoma", 12.0f, FontStyle.Bold);
        protected readonly Size CurrencyAdjustSize = new Size(196, 23);
        protected const string CurrencyAdjustName = "m_currency{0}";

        protected Bitmap CurrencyAdjustImage = Resources.TextBack;
        
        protected const int CurrencyAdjustImageYBuffer = 29;
        protected readonly Size CurrencyAdjustImageSize = new Size(212, 29);

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
        protected int m_currencyTotalX;
        protected int m_currencyAdjustXBuffer;
        protected int m_currencyAdjustImageXBuffer;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the AdjustBankForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="bank">The Bank object that needs to be
        /// adjusted.</param>
        /// <exception cref="System.ArgumentNullException">parent, 
        /// displayMode, or bank are a null reference.</exception>
        public AdjustBankForm(PointOfSale parent, DisplayMode displayMode, Bank bank)
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

            if (m_displayMode is NormalDisplayMode)
            {
                m_currencyNameXBuffer = NormalOrWideCurrencyNameXBuffer;
                m_currencyTotalX = NormalOrWideCurrencyTotalX;
                m_currencyAdjustXBuffer = NormalOrWideCurrencyAdjustXBuffer;
                m_currencyAdjustImageXBuffer = NormalOrWideCurrencyAdjustImageXBuffer;
            }
            else if (m_displayMode is WideDisplayMode)
            {
                m_titleLabel.Location = new Point(m_titleLabel.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_titleLabel.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_errorCountLabel.Location = new Point(m_errorCountLabel.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_errorCountLabel.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_button0.Location = new Point(m_button0.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button0.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button1.Location = new Point(m_button1.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button1.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button2.Location = new Point(m_button2.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button2.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button3.Location = new Point(m_button3.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button3.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button4.Location = new Point(m_button4.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button4.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button5.Location = new Point(m_button5.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button5.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button6.Location = new Point(m_button6.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button6.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button7.Location = new Point(m_button7.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button7.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button8.Location = new Point(m_button8.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button8.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_button9.Location = new Point(m_button9.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_button9.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_clearButton.Location = new Point(m_clearButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_clearButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_decimalButton.Location = new Point(m_decimalButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_decimalButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_plusMinusButton.Location = new Point(m_plusMinusButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_plusMinusButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_currencyPanel.Location = new Point(m_currencyPanel.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_currencyPanel.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_upButton.Location = new Point(m_upButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_upButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_downButton.Location = new Point(m_downButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_downButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_okButton.Location = new Point(m_okButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_okButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_cancelButton.Location = new Point(m_cancelButton.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_cancelButton.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_currencyNameXBuffer = NormalOrWideCurrencyNameXBuffer;
                m_currencyTotalX = NormalOrWideCurrencyTotalX;
                m_currencyAdjustXBuffer = NormalOrWideCurrencyAdjustXBuffer;
                m_currencyAdjustImageXBuffer = NormalOrWideCurrencyAdjustImageXBuffer;
            }
            else
            {
                BackgroundImage = Resources.AdjustBankBack800;

                m_titleLabel.Location = new Point(13, 9);
                m_titleLabel.Size = new Size(775, 85);

                m_errorCountLabel.Location = new Point(515, 160);
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
                m_plusMinusButton.Location = new Point(m_plusMinusButton.Location.X - SmallXDiff, m_plusMinusButton.Location.Y - SmallYDiff);

                m_currencyPanel.Location = new Point(16, 101);
                m_currencyPanel.Size = new Size(404, 381);

                m_upButton.Location = new Point(422, 102);
                m_downButton.Location = new Point(422, 428);

                m_okButton.Location = new Point(144, 515);
                m_cancelButton.Location = new Point(494, 515);

                m_currencyNameXBuffer = CompactCurrencyNameXBuffer;
                m_currencyTotalX = CompactCurrencyTotalX;
                m_currencyAdjustXBuffer = CompactCurrencyAdjustXBuffer;
                m_currencyAdjustImageXBuffer = CompactCurrencyAdjustImageXBuffer;
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

                // Create the currency current total.
                Label currencyTotalLabel = new Label();
                currencyTotalLabel.AutoSize = false;
                currencyTotalLabel.Location = new Point(m_currencyTotalX, currY);
                currencyTotalLabel.Size = CurrencyTotalSize;
                currencyTotalLabel.Font = CurrencyTotalFont;
                currencyTotalLabel.ForeColor = CurrencyTotalColor;
                currencyTotalLabel.AutoEllipsis = true;
                currencyTotalLabel.TextAlign = ContentAlignment.MiddleRight;
                currencyTotalLabel.Text = currency.FormatCurrencyString(currency.Total);

                m_currencyPanel.Controls.Add(currencyTotalLabel);

                // Create the currency adjustment text box.
                TextBox currencyAdjustText = new TextBox();
                currencyAdjustText.Location = new Point(m_currencyAdjustXBuffer, currY + CurrencyAdjustYBuffer);
                currencyAdjustText.Font = CurrencyAdjustFont;
                currencyAdjustText.Size = CurrencyAdjustSize;
                currencyAdjustText.ForeColor = CurrencyAdjustColor;
                currencyAdjustText.BackColor = CurrencyAdjustBackColor;
                currencyAdjustText.BorderStyle = BorderStyle.None;
                currencyAdjustText.TextAlign = HorizontalAlignment.Right;
                currencyAdjustText.MaxLength = MaxNumbers;
                currencyAdjustText.TabIndex = tabIndex++;
                currencyAdjustText.GotFocus += TextBoxFocused;
                currencyAdjustText.KeyDown += EnterPressed;
                currencyAdjustText.Validating += NumberValidate;

                // Set the text box's name, text, and tag.
                currencyAdjustText.Name = string.Format(CultureInfo.InvariantCulture, CurrencyAdjustName, currency.ISOCode);
                currencyAdjustText.Tag = currency;
                currencyAdjustText.Text = DefaultValue;

                m_currencyPanel.Controls.Add(currencyAdjustText);

                // Set the focus to the first text box.
                if(!focusSet)
                {
                    m_currentTextBox = currencyAdjustText;
                    focusSet = true;
                }

                // Create the currency adjustment background image.
                ImageLabel currencyAdjustLabel = new ImageLabel(CurrencyAdjustImage);
                currencyAdjustLabel.Location = new Point(m_currencyAdjustImageXBuffer, currY + CurrencyAdjustImageYBuffer);
                currencyAdjustLabel.Size = CurrencyAdjustImageSize;

                m_currencyPanel.Controls.Add(currencyAdjustLabel);

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
                m_currentTextBox.Focus();
                SendKeys.Send(((Button)sender).Text);

                //// Where do they want to insert the new text?
                //int caret = m_currentTextBox.SelectionStart;
                //string text = m_currentTextBox.Text ?? string.Empty;

                //if(m_currentTextBox.SelectionLength > 0)
                //{
                //    text = text.Substring(0, m_currentTextBox.SelectionStart) + text.Substring(m_currentTextBox.SelectionStart + m_currentTextBox.SelectionLength);
                //    caret = m_currentTextBox.SelectionStart;
                //}

                //m_currentTextBox.Text = text.Insert(caret, button.Text).Trim();

                //// Make sure we aren't bigger than the max.
                //if(m_currentTextBox.Text.Length > m_currentTextBox.MaxLength)
                //    m_currentTextBox.Text = m_currentTextBox.Text.Substring(0, m_currentTextBox.MaxLength);

                //// Set the focus back to the text box.
                //m_currentTextBox.Focus();
                //m_currentTextBox.Select(m_currentTextBox.Text.Length, 0);
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
        /// Handles when the +/- button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains 
        /// the event data.</param>
        private void PlusMinusButtonClick(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;

            if(button != null && m_currentTextBox != null)
            {
                // Make sure the text box only contains numbers.
                decimal result;

                if(decimal.TryParse(m_currentTextBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                    m_currentTextBox.Text = (result * decimal.MinusOne).ToString("N", CultureInfo.CurrentCulture);

                // Set the focus back to the text box.
                m_currentTextBox.Focus();
                m_currentTextBox.Select(m_currentTextBox.Text.Length, 0);
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

                if(!decimal.TryParse(textBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
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
                AdjustmentAmounts = new Dictionary<BankCurrency, decimal>();

                // Update the totals of the bank.
                foreach(Control control in m_currencyPanel.Controls)
                {
                    if(control is TextBox)
                    {
                        TextBox textBox = control as TextBox;

                        if(!AdjustmentAmounts.ContainsKey((BankCurrency)textBox.Tag))
                            AdjustmentAmounts.Add((BankCurrency)textBox.Tag, 0M);

                        AdjustmentAmounts[(BankCurrency)textBox.Tag] = Convert.ToDecimal(textBox.Text, CultureInfo.CurrentCulture);
                    }
                }

                ClearControls();
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets a list of the adjustment amounts entered by the user.
        /// </summary>
        public IDictionary<BankCurrency, decimal> AdjustmentAmounts
        {
            get;
            private set;
        }
        #endregion
    }
}