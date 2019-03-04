using System;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

//US4397: (US1592) POS: B3 Hand Pay
//US4404: (US1592) POS: B3 Jackpot Payment
//US4395: (US1592) POS: B3 Unlock Accounts
//US4338: (US1592) POS: Redeem B3
//DE13137: Error found in US4338: (US1592) POS: Redeem B3 > Receipt does not include Wins
//DE13546: B3 > POS: Displays hand pay amount when taxable payout and handpay are the same

namespace GTI.Modules.POS.UI
{
    internal partial class B3RetrieveAccountForm : POSForm
    {
        private TransactionState m_trasactionState;
        private B3State m_serverState;
        private int m_account;
        private bool m_isKeypadEnabled;

        public B3RetrieveAccountForm(PointOfSale parent, DisplayMode displayMode, bool isDoubleAccount)
            : base(parent, displayMode)
        {
            InitializeComponent();
            UpdateUiState(B3State.RetrieveAccount);
            RetrieveAccountButton.Focus();
            HideHandPayTaxableAmounts();
            IsDoubleAccount = isDoubleAccount;

            //hide win credits & total 
            if (!IsDoubleAccount)
            {
                WinCreditLabel.Visible = false;
                WinCreditBackground.Visible = false;
                WinCreditTitle.Visible = false;
                TotalBackground.Visible = false;
                TotalLabel.Visible = false;
                TotalTitle.Visible = false;
            }
        }

        #region Enum

        private enum B3State
        {
            Inactive,
            Active,
            Cashout,
            Redeem,
            Expired,
            Unlock,
            Undefined,
            RetrieveAccount, //used for UI only 
            Cancel //used for UI only 
        }

        private enum TransactionState
        {
            Complete,
            Pending,
        }

        #endregion

        #region Properties

        public bool IsDoubleAccount { get; private set; } //DE13137

        public decimal TaxableAmount { get; private set; }

        public decimal Credits { get; private set; } //DE13137

        public decimal WinCredit { get; private set; } //DE13137

        public decimal Total { get; private set; }
        
