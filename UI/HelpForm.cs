using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.Shared;
using GTI.Modules.POS.Properties;
using GTI.Modules.POS.Business;
using System.Media;

namespace GTI.Modules.POS.UI
{
    internal partial class HelpForm : POSForm
    {
        public enum HelpTopic
        {
            Ordering = 0,
            AutoCoupons,
            Coupons,
            Payment
        }

        private DateTime m_idleSince = DateTime.Now;
        private SoundPlayer m_soundPlayer = null;
        private bool m_suspendBillAcceptor = false;
        private bool m_billAcceptorSuspended = false;
        
        public HelpForm(PointOfSale pos):base(pos)
        {
            initHelpForm(HelpTopic.Ordering);
        }

        /// <summary>
        /// Creates form and selects the given tab index (zero based).
        /// </summary>
        /// <param name="tab">Tab to show.</param>
        public HelpForm(PointOfSale pos, HelpTopic topic, bool suspendBillAcceptor = false):base(pos)
        {
            m_suspendBillAcceptor = suspendBillAcceptor;
            initHelpForm(topic);
        }

        private void initHelpForm(HelpTopic topic)
        {
            DisplayMode displayMode = m_parent.Settings.DisplayMode;
            int tab = 1;

            InitializeComponent();

            m_btnClose.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

            if (!m_parent.WeAreAnAdvancedPOSKiosk) //need to adjust screen
            {
                this.Size = displayMode.BaseFormSize;
                this.Location = new Point(displayMode.OffsetForFullScreenX, displayMode.OffsetForFullScreenY);

                if (m_parent.WeAreABuyAgainKiosk)
                {
                    m_helpTab.TabPages.Clear();

                    if (m_parent.Settings.AutoApplyCouponsOnSimpleKiosks)
                    {
                        m_helpTab.TabPages.Add(m_tabPageAutoCoupons);

                        if (topic != HelpTopic.AutoCoupons)
                            tab = 2;
                    }

                    m_helpTab.TabPages.Add(m_tabPageSimpleKioskPayment);
                }
                else if (m_parent.WeAreASimplePOSKiosk)
                {
                    if (topic == HelpTopic.AutoCoupons)
                        tab = 4 + (m_parent.Settings.AllowPaperOnKiosks ? 1 : 0);

                    m_helpTab.TabPages.Clear();
                    m_helpTab.TabPages.Add(m_tabPageOrderingElectronics);
                    
                    if (m_parent.Settings.AllowPaperOnKiosks)
                        m_helpTab.TabPages.Add(m_tabPageOrderingPaper);

                    m_helpTab.TabPages.Add(m_tabPageOrderingFromReceipt);

                    m_helpTab.TabPages.Add(m_tabPageQuantityControl);

                    if (m_parent.Settings.AutoApplyCouponsOnSimpleKiosks)
                        m_helpTab.TabPages.Add(m_tabPageAutoCoupons);

                    m_helpTab.TabPages.Add(m_tabPageSimpleKioskPayment);

                    if (topic == HelpTopic.Payment)
                        tab = m_helpTab.TabPages.Count;
                }
                else //hybrid POS kiosk
                {
                    m_helpTab.TabPages.Clear();
                    m_helpTab.TabPages.Add(m_tabPageOrderingElectronics);

                    if (m_parent.Settings.AllowPaperOnKiosks)
                        m_helpTab.TabPages.Add(m_tabPageOrderingPaper);

                    m_helpTab.TabPages.Add(m_tabPageHybridOrderingReceipt);

                    m_helpTab.TabPages.Add(m_tabPageQuantityControl);

                    if (m_parent.Settings.AllowCouponButtonOnHybridKiosk)
                    {
                        if (topic == HelpTopic.Coupons)
                            tab = 4 + (m_parent.Settings.AllowPaperOnKiosks ? 1 : 0);

                        m_helpTab.TabPages.Add(m_tabPageHybridCoupons);

                        if (m_parent.Settings.AutoApplyCouponsOnSimpleKiosks)
                        {
                            if (topic == HelpTopic.AutoCoupons)
                                tab = 5 + (m_parent.Settings.AllowPaperOnKiosks ? 1 : 0);

                            m_helpTab.TabPages.Add(m_tabPageAutoCouponsWithCouponButton);
                        }
                    }
                    else //no coupon button
                    {
                        if (m_parent.Settings.AutoApplyCouponsOnSimpleKiosks)
                        {
                            if (topic == HelpTopic.AutoCoupons)
                                tab = 4 + (m_parent.Settings.AllowPaperOnKiosks ? 1 : 0);

                            m_helpTab.TabPages.Add(m_tabPageAutoCoupons);
                        }
                    }

                    m_helpTab.TabPages.Add(m_tabPageSimpleKioskPayment);

                    if (topic == HelpTopic.Payment)
                        tab = m_helpTab.TabPages.Count;
                }
            }
            else //advanced Kiosk
            {
                if (topic == HelpTopic.Coupons)
                    tab = 3;
                else if (topic == HelpTopic.Payment)
                    tab = 4;

                m_helpTab.TabPages.Clear();
                m_helpTab.TabPages.Add(m_tabPageAdvancedKioskQuickAndEasy);
                m_helpTab.TabPages.Add(m_tabPageAdvancedKioskMainScreen);
                m_helpTab.TabPages.Add(m_tabPageCoupons);

                if(m_parent.Settings.UseSimplePaymentForAdvancedKiosk)
                    m_helpTab.TabPages.Add(m_tabPageAdvancedKioskPaymentSimple);
                else
                    m_helpTab.TabPages.Add(m_tabPageAdvancedKioskPayment);
            }

            m_helpTab.SelectedIndex = tab - 1;

            if (m_parent.Settings.UseKeyClickSoundsOnKiosk)
                m_soundPlayer = new SoundPlayer();
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Value = 0;
            m_timeoutProgress.Hide();
            m_lblProgressExplanation.Hide();
        }

