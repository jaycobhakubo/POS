#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2009 GameTech
// International, Inc.
#endregion

// Rally TA5748

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using System.Collections.Generic;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form used to collect play with paper pack information.
    /// </summary>
    internal partial class PlayWithPaperForm : POSForm
    {
        #region Constants and Data Types
        protected const int ScrollDelta = 100;

        protected const int ControlStartX = 6;
        protected const int ControlStartY = 6;
        protected const int MaxVisibleY = 400;

        protected readonly Size PackageNameSize = new Size(385, 20);
        protected readonly Color PackageNameColor = Color.Yellow;
        protected const int PackageNameYBuffer = 4;

        protected const int ProductYBuffer = 5;

        protected const int ProductNameXBuffer = 12;
        protected readonly Size ProductNameSize = new Size(205, 25);
        protected readonly Color ProductNameColor = Color.White;
        protected readonly Font ProductNameFont = new Font("Tahoma", 9.0f, FontStyle.Bold);

        protected const int ProductTextXBuffer = 225;
        // Get an instance in case it returns different images each time.
        protected Bitmap ProductTextImage = Resources.TextBack;
        protected readonly Size ProductTextSize = new Size(135, 25);

        protected const string ProductStartNamePrefix = "m_startNumber";
        protected const string ProductStartNameDelim = "_";
        protected const string ProductStartSuffix = "{0}" + ProductStartNameDelim + "{1}" + ProductStartNameDelim + "{2}";
        protected const string ProductStartName = ProductStartNamePrefix + ProductStartSuffix;
        protected const int ProductStartXBuffer = 233;
        protected const int ProductStartYBuffer = 3;
        protected readonly Color ProductStartColor = Color.Yellow;
        protected readonly Color ProductStartBackColor = Color.FromArgb(19, 60, 96);
        protected readonly Font ProductStartFont = new Font("Tahoma", 12.0f, FontStyle.Bold);
        protected readonly Size ProductStartSize = new Size(120, 20);

        protected const string ProductErrorNamePrefix = "m_startError";
        protected const string ProductErrorName = ProductErrorNamePrefix + ProductStartSuffix;
        protected const int ErrorYBuffer = 5;
        protected const int ErrorXBuffer = 12;
        protected readonly Color ErrorColor = Color.Red;
        protected readonly Font ErrorFont = new Font("Tahoma", 9.0f, FontStyle.Bold);
        protected readonly Size ErrorSize = new Size(348, 15);
        protected readonly Color NoErrorsColor = Color.Lime;

        /// <summary>
        /// The different types of problems that can occur with start numbers.
        /// </summary>
        protected enum StartNumberStatus
        {
            Ok = 0,
            InvalidNumber = -61,
            DuplicateNumber = -72,
            OutsideOfRange = -80
        }
        #endregion

        #region Member Variables
        protected Dictionary<ProductStartNumbers, List<TextBox>> m_textBoxes = new Dictionary<ProductStartNumbers, List<TextBox>>();
        protected TextBox m_currentTextBox;
        protected int m_errorCount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PlayWithPaperForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public PlayWithPaperForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
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

            // This is a special dialog, so override the default size.
            Size = m_displayMode.LargeDialogSize;
        }

        /// <summary>
        /// Removes all controls from the product panel.
        /// </summary>
        protected void ClearControls()
        {
            // Reset focus.
            m_currentTextBox = null;

            // Remove all the controls from the dictionary.
            foreach(KeyValuePair<ProductStartNumbers, List<TextBox>> pair in m_textBoxes)
            {
                foreach(TextBox textBox in pair.Value)
                {
                    textBox.Tag = null;
                }

                pair.Value.Clear();
            }

            m_textBoxes.Clear();

            // Remove all the controls from the panel and dispose of them.
            foreach(Control control in m_productPanel.Controls)
            {
                if(control is TextBox)
                {
                    control.GotFocus -= TextBoxFocused;
                    control.KeyDown -= EnterPressed;
                    control.Validating -= StartNumberValidate;
                }

                control.Dispose();
            }

            m_productPanel.Controls.Clear();
        }

        /// <summary>
        /// Arranges the controls on the form based on sale.
        /// </summary>
        protected void ArrangeForm()
        {
            ClearControls();

            bool focusSet = false;
            int currY = ControlStartY;
            int tabIndex = 1;
            int count = 0;

            if(m_parent.CurrentSale != null)
            {
                // Loop through all products and create controls for start
                // number entry.
                SaleItem[] items = m_parent.CurrentSale.GetItems();

                if(items != null)
                {
                    foreach(SaleItem item in items)
                    {
                        if(item.IsPackageItem && item.Package.HasElectronicBingo)
                        {
                            for(int packageQty = 0; packageQty < item.Quantity; packageQty++)
                            {
                                // Create the package name.
                                Label packageNameLabel = new Label();
                                packageNameLabel.AutoSize = false;
                                packageNameLabel.Location = new Point(ControlStartX, currY);
                                packageNameLabel.Size = PackageNameSize;
                                packageNameLabel.ForeColor = PackageNameColor;
                                packageNameLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.PackageNameAndSession, item.Package.DisplayText.Trim(), item.Session);

                                m_productPanel.Controls.Add(packageNameLabel);

                                currY += packageNameLabel.Size.Height + PackageNameYBuffer;

                                foreach(Product product in item.Package.GetProducts())
                                {
                                    ElectronicBingoProduct electProd = product as ElectronicBingoProduct;

                                    // Is this an electronic bingo product that is not CBB?
                                    if(electProd != null && electProd.GameType != GameType.CrystalBall && electProd.GameType != GameType.PickYurPlatter)
                                    {
                                        for(int productQty = 0; productQty < electProd.Quantity; productQty++)
                                        {
                                            // Create the product name.
                                            Label productNameLabel = new Label();
                                            productNameLabel.AutoSize = false;
                                            productNameLabel.Location = new Point(ProductNameXBuffer, currY);
                                            productNameLabel.Size = ProductNameSize;
                                            productNameLabel.Font = ProductNameFont;
                                            productNameLabel.ForeColor = ProductNameColor;
                                            productNameLabel.AutoEllipsis = true;
                                            productNameLabel.TextAlign = ContentAlignment.MiddleRight;
                                            productNameLabel.Text = electProd.Name + ":";

                                            m_productPanel.Controls.Add(productNameLabel);
                                            
                                            // Create the product start number text box.
                                            TextBox productStartText = new TextBox();
                                            productStartText.Location = new Point(ProductStartXBuffer, currY + ProductStartYBuffer);
                                            productStartText.Font = ProductStartFont;
                                            productStartText.Size = ProductStartSize;
                                            productStartText.ForeColor = ProductStartColor;
                                            productStartText.BackColor = ProductStartBackColor;
                                            productStartText.BorderStyle = BorderStyle.None;
                                            productStartText.MaxLength = StringSizes.MaxStartNumberLength;
                                            productStartText.TabIndex = tabIndex++;
                                            productStartText.GotFocus += TextBoxFocused;
                                            productStartText.KeyDown += EnterPressed;
                                            productStartText.Validating += StartNumberValidate;

                                            // Add the text box to the dictionary.
                                            ProductStartNumbers prodStartNums = new ProductStartNumbers() { SessionPlayedId = item.Session.SessionPlayedId, ProductId = electProd.Id, StartNumbers = new List<StartNumber>() };

                                            if(!m_textBoxes.ContainsKey(prodStartNums))
                                                m_textBoxes.Add(prodStartNums, new List<TextBox>());

                                            m_textBoxes[prodStartNums].Add(productStartText);

                                            // Set the text box's name and tag.
                                            productStartText.Name = string.Format(CultureInfo.InvariantCulture, ProductStartName, prodStartNums.SessionPlayedId, prodStartNums.ProductId, m_textBoxes[prodStartNums].Count - 1);
                                            productStartText.Tag = prodStartNums;

                                            m_productPanel.Controls.Add(productStartText);

                                            // Set the focus to the first text box.
                                            if(!focusSet)
                                            {
                                                m_currentTextBox = productStartText;
                                                focusSet = true;
                                            }

                                            // Create the product start number background image.
                                            ImageLabel productTextLabel = new ImageLabel(ProductTextImage);
                                            productTextLabel.Location = new Point(ProductTextXBuffer, currY);
                                            productTextLabel.Size = ProductTextSize;

                                            m_productPanel.Controls.Add(productTextLabel);

                                            currY += productNameLabel.Size.Height + ProductYBuffer;

                                            // Create the error label.
                                            Label productErrorLabel = new Label();
                                            productErrorLabel.AutoSize = false;
                                            productErrorLabel.Name = string.Format(CultureInfo.InvariantCulture, ProductErrorName, prodStartNums.SessionPlayedId, prodStartNums.ProductId, m_textBoxes[prodStartNums].Count - 1);
                                            productErrorLabel.Location = new Point(ErrorXBuffer, currY);
                                            productErrorLabel.Size = ErrorSize;
                                            productErrorLabel.Font = ErrorFont;
                                            productErrorLabel.ForeColor = ErrorColor;
                                            productErrorLabel.TextAlign = ContentAlignment.MiddleRight;

                                            m_productPanel.Controls.Add(productErrorLabel);

                                            currY += productErrorLabel.Size.Height + ErrorYBuffer;

                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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

            m_requiredCountLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.RequiredStartNumbers, count);
        }

        /// <summary>
        /// Returns the companion error label to the specified text box.
        /// </summary>
        /// <param name="textBox">The text box to use to find its companion
        /// error label.</param>
        /// <returns>The Label instance or null if none was found.</returns>
        protected Label FindErrorLabel(TextBox textBox)
        {
            Label errorLabel = null;

            if(textBox != null)
            {
                string[] textBoxNameParts = textBox.Name.Split(new string[] {ProductStartNameDelim}, StringSplitOptions.None);

                if(textBoxNameParts != null && textBoxNameParts.Length > 3)
                {
                    Control[] controls = m_productPanel.Controls.Find(string.Format(CultureInfo.InvariantCulture, ProductErrorName, ((ProductStartNumbers)textBox.Tag).SessionPlayedId, ((ProductStartNumbers)textBox.Tag).ProductId, textBoxNameParts[3]), false);

                    if(controls != null && controls.Length > 0 && controls[0] is Label)
                    {
                        errorLabel = (Label)controls[0];
                    }
                }
            }

            return errorLabel;
        }

        /// <summary>
        /// Gets a list of ProductStartNumbers from the form.
        /// </summary>
        /// <returns>A list of ProductStartNumbers to form is currently
        /// displaying.</returns>
        protected IList<ProductStartNumbers> GetStartNumbers()
        {
            List<ProductStartNumbers> products = new List<ProductStartNumbers>();

            foreach(KeyValuePair<ProductStartNumbers, List<TextBox>> pair in m_textBoxes)
            {
                ProductStartNumbers productStartNums = new ProductStartNumbers();
                productStartNums.StartNumbers = new List<StartNumber>();

                productStartNums.SessionPlayedId = pair.Key.SessionPlayedId;
                productStartNums.ProductId = pair.Key.ProductId;

                foreach(TextBox textBox in pair.Value)
                {
                    StartNumber num = new StartNumber();
                    int result;

                    if(int.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
                        num.Number = result;

                    productStartNums.StartNumbers.Add(num);
                }

                products.Add(productStartNums);
            }

            return products;
        }

        /// <summary>
        /// Displays a list of start numbers (and possibly errors) on the form.
        /// </summary>
        /// <param name="products">A list of ProductStartNumbers to display on
        /// the form.</param>
        /// <returns>true if an error was found; otherwise false.</returns>
        private bool SetStartNumbers(IList<ProductStartNumbers> products)
        {
            bool foundError = false;
            TextBox firstErrorBox = null;

            if(products != null && products.Count > 0)
            {
                foreach(ProductStartNumbers product in products)
                {
                    if(product.StartNumbers.Count > 0)
                    {
                        try
                        {
                            int currentNumber = 0;

                            // Look for all controls related to this session/product.
                            if(m_textBoxes.ContainsKey(product))
                            {
                                for(int x = 0; x < m_textBoxes[product].Count; x++)
                                {
                                    StartNumber startNum = product.StartNumbers[currentNumber];

                                    // Display the number, if applicable.
                                    if(startNum.Number != 0)
                                        m_textBoxes[product][x].Text = startNum.Number.ToString(CultureInfo.CurrentCulture);
                                    else
                                        m_textBoxes[product][x].Text = null;

                                    // Display the error, if applicable.
                                    Label errorLabel = FindErrorLabel(m_textBoxes[product][x]);

                                    if(errorLabel != null)
                                    {
                                        switch((StartNumberStatus)startNum.Status)
                                        {
                                            case StartNumberStatus.Ok:
                                                errorLabel.Text = null;
                                                break;

                                            case StartNumberStatus.InvalidNumber:
                                                errorLabel.Text = Resources.StartNumberInvalidCard;
                                                foundError = true;
                                                m_errorCount++;
                                                break;

                                            case StartNumberStatus.DuplicateNumber:
                                                errorLabel.Text = Resources.StartNumberDuplicate;
                                                foundError = true;
                                                m_errorCount++;
                                                break;

                                            case StartNumberStatus.OutsideOfRange:
                                                errorLabel.Text = Resources.StartNumberOutside;
                                                foundError = true;
                                                m_errorCount++;
                                                break;

                                            default:
                                                errorLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.StartNumberUnknownError, startNum.Status);
                                                foundError = true;
                                                m_errorCount++;
                                                break;
                                        }
                                    }

                                    // Is this the first error found?
                                    if((StartNumberStatus)startNum.Status != StartNumberStatus.Ok && firstErrorBox == null)
                                        firstErrorBox = m_textBoxes[product][x];

                                    currentNumber++;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            POSMessageForm.Show(this, m_parent, ex.Message);
                        }
                    }
                }
            }

            if(foundError && firstErrorBox != null)
            {
                firstErrorBox.Focus();
                firstErrorBox.SelectAll();
            }
            else if(!foundError && m_currentTextBox != null)
                m_currentTextBox.SelectAll();

            return foundError;
        }

        /// <summary>
        /// Checks to see if the last async. operation returned an exception and 
        /// show a message box if necessary.
        /// </summary>
        /// <returns>true if there was an exception; otherwise false.</returns>
        private bool CheckForError()
        {
            if(m_parent.LastAsyncException != null)
            {
                if(m_parent.LastAsyncException is ServerCommException)
                    m_parent.ServerCommFailed();
                else
                    POSMessageForm.Show(this, m_parent, m_parent.LastAsyncException.Message);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Raises the Shown event.
        /// </summary>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                m_parent.MagCardReader.EndReading();
                m_parent.CanUpdateMenus = false;

                m_parent.StartGetStartNumbers(GetStartNumbers());
                m_parent.ShowWaitForm(this); // Block until we are done.

                if(!CheckForError())
                {
                    SetStartNumbers(m_parent.LastStartNumbers);
                }
                
                m_parent.CanUpdateMenus = true;
                m_parent.MagCardReader.BeginReading();
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed to get the start numbers: " + ex.Message, LoggerLevel.Severe);
                POSMessageForm.Show(this, m_parent, string.Format(CultureInfo.CurrentCulture, Resources.GetStartNumbersFailed, ex.Message));
            }   
        }

        /// <summary>
        /// Handles when a start number text box gets focus.
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
            int y = m_productPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) - ScrollDelta;

            if(y < 0)
                m_productPanel.AutoScrollPosition = new Point(m_productPanel.AutoScrollPosition.X, 0);
            else
                m_productPanel.AutoScrollPosition = new Point(m_productPanel.AutoScrollPosition.X, y);
        }

        /// <summary>
        /// Handles the down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void DownButtonClick(object sender, EventArgs e)
        {
            int y = m_productPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) + ScrollDelta;

            m_productPanel.AutoScrollPosition = new Point(m_productPanel.AutoScrollPosition.X, y);
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
        /// Handles the start number TextBoxs' validate event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A CancelEventArgs object that contains the
        /// event data.</param>
        private void StartNumberValidate(object sender, CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Is the sender a text box?
            if(textBox != null)
            {
                Label errorLabel = FindErrorLabel(textBox);

                if(errorLabel != null)
                {
                    // Make sure the text box only contains numbers.
                    int result;

                    if(!int.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
                    {
                        e.Cancel = true;

                        // Set the error on which ever textbox we are validating.
                        errorLabel.Text = Resources.StartNumberInvalid;

                        m_errorCount++;
                    }
                    else
                    {
                        errorLabel.Text = string.Empty;
                    }
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
                m_errorCountLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.ErrorsDetected, m_errorCount); // Rally TA7464
            }
            else
            {
                m_errorCountLabel.ForeColor = NoErrorsColor;
                m_errorCountLabel.Text = Resources.NoErrorsDetected; // Rally TA7464
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

            if(!valid)
                return;

            try
            {
                m_parent.MagCardReader.EndReading();
                m_parent.CanUpdateMenus = false;

                m_parent.StartCheckStartNumbers(GetStartNumbers());
                m_parent.ShowWaitForm(this); // Block until we are done.

                if(!CheckForError())
                {
                    // Reset the errors.
                    m_errorCount = 0;

                    // Display the new numbers and check for errors.
                    if(!SetStartNumbers(m_parent.LastStartNumbers))
                    {
                        // Everything was okay, so we can save.
                        m_parent.CurrentSale.ClearStartNumbers();
                        m_parent.CurrentSale.SetStartNumbers(GetStartNumbers());
                        ClearControls();
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                        UpdateErrorCount();
                }

                m_parent.CanUpdateMenus = true;
                m_parent.MagCardReader.BeginReading();
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed to check the start numbers: " + ex.Message, LoggerLevel.Severe);
                POSMessageForm.Show(this, m_parent, string.Format(CultureInfo.CurrentCulture, Resources.CheckStartNumbersFailed, ex.Message));
            }   
        }
        #endregion
    }

    /// <summary>
    /// A subclass of panel with no scroll bars.
    /// </summary>
    internal class NoScrollBarsPanel : GTI.Controls.EliteGradientPanel
    {
        #region Win32 Interop Declarations
        private const int SB_BOTH = 3;
        private const int WM_NCCALCSIZE = 0x0083;

        /// <summary>
        /// Shows or hides the specified scroll bar.
        /// </summary>
        /// <param name="hWnd">Handle to a scroll bar control or a window with
        /// a standard scroll bar, depending on the value of the wBar
        /// parameter.</param>
        /// <param name="wBar">Specifies the scroll bar(s) to be shown or
        /// hidden.</param>
        /// <param name="bShow">Specifies whether the scroll bar is shown or
        /// hidden. If this parameter is TRUE, the scroll bar is shown;
        /// otherwise, it is hidden.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If
        /// the function fails, the return value is zero.</returns>
        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        #endregion

        #region Member Methods
        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows message to process.</param>
        protected override void WndProc(ref Message m)
        {
            switch(m.Msg)
            {
                case WM_NCCALCSIZE:
                    // Suppress the scrollbars.
                    ShowScrollBar(m.HWnd, SB_BOTH, false);
                    break;
            }

            base.WndProc(ref m);
        }
        #endregion
    }
}
