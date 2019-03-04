using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using GTI.Controls;

namespace GTI.Modules.POS.UI
{
    internal partial class B3KioskForm : GTI.Modules.POS.UI.POSForm
    {
        private DateTime m_idleSince = DateTime.Now;
        private bool m_weHaveABillAcceptor = false;

        public B3KioskForm(PointOfSale parent)
            : base(parent, parent.Settings.DisplayMode.BasicCopy())
        {
            InitializeComponent();
            base.ApplyDisplayMode();
            this.StartPosition = FormStartPosition.Manual;

            if(parent.WeAreAnAdvancedPOSKiosk)
                this.Location = new Point(parent.SellingForm.Location.X + parent.Settings.DisplayMode.OffsetForFullScreenX, parent.SellingForm.Location.Y + parent.Settings.DisplayMode.OffsetForFullScreenY);
            else
                this.Location = new Point(parent.SellingForm.KioskForm.Location.X + parent.Settings.DisplayMode.OffsetForFullScreenX, parent.SellingForm.KioskForm.Location.Y + parent.Settings.DisplayMode.OffsetForFullScreenY);

            m_parent.Guardian.UpdateDeviceStates();
            m_weHaveABillAcceptor = m_parent.Guardian.AcceptorState != GuardianWrapper.DeviceState.NotInstalled && !m_parent.Settings.KioskTestWithoutAcceptor;

            if (m_weHaveABillAcceptor)
                m_parent.Guardian.MoneyAccepted += new EventHandler<Guardian.Acceptors.ItemAcceptResultEventArgs>(Guardian_MoneyAccepted);

            if (m_parent.Settings.KioskTestWithoutAcceptor)
            {
                m_btnB3Test1.Visible = true;
                m_btnB3Test1.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                m_btnB3Test5.Visible = true;
                m_btnB3Test5.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                m_btnB3Test10.Visible = true;
                m_btnB3Test10.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            }

            //set the bill acceptor to get bills
            if (m_weHaveABillAcceptor)
                m_parent.Guardian.ActivateBillAcceptor(); //keep accepting bills until we deactivate it

            if (!m_parent.IsB3Sale)
            {
                m_parent.ClearSale();

                if (!m_parent.SellingForm.SelectB3Menu(true))
                {
                    POSMessageForm.Show(m_parent.SellingForm.KioskForm, m_parent, Resources.AddSaleErrorSessionSync);
                    DialogResult = DialogResult.Abort;
                    Close();
                    return;
                }
            }

            //see if we are here to give change as B3 credits
            if (m_parent.SellingForm.GiveChangeAsB3Credit && m_parent.SellingForm.ChangeToGiveAsB3Credit != 0M)
            {
                m_lblInstructions.Text = "You can insert bills to add to the amount applied toward B3.";
                LogTender(TenderType.Cash, m_parent.SellingForm.ChangeToGiveAsB3Credit);
            }

            m_parent.SellingForm.GiveChangeAsB3Credit = false;
            m_parent.SellingForm.ChangeToGiveAsB3Credit = 0M;

            m_btnBuy.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            m_btnQuit.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
        }

        private void m_btnBuy_Click(object sender, EventArgs e)
        {
            m_btnBuy.Enabled = false;
            m_btnBuy.Update();
            Application.DoEvents();

            //we need to disable the bill acceptor before we can continue (may be accepting right now)
            if (m_weHaveABillAcceptor)
            {
                if (!m_parent.Guardian.ActivateBillAcceptor(false))
                {
                    m_btnBuy.Enabled = false;
                    return; //can't cancel, stay in sale
                }
            }

            NotIdle();
            m_kioskTimer.Stop();

            try
            {
                m_parent.SellingForm.ProcessSale();
            }
            catch (Exception)
            {
                //try to give the money back through normal means (disallowing B3 credit).
                decimal leftToPay = m_parent.SellingForm.VendChange(Convert.ToDecimal(m_lblTotal.Text.Replace('$', ' ')), true);

                if (leftToPay != 0M) //not all of it was paid out, tell the Guardian.
                    m_parent.RequestHelpFromGuardian("B3 sale failed and refunding cash - Could not dispense "+leftToPay.ToString("C")+ ".");

                m_parent.ClearSale(); //the B3 sale failed
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void m_btnQuit_Click(object sender, EventArgs e)
        {
            m_btnQuit.Enabled = false;
            Application.DoEvents();

            //we need to disable the bill acceptor before we can continue (may be accepting right now)
            if (m_weHaveABillAcceptor)
            {
                if (!m_parent.Guardian.ActivateBillAcceptor(false))
                {
                    m_btnQuit.Enabled = true;
                    return; //can't cancel, stay in sale
                }
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Value = 0;
            m_timeoutProgress.Hide();
        }

        private void m_kioskTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended) //don't do anything
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds((int)((double)KioskIdleLimitInSeconds * 1000f / 3f * 2f))) //start the countdown after 2/3rds idle limit reached
            {
                if (!m_timeoutProgress.Visible)
                    m_timeoutProgress.Show();

                m_timeoutProgress.Increment(m_kioskTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    m_kioskTimer.Stop();

                    //stop the bill acceptor
                    if (m_weHaveABillAcceptor)
                    {
                        if (!m_parent.Guardian.ActivateBillAcceptor(false)) //could not stop the bill acceptor, keep going
                        {
                            NotIdle(); //reset the timer
                            m_kioskTimer.Start();
                            return;
                        }
                    }

                    int answer = POSMessageForm.ShowCustomTwoButton(this, m_parent, Resources.KioskIdleQuestion, Resources.KioskIdle, true, 2, Resources.Continue, m_btnBuy.Visible? Resources.PrintReceipt : Resources.CancelSale);

                    NotIdle(); //reset the timer
                    m_kioskTimer.Start();

                    if (answer == 2) //cancel sale
                    {
                        if (m_btnBuy.Visible)
                        {
                            m_btnBuy_Click(sender, new EventArgs());
                        }
                        else
                        {
                            DialogResult = DialogResult.Abort;
                            Close();
                        }

                        return;
                    }

                    //start the bill acceptor
                    if (m_weHaveABillAcceptor)
                        m_parent.Guardian.ActivateBillAcceptor(); //accept bills until we deactivate
                }
            }
        }

