#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc
#endregion

//US4436: Close a bank from the POS
//US4767: POS > Close Bank: Add onscreen number key pad
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Modules.Shared;

// ReSharper disable once CheckNamespace
namespace GTI.Modules.POS.UI
{
    public partial class DenomControl : UserControl
    {
        #region Local 
        //US4767
        private bool m_hasFocus;
        private readonly Color m_selectedBackgroundColor = Color.FromArgb(255, 39, 74, 117);
        private readonly Color m_unselectedBackgroundColor = Color.FromArgb(255, 95, 87, 83);
        #endregion

        #region contructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DenomControl"/> class.
        /// </summary>
        /// <param name="denom">The denom.</param>
        /// <param name="currencyName">Name of the currency.</param>
        public DenomControl(Denomination denom, string currencyName)
        {
            InitializeComponent();

            //init local variables and UI labels etc
            Denom = denom;
            NameLabel.Text = denom.Name;
            TypeLabel.Text = denom.Type.ToString();
            ValueLabel.Text = string.Format("{0:C}", denom.Value);
            CountTextBox.Text = denom.Count.ToString();
            TotalLabel.Text = string.Format("{0:C}", denom.Value * denom.Count);
            CurrencyName = currencyName;
            m_hasFocus = false;
        }
        #endregion

        #region events
        /// <summary>
        /// Occurs when [total changed event].
        /// </summary>
        public event EventHandler<EventArgs> TotalChangedEvent;

        public event EventHandler<EventArgs> EnterPressEvent;

        public event EventHandler<EventArgs> FocusEvent;
        #endregion
        
        #region properties
        /// <summary>
        /// Gets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public decimal Total
        {
            get
            {
                decimal valueAmount;
                int count;
                if (!decimal.TryParse(ValueLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out valueAmount))
                {
                    return 0m;
                }
                if (!int.TryParse(CountTextBox.Text, out count))
                {
                    return 0m;
                }

                return valueAmount * count;

            }
        }

        /// <summary>
        /// Gets the name of the currency.
        /// </summary>
        /// <value>
        /// The name of the currency.
        /// </value>
        public string CurrencyName { get; private set; }

        /// <summary>
        /// Gets the denom.
        /// </summary>
        /// <value>
        /// The denom.
        /// </value>
        public Denomination Denom { get; private set; }
        #endregion

        #region methods