        public bool IsTaxableAmount { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parses the account number.
        /// </summary>
        /// <returns></returns>
        private int ParseAccountNumber()
        {
            if (string.IsNullOrEmpty(AccountNumberLabel.Text))
            {
                return 0;
            }

            int account;
            if (!int.TryParse(AccountNumberLabel.Text, out account))
            {
                ClearUiAccount();
                return 0;
            }

            return account;
        }

        /// <summary>
        /// Updates the state of the UI.
        /// </summary>
        /// <param name="b3State">State of the b3.</param>
        private void UpdateUiState(B3State b3State)
        {
            switch (b3State)
            {
                case B3State.RetrieveAccount:
                    RetrieveAccountButton.Enabled = true;
                    RedeemButton.Enabled = false;
                    UnlockButton.Enabled = false;
                    CancelButton.Enabled = false;
                    EnableKeypad();
                    break;
                case B3State.Redeem:
                    RetrieveAccountButton.Enabled = false;
                    RedeemButton.Enabled = true;
                    UnlockButton.Enabled = false;
                    CancelButton.Enabled = true;
                    DisableKeypad();
                    break;
                case B3State.Unlock:
                    RetrieveAccountButton.Enabled = false;
                    RedeemButton.Enabled = false;
                    UnlockButton.Enabled = true;
                    CancelButton.Enabled = true;
                    DisableKeypad();
                    break;
                case B3State.Cancel: 
                    RetrieveAccountButton.Enabled = false;
                    RedeemButton.Enabled = false;
                    UnlockButton.Enabled = false;
                    CancelButton.Enabled = true;
                    DisableKeypad();
                    break;
            }
        }

        /// <summary>
        /// Clears the UI account.
        /// </summary>
        private void ClearUiAccount()
        {
            //clear account
            m_account = 0;
            AccountNumberLabel.Text = string.Empty;
            StatusLabel.Text = string.Empty;

            TaxableAmount = 0;
            Credits = 0;
            WinCredit = 0;
            Total = 0;


            UpdateUiAmounts();
            HideHandPayTaxableAmounts();

            UpdateUiState(B3State.RetrieveAccount);

        }

        /// <summary>
        /// Updates the UI amounts.
        /// </summary>
        private void UpdateUiAmounts()
        {
            CreditLabel.Text = String.Format("{0:C}", Credits);
            WinCreditLabel.Text = String.Format("{0:C}", WinCredit);
            TotalLabel.Text = String.Format("{0:C}", Total);
            HandPayLabel.Text = String.Format("{0:C}", TaxableAmount);
        }

        /// <summary>
        /// Hides the hand pay taxable amounts.
        /// </summary>
        private void HideHandPayTaxableAmounts()
        {
            //hide UI controls
            HandPayTitleLabel.Visible = false;
            TaxablePayTitleLabel.Visible = false;
            HandPayLabel.Visible = false;
            HandPayBackGround.Visible = false;
        }

        /// <summary>
        /// Disables the keypad.
        /// </summary>
        private void DisableKeypad()
        {
            Button0.Enabled = false;
            Button1.Enabled = false;
            Button2.Enabled = false;
            Button3.Enabled = false;
            Button4.Enabled = false;
            Button5.Enabled = false;
            Button6.Enabled = false;
            Button7.Enabled = false;
            Button8.Enabled = false;
            Button9.Enabled = false;
            ButtonBack.Enabled = false;
            ButtonClear.Enabled = false;
            m_isKeypadEnabled = false;
        }

        /// <summary>
        /// Enables the keypad.
        /// </summary>
        private void EnableKeypad()
        {
            Button0.Enabled = true;
            Button1.Enabled = true;
            Button2.Enabled = true;
            Button3.Enabled = true;
            Button4.Enabled = true;
            Button5.Enabled = true;
            Button6.Enabled = true;
            Button7.Enabled = true;
            Button8.Enabled = true;
            Button9.Enabled = true;
            ButtonBack.Enabled = true;
            ButtonClear.Enabled = true;
            m_isKeypadEnabled = true;
        }

        /// <summary>
        /// Appends the numeric value to account label.
        /// </summary>
        /// <param name="number">The number.</param>
        private void AppendNumeric(int number)
        {
            if (!string.IsNullOrEmpty(StatusLabel.Text))
            {
                StatusLabel.Text = string.Empty;
            }

            if (AccountNumberLabel.Text.Length >= 8)
            {
                return;
            }

            AccountNumberLabel.Text += number;
        }

        /// <summary>
        /// Removes numeric value from the account label.
        /// </summary>
        private void RemoveNumeric()
        {
            if (!string.IsNullOrEmpty(StatusLabel.Text))
            {
                StatusLabel.Text = string.Empty;
            }

            if (string.IsNullOrEmpty(AccountNumberLabel.Text))
            {
                return;
            }

            AccountNumberLabel.Text = AccountNumberLabel.Text.Substring(0, AccountNumberLabel.Text.Length - 1);
        }

        /// <summary>
        /// Cancels the account transaction.
        /// </summary>
        private void CancelAccountTransaction()
        {
            if (m_account <= 0)
            {
                return;
            }

            var msg = new B3CancelAccountTransaction(m_account, (int)m_serverState);
            msg.Send();
        }

        #region Event Handlers

        private void RetrieveAccountButton_Click(object sender, EventArgs e)
        {
            var account = ParseAccountNumber();

            if (account == 0)
            {
                return;
            }

            //get the account
            m_parent.B3RetrieveAccount(account);
            m_parent.ShowWaitForm(this);
        }

        private void RedeemButton_Click(object sender, EventArgs e)
        {
            var account = ParseAccountNumber();

            if (account == 0)
            {
                return;
            }

            //redeem account, pop B2 drawer, and print receipt
            m_parent.B3RedeemAccount(account);
            m_parent.ShowWaitForm(this);

            //clear account
            ClearUiAccount();
            RetrieveAccountButton.Focus();

            //update state
            UpdateUiState(B3State.RetrieveAccount);
            m_serverState = B3State.Redeem;
            m_trasactionState = TransactionState.Complete;
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            var account = ParseAccountNumber();

            if (account == 0)
            {
                return;
            }

            //unlock account
            m_parent.B3UnlockAccount(account);
            m_parent.ShowWaitForm(this);

            //clear account info/
            //DE13481: Redeeming incorrect amount for B3 from the POS
            ClearUiAccount();
            RetrieveAccountButton.Focus();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            //clear account
            CancelAccountTransaction();
            ClearUiAccount();
            RetrieveAccountButton.Focus();
        }

        private void B3RetrieveAccountForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!m_isKeypadEnabled)
            {
                return;
            }