        private void B3KioskForm_Shown(object sender, EventArgs e)
        {
            m_parent.SellingForm.NotIdle(true); //stop main timeout timer, we are going to use our own
            m_timeoutProgress.Maximum = (int)(((double)KioskIdleLimitInSeconds * 1000f) / 3f);
            UpdateQuitPrintAndInstructions();
            NotIdle();
            m_kioskTimer.Start();
        }

        private void LogTender(TenderType tenderType, decimal amount)
        {
            m_parent.ProcessingTender = true;
            DisableSimpleKioskControls();

            m_parent.AddSaleItem(m_parent.CurrentSession, 1, new B3Credit
            {
                Amount = amount
            });

//            m_parent.SellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.B3AmountCollected, amount);
//            m_parent.SellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.B3AmountCollectedLifetime, amount);
            m_lblTotal.Text = "$"+(Convert.ToDecimal(m_lblTotal.Text.Substring(1)) + amount).ToString();
            UpdateQuitPrintAndInstructions();
            DisableSimpleKioskControls(false);
            m_parent.ProcessingTender = false;
        }

        private void UpdateQuitPrintAndInstructions()
        {
            m_btnQuit.Visible = Convert.ToDecimal(m_lblTotal.Text.Substring(1)) == 0;
            m_btnBuy.Visible = !m_btnQuit.Visible;
            m_lblInstructions2.Visible = m_btnBuy.Visible;
        }

        private void DisableSimpleKioskControls(bool disable = true)
        {
            if (m_parent.WeAreANonAdvancedPOSKiosk)
            {
                if (disable)
                    m_kioskTimer.Stop();
                else
                    m_kioskTimer.Start();

                NotIdle();

                m_btnQuit.Enabled = !disable;
                m_btnBuy.Enabled = !disable;
            }
        }

        private void B3KioskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //turn off the bill acceptor
            if (m_weHaveABillAcceptor)
            {
                m_parent.Guardian.ActivateBillAcceptor(false);
                m_parent.Guardian.MoneyAccepted -= Guardian_MoneyAccepted;
            }

            NotIdle();
            m_kioskTimer.Stop();
            m_parent.SellingForm.NotIdle();
            m_parent.SellingForm.SelectB3Menu(false);
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_btnB3Test_Click(object sender, EventArgs e)
        {
            decimal value = Convert.ToDecimal(((ImageButton)sender).Text.Substring(1));
            m_parent.SellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, value); //keep track of how much we collected for this transaction (power failure)
            LogTender(TenderType.Cash, value);
        }
        
        #region Bill acceptor events

        private void Guardian_MoneyAccepted(object sender, Guardian.Acceptors.ItemAcceptResultEventArgs e)
        {
            NotIdle();

            if (e == null) //the bill acceptor has something
            {
                if (m_lblTotal.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        DisableSimpleKioskControls();
                    }));
                else
                    DisableSimpleKioskControls();

                return;
            }

            if (e.Result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.REJECTED)
            {
                if (m_lblTotal.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        DisableSimpleKioskControls(false);
                    }));
                else
                    DisableSimpleKioskControls(false);

                return;
            }

            if (e.Result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.ACCEPTED) //we accepted something
            {
                decimal value = e.Result.Value == null ? 0M : (decimal)e.Result.Value;

                if (m_parent.Settings.KioskTestTreatTicketAs20 && e.Result.Item.ItemType == Guardian.Acceptors.AcceptorItemTypes.BARCODE)
                {
                    if (e.Result.Barcode == "000000000000000100")
                        value = 1M;
                    else if (e.Result.Barcode == "000000000000000500")
                        value = 5M;
                    else if (e.Result.Barcode == "000000000000001000")
                        value = 10M;
                    else if (e.Result.Barcode == "000000000000002000")
                        value = 20M;
                    else if (e.Result.Barcode == "000000000000005000")
                        value = 50M;
                    else if (e.Result.Barcode == "000000000000010000")
                        value = 100M;
                    else
                        value = 20M;
                }

                m_parent.SellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, value); //keep track of how much we collected for this transaction (power failure)

                if (m_lblTotal.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        LogTender(TenderType.Cash, value);
                    }));
                else
                    LogTender(TenderType.Cash, value);
            }
        }
        
        #endregion
    }
}