        /// <summary>
        /// Users the control clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void UserControlClicked(object sender, EventArgs e)
        {
            //have to raise the onclick so the user control gets focus
            //OnClick(e);
            ToggleSelected();

            if (m_hasFocus)
            {
                CountTextBox.SelectAll();
                CountTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles the Click event of the IncrementCountButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void IncrementCountButton_Click(object sender, EventArgs e)
        {
            UpdateSelected(true);
            
            decimal valueAmount;

            decimal.TryParse(ValueLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out valueAmount);

            int count;

            //parse the count
            if (!int.TryParse(CountTextBox.Text, out count))
                return;

            if (++count * valueAmount > 9999999)
                return;

            //increments and set the count
            CountTextBox.Text = count.ToString();
            Denom.Count = count;

            //update total
            UpdateTotal();
        }

        /// <summary>
        /// Handles the Click event of the DecrementCountButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DecrementCountButton_Click(object sender, EventArgs e)
        {
            UpdateSelected(true);
            int count;

            //parse the count
            if (!int.TryParse(CountTextBox.Text, out count))
            {
                return;
            }

            if (count <= 0)
            {
                return;
            }

            //decrement and set the count
            count--;
            CountTextBox.Text = count.ToString();
            Denom.Count = count;

            //update total
            UpdateTotal();
        }

        /// <summary>
        /// Handles the TextChanged event of the CountTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CountTextBox_TextChanged(object sender, EventArgs e)
        {
            //update totals
            if (string.IsNullOrEmpty(CountTextBox.Text))
            {
                return;
            }

            int count;
            int.TryParse(CountTextBox.Text, out count);
            Denom.Count = count;
            UpdateTotal();
        }

        /// <summary>
        /// Handles the KeyPress event of the CountTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> instance containing the event data.</param>
        private void CountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Code to only allow numeric values in the textbox
            if (!(char.IsNumber(e.KeyChar) || 
                (e.KeyChar == (char)Keys.Back) || 
                (e.KeyChar == (char)Keys.Delete) ||
                (e.KeyChar == (char)Keys.Enter)) ||
                (e.KeyChar == '.'))
            {
                e.Handled = true;
                return;
            }

            if (char.IsNumber(e.KeyChar))
            {
                decimal valueAmount;
                
                if (!decimal.TryParse(ValueLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out valueAmount))
                {
                    e.Handled = true;
                    return;
                }

                var stringValue = CountTextBox.Text;

                if (CountTextBox.SelectionStart > stringValue.Length)
                    stringValue += e.KeyChar;
                else if (CountTextBox.SelectionLength > stringValue.Length)
                    stringValue = stringValue.Substring(0, CountTextBox.SelectionStart) + e.KeyChar;
                else
                    stringValue = stringValue.Substring(0, CountTextBox.SelectionStart) + e.KeyChar + stringValue.Substring(CountTextBox.SelectionStart + CountTextBox.SelectionLength);

                var count = 0;

                if (!string.IsNullOrEmpty(stringValue))
                {
                    if (!int.TryParse(stringValue, out count))
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if (count * valueAmount > 9999999)
                {
                    e.Handled = true;
                    return;
                }

                if (count * valueAmount < 0)
                {
                    e.Handled = true;
                }
            }
            else if (e.KeyChar == (char) Keys.Enter)
            {
                //Enter pressed. Raise event
                //Select the next denom 
                var handler = EnterPressEvent;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Leave event of the CountTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CountTextBox_Leave(object sender, EventArgs e)
        {
            //update totals
            if (string.IsNullOrEmpty(CountTextBox.Text))
            {
                CountTextBox.Text = 0.ToString();
                Denom.Count = 0;
                UpdateTotal();
            }

            //var handler = TextBoxFocusEvent;

            //if (handler != null)
            //{
            //    handler(null, EventArgs.Empty);
            //}
        }
        //US4767
        /// <summary>
        /// Handles the Enter event of the CountTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CountTextBox_Enter(object sender, EventArgs e)
        {
            if(!m_hasFocus)
                UpdateSelected(true);
        }

        /// <summary>
        /// Updates the total.
        /// </summary>
        private void UpdateTotal()
        {
            //update total
            TotalLabel.Text = string.Format("{0:C}", Total);

            var handler = TotalChangedEvent;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the count textbox focus.
        /// </summary>
        public void SetCountTextboxFocus()
        {
            CountTextBox.Focus();
        }

        //US4767
        /// <summary>
        /// Toggles the focus.
        /// </summary>
        private void ToggleSelected()
        {
            UpdateSelected(!m_hasFocus);
        }
        //US4767
        /// <summary>
        /// Updates the selected.
        /// </summary>
        /// <param name="hasFocus">if set to <c>true</c> [has focus].</param>
        private void UpdateSelected(bool hasFocus)
        {
            //already has focus, do not need to update
            if (m_hasFocus == hasFocus)
            {
                return;
            }

            UpdateSelectedBackground(hasFocus);

            //raise event focus
            var handler = FocusEvent;
            if (handler != null)
            {
                handler(hasFocus ? this : null, EventArgs.Empty);
            }
        }
        //US4767
        /// <summary>
        /// Updates the selected background.
        /// </summary>
        /// <param name="hasFocus">if set to <c>true</c> [has focus].</param>
        public void UpdateSelectedBackground(bool hasFocus)
        {
            //update background 
            BackColor = hasFocus ? m_selectedBackgroundColor : m_unselectedBackgroundColor;

            //update flag
            m_hasFocus = hasFocus;
        }

        #endregion
    }
}