            if (char.IsNumber(e.KeyChar))
            {
                AppendNumeric(Convert.ToInt32(e.KeyChar.ToString()));
            }

            if (e.KeyChar == 8) //backspace
            {
                RemoveNumeric();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (m_account > 0 && m_trasactionState == TransactionState.Pending)
            {
                CancelAccountTransaction();
            }

            ClearUiAccount();
            Close();
        }

        #region Num pad events

        private void Button1_Click(object sender, EventArgs e)
        {
            AppendNumeric(1);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            AppendNumeric(2);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            AppendNumeric(3);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            AppendNumeric(4);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            AppendNumeric(5);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            AppendNumeric(6);
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            AppendNumeric(7);
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            AppendNumeric(8);
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            AppendNumeric(9);
        }

        private void Button0_Click(object sender, EventArgs e)
        {
            AppendNumeric(0);
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            AccountNumberLabel.Text = string.Empty;
            if (!string.IsNullOrEmpty(StatusLabel.Text))
            {
                StatusLabel.Text = string.Empty;
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            RemoveNumeric();
        }

        #endregion

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieve account complete. Updates state and UI once the retrieve is complete
        /// </summary>
        /// <param name="message">The message.</param>
        public void RetrieveAccountComplete(B3RetrieveAccountMessage message)
        {
            //need to be on the UI thread when updating UI
            if (CreditLabel.InvokeRequired)
            {
                CreditLabel.Invoke(new Action<B3RetrieveAccountMessage>(RetrieveAccountComplete), message);
                return;
            }
            
            //switch on status
            switch ((B3State)message.Status)
            {
                case B3State.Cashout: //Cashout
                case B3State.Inactive: //Inactive
                case B3State.Unlock:
                    UpdateUiState(B3State.Redeem);
                    break;

                case B3State.Active: //Active
                    StatusLabel.Text = Resources.AccountInUseString;
                    UpdateUiState(B3State.Unlock);
                    break;
                case B3State.Redeem: //Redeem
                    StatusLabel.Text = Resources.AccountRedeemedString;
                    UpdateUiState(B3State.Cancel);
                    break;
                case B3State.Expired: //Expired
                    StatusLabel.Text = Resources.AccountExpiredString;
                    UpdateUiState(B3State.Cancel);
                    break;
                case B3State.Undefined: //Undefined
                    {
                        //clear state
                        ClearUiAccount();
                        return;
                    }
            }

            //Set state
            m_trasactionState = TransactionState.Pending;
            m_serverState = (B3State)message.Status;

            //get account number from textbox
            m_account = int.Parse(AccountNumberLabel.Text);

            //set amounts
            Credits = (decimal)message.Credit / 100;
            WinCredit = (decimal)message.WinCredit / 100;
            Total = Credits + WinCredit;
            
            //set Taxable & Hand Pay
            //Hide controls and then figure out what we need to show
            HideHandPayTaxableAmounts();
            TaxableAmount = 0;
            IsTaxableAmount = false;

            //DE13546
            if (message.Taxable > 0)
            {
                IsTaxableAmount = true;
                TaxableAmount = (decimal)message.Taxable / 100;
                TaxablePayTitleLabel.Visible = true;
                HandPayLabel.Visible = true;
                HandPayBackGround.Visible = true;
            }
            else if (message.HandPay > 0)
            {
                TaxableAmount = (decimal)message.HandPay / 100;
                HandPayTitleLabel.Visible = true;
                HandPayLabel.Visible = true;
                HandPayBackGround.Visible = true;
            }

            //update UI labels
            UpdateUiAmounts();

        }

        /// <summary>
        /// Unlock account complete. Updates state and UI once the unlock is complete
        /// </summary>
        /// <param name="message">The message.</param>
        public void UnlockAccountComplete(B3UnlockAccountMessage message)
        {
            //need to be on the UI thread when updating UI
            if (CreditLabel.InvokeRequired)
            {
                CreditLabel.Invoke(new Action<B3UnlockAccountMessage>(UnlockAccountComplete), message);
                return;
            }
            
            //Set State of the Server
            m_serverState = B3State.Unlock;
            m_trasactionState = TransactionState.Complete;

            //Set state of the UI
            StatusLabel.Text = string.Empty;
            UpdateUiState(B3State.Redeem);
            HideHandPayTaxableAmounts();
        }

        #endregion

    }
}