        private void m_kioskTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;
            
            if (idleFor > TimeSpan.FromMilliseconds(10000)) //start the countdown after 90 seconds - total time = 2 minutes
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Show();
                    m_lblProgressExplanation.Show();
                }

                m_timeoutProgress.Increment(m_kioskTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    DialogResult = DialogResult.Abort;
                    Close();
                }
            }
        }

        private void HelpForm_Shown(object sender, EventArgs e)
        {
            if (m_suspendBillAcceptor) //we were asked to suspend the bill acceptor
            {
                Application.DoEvents(); //finish drawing the screen
                m_btnClose.Enabled = false; 

                m_billAcceptorSuspended = m_parent.Guardian.SuspendBillAcceptor();

                if (!m_billAcceptorSuspended) //the bill acceptor is accepting something, get out of here!
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else //suspended, allow user to exit
                {
                    Application.DoEvents();
                    m_btnClose.Enabled = true;
                }
            }

            m_timeoutProgress.Maximum = 110000; //1 minute 50 seconds

            NotIdle();

            m_kioskTimer.Start();
        }

        private void HelpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            NotIdle();

            if (m_billAcceptorSuspended)
                m_parent.Guardian.SuspendBillAcceptor(false);

            m_kioskTimer.Stop();
        }

        private void m_helpTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            NotIdle();

            if (m_soundPlayer != null)
                m_soundPlayer.Play();
        }

        private void SomethingWasClicked(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void m_helpTab_DrawItem(object sender, DrawItemEventArgs e)
        {
            //we want the tab control to paint with the top area to the right of the last tab as invisible.
            e.DrawBackground();

            //find the control's rectangle and make it into a rectangle covering the area we want to fix
            Rectangle rect = m_helpTab.ClientRectangle;

            rect.X = m_helpTab.GetTabRect(m_helpTab.TabCount - 1).X + m_helpTab.GetTabRect(m_helpTab.TabCount - 1).Width;
            rect.Width -= m_helpTab.GetTabRect(m_helpTab.TabCount - 1).X;
            rect.Height = m_helpTab.GetTabRect(m_helpTab.TabCount - 1).Height;

            //draw the rectangle filled with the color from that area of the background image
            if(m_parent.WeAreAnAdvancedPOSKiosk)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(162, 169, 185)), rect);
            else
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(210, 207, 213)), rect);

            //draw our tab
            rect = m_helpTab.GetTabRect(e.Index);
            rect.Height += 2; //covers small "selected" color under tab

            //we'll make our unselected tabs a little lighter than the selected tab
            e.Graphics.FillRectangle(e.State == DrawItemState.Selected ? SystemBrushes.Control : SystemBrushes.ControlLight, rect);

            if (!m_helpTab.Enabled)
            {
                e.Graphics.DrawString(m_helpTab.TabPages[e.Index].Text, m_helpTab.Font, System.Drawing.Brushes.Black, new PointF(e.Bounds.X - 1, e.Bounds.Y - 1));
                e.Graphics.DrawString(m_helpTab.TabPages[e.Index].Text, m_helpTab.Font, System.Drawing.Brushes.White, new PointF(e.Bounds.X + 1, e.Bounds.Y + 1));
                e.Graphics.DrawString(m_helpTab.TabPages[e.Index].Text, m_helpTab.Font, System.Drawing.Brushes.Gray, new PointF(e.Bounds.X, e.Bounds.Y));
            }
            else
            {
                e.Graphics.DrawString(m_helpTab.TabPages[e.Index].Text, m_helpTab.Font, System.Drawing.Brushes.Black, new PointF(e.Bounds.X, e.Bounds.Y));
            }

            //e.DrawFocusRectangle();
        }
    }
}
