#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 GameTech
// International, Inc.
#endregion

//DE12930: POS: Coupon is not removed from the transaction
//US4852: Product Center > Coupons: Require spend

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GTI.Modules.Shared;
using System.Runtime.InteropServices;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using System.Media;


namespace GTI.Modules.POS.UI
{
    internal partial class PayCouponForm4 : POSForm
    {
        #region Enumerations
        private enum CouponStatus
        {
            NeedToAddPackageFromAnotherSessionToUseCoupon = 0,
            Valid = 1,
            CouponValueIsMoreThanSaleTotal = 2,
            Selected = 3,
            CouponMinimumSpendNotReached = 4,
            MultiPackageCouponUsedByAnotherPackage = 5,
            AddAndUse = 6,
            PackageAlreadyHasACoupon = 7,
            PackageNotAllowed
        }
        #endregion

        #region Member Variables

        private decimal m_taxesAndFees;
        private decimal m_deviceFees;
        private decimal m_taxRate;
        private ImageList m_imageListSmall = new ImageList();
        private bool m_playerCompLoaded;
        private List<PlayerComp> m_playerComps = null;
        private List<PlayerComp> m_addedAndUsedPlayerComps = new List<PlayerComp>();
        private ListViewHitTestInfo m_hitInfo = null;
        private ListViewItem onlyUpdateThisItem = null;
        private int m_idleFor = 0;
        private List<PlayerComp> m_autoApplyComps = new List<PlayerComp>();
        private bool m_processingCoupon = false;
        private SoundPlayer m_soundPlayer = null;
 
        #endregion

        #region Constructors

        internal PayCouponForm4(PointOfSale parent, int deviceId, bool backAndContinue = false):base(parent)
        {
            InitializeComponent();

            DoubleBuffered = true;

            if (parent.Settings.DisplayMode is WideDisplayMode)
            {
                this.Size = parent.Settings.DisplayMode.WideFormSize;
                this.BackgroundImage = Resources.CouponBackWide;
                m_panelMain.Location = new Point(m_panelMain.Location.X + parent.Settings.DisplayMode.EdgeAdjustmentForNormalToWideX, m_panelMain.Location.Y + parent.Settings.DisplayMode.EdgeAdjustmentForNormalToWideY);
            }

            if (parent.WeAreAPOSKiosk)
            {
                FinishSaleButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                ContinueSaleButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                ContinueSaleButton.Text = "Help";
                ContinueSaleButton.ImageNormal = Resources.PurpleButtonUp;
                ContinueSaleButton.ImagePressed = Resources.PurpleButtonDown;
                ContinueSaleButton.ImageIcon = null;

                if (parent.WeAreAHybridKiosk)
                {
                    CouponListUpButton.Visible = false;
                    CouponListDownButton.Visible = false;
                    gtiListViewPlayerCoupon.Scrollable = true;
                    gtiListViewPlayerCoupon.Size = new Size(692, gtiListViewPlayerCoupon.Size.Height);
                    gtiListViewPlayerCoupon.Columns[2].Width = 156;
                    m_soundPlayer = new SoundPlayer(); 
                }
            }

            Prep(deviceId, backAndContinue);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the selected coupons.
        /// </summary>
        /// <value>
        /// The lilst of selected coupons.
        /// </value>
        public List<PlayerComp> SelectedCoupons { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [finish sale].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [finish sale]; otherwise, <c>false</c>.
        /// </value>
        public bool FinishSale { get; private set; }

        #endregion

        #region Methods

        public void Prep(int deviceId, bool backAndContinue = false)
        {
            if (!backAndContinue)
            {
                ContinueSaleButton.Hide();
                FinishSaleButton.Text = Resources.Close;
            }
            else
            {
                ContinueSaleButton.Show();
                FinishSaleButton.Text = Resources.Continue;
            }

            if (m_parent.WeAreAPOSKiosk)
                ContinueSaleButton.Show(); //this is our help button

            FinishSale = true; //only set to false if user presses "Continue Sale"
            SelectedCoupons = new List<PlayerComp>();

            //set device name on screen
            lblDevice.Text = Device.FromId(deviceId).Name;

            //set taxes
            m_taxesAndFees = m_parent.CurrentSale.CalculateTaxes();
            m_taxRate = m_parent.CurrentSale.TaxRate;

            //set device fee
            if (m_parent.CurrentSale.Device.Id != deviceId)
                m_parent.CurrentSale.Device = Device.FromId(deviceId);

            m_deviceFees = m_parent.CurrentSale.DeviceFee;
        }

        /// <summary>
        /// Handles the Load event of the PayCouponForm4 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PayCouponForm4_Load(object sender, EventArgs e)
        {
            if (m_parent.Settings.CouponTaxable)
            {
                lblNonTaxedCouponLabel.Hide();
                lblNonTaxedCouponTotals.Hide();
            }
            else
            {
                lblTaxedCouponLabel.Hide();
                lblTaxedCouponTotals.Hide();
            }

            //load player coupons
            if (!m_playerCompLoaded)
            {
                if (!LoadPlayerComp())
                {
                    Close();
                    return;
                }
            }

            gtiListViewPlayerCoupon.ForceClickOnColumn(1);

            //update calculations in UI
            UpdateCalculations();

            //enable scroll bar
            //gtiListViewPlayerCoupon.Scrollable = false;
        }

        private void gtiListViewPlayerCoupon_MouseDown(object sender, MouseEventArgs e)
        {
            m_hitInfo = gtiListViewPlayerCoupon.HitTest(e.Location);
            gtiListViewPlayerCoupon.AllowEraseBackground = false;

            if (m_parent.WeAreAHybridKiosk && m_parent.Settings.UseKeyClickSoundsOnKiosk)
                m_soundPlayer.Play();
        }

        private void PayCouponForm4_MouseLeave(object sender, EventArgs e)
        {
            m_hitInfo = null;
            gtiListViewPlayerCoupon.AllowEraseBackground = true;
        }

        private void gtiListViewPlayerCoupon_MouseUp(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = gtiListViewPlayerCoupon.HitTest(e.Location);

            if (m_hitInfo != null && m_hitInfo.Item != null)
            {
                if (m_hitInfo.Item == hit.Item)
                    ItemClicked(hit.Item);
            }

            gtiListViewPlayerCoupon.AllowEraseBackground = true;
        }

        /// <summary>
        /// Removes the package assigned to this coupon if the package was added using Add+Use.
        /// </summary>
        /// <param name="coupon"></param>
        /// <returns></returns>
        public bool HandleAddAndUsePackageRemoval(SaleItem couponSaleItem)
        {
            int packageID = couponSaleItem.Coupon.PackageID;
            PlayerComp ourCoupon = m_addedAndUsedPlayerComps.Find(c => c.Id == couponSaleItem.Coupon.Id && c.PackageID == packageID);

            if (ourCoupon == null)
                return false;

            m_addedAndUsedPlayerComps.Remove(ourCoupon);

            object[] saleItems = m_parent.SellingForm.GetSaleListRaw();

            for (int x = 0; x < saleItems.Length; x++)
            {
                SaleItem si = saleItems[x] as SaleItem;

                if (si != null && si.IsPackageItem && si.Package.Id == packageID && si.Session == couponSaleItem.Session && (ourCoupon.CouponType == PlayerComp.CouponTypes.AltPricePackage? si.Package.UseAltPrice : true))
                {
                    if (si.Quantity == 1)
                        m_parent.RemoveSaleItem(si);
                    else
                        si.Quantity--;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles clicking on a list item.
        /// </summary>
        /// <param name="item">The clicked item in the list.</param>
        private void ItemClicked(ListViewItem item)
        {
            if (m_processingCoupon)
                return;

            m_processingCoupon = true;

            //hide status
            lblStatus.Visible = false;

            bool needToShowStatus = false;
            string statusText = string.Empty;

            //get list view item and coupon
            var selectedListViewItem = item;
            var coupon = selectedListViewItem.Tag as PlayerComp;

            //return if null
            if (coupon == null || selectedListViewItem == null)
            {
                gtiListViewPlayerCoupon.AllowEraseBackground = true;
                m_processingCoupon = false;
                return;
            }

            onlyUpdateThisItem = gtiListViewPlayerCoupon.SelectedItems[0];

            gtiListViewPlayerCoupon.BeginUpdate();
            
            //if already in selected coupons, then remove
            if (SelectedCoupons.Contains(coupon))
            {
                int packageID = coupon.PackageID;
                bool removePackage = m_addedAndUsedPlayerComps.Contains(coupon); //need to remove the package too since it was added with Add+Use

                //remove coupon
                selectedListViewItem.ImageIndex = -1;
                SelectedCoupons.Remove(coupon);
                SetAltPriceForPackage(coupon, false);
                m_parent.RemoveCouponFromSale(coupon);

                if (removePackage)
                {
                    m_addedAndUsedPlayerComps.Remove(coupon);

                    object[] saleItems = m_parent.SellingForm.GetSaleListRaw();

                    for (int x = 0; x < saleItems.Length; x++)
                    {
                        SaleItem si = saleItems[x] as SaleItem;

                        if (si != null && si.IsPackageItem && si.Package.Id == packageID && !si.Package.UseAltPrice)
                        {
                            if (si.Quantity == 1)
                                m_parent.RemoveSaleItem(si);
                            else
                                si.Quantity--;

                            break;
                        }
                    }
                }
            }
            else //add coupon
            {
                int AddAndUsePackageID = 0;

                if (GetCouponStatus(coupon) == CouponStatus.AddAndUse) //need to do an Add+Use
                {
                    int itemCount = m_parent.CurrentSale.GetItems().Length;

                    //find the package we need to ring up
                    PackageButton pb = m_parent.GetMenuButtonForPackage(coupon.PackageID);

                    gtiListViewPlayerCoupon.Hide();
                    System.Windows.Forms.Application.DoEvents();

                    //ring up the package
                    m_parent.PushMenuButton(pb);

                    if (m_parent.CurrentSale.GetItems().Length != itemCount) //something added
                        AddAndUsePackageID = coupon.PackageID;

                    gtiListViewPlayerCoupon.Show();
                    System.Windows.Forms.Application.DoEvents();

                    //compute stairstep discounts
                    m_parent.UpdateAutoDiscounts();
                }

                //try to use the coupon
                if (!IsCouponValid(coupon))
                {
                    needToShowStatus = true;
                    statusText = lblStatus.Text;

                    if (AddAndUsePackageID != 0)
                    {
                        SaleItem[] saleItems = m_parent.CurrentSale.GetItems();

                        for (int x = 0; x < saleItems.Length; x++)
                        {
                            if (saleItems[x].IsPackageItem && saleItems[x].Package.Id == AddAndUsePackageID && !saleItems[x].Package.UseAltPrice)
                            {
                                if (saleItems[x].Quantity == 1)
                                    m_parent.RemoveSaleItem(x);
                                else
                                    saleItems[x].Quantity--;

                                break;
                            }
                        }
                    }
                }
                else
                {
                    //add coupon
                    selectedListViewItem.ImageIndex = 0;
                    SelectedCoupons.Add(coupon);
                    SetAltPriceForPackage(coupon, true);
                    m_parent.AddCouponToSale(coupon);

                    if (AddAndUsePackageID != 0)
                        m_addedAndUsedPlayerComps.Add(coupon);
                }
            }
            
            //update coupons since the sale state changed
            SetItemColors();

            //update calculations
            UpdateCalculations();

            if (needToShowStatus)
            {
                lblStatus.Text = statusText;
                lblStatus.Visible = true;
                Application.DoEvents();
            }

            gtiListViewPlayerCoupon.EndUpdate();
            gtiListViewPlayerCoupon.AllowEraseBackground = true;
            onlyUpdateThisItem = null;
            m_processingCoupon = false;
        }

        private void SetItemColors()
        {
            foreach (ListViewItem item in gtiListViewPlayerCoupon.Items)
            {
                PlayerComp coupon = item.Tag as PlayerComp;

                switch (GetCouponStatus(coupon))
                {
                    case CouponStatus.PackageNotAllowed:
                    case CouponStatus.NeedToAddPackageFromAnotherSessionToUseCoupon : //no, no product (it is in another session)
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.DarkGray)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.DarkGray;
                        }
                    }
                    break;

                    case CouponStatus.Valid: //yes
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.Black)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.Black;
                        }
                    }
                    break;

                    case CouponStatus.CouponValueIsMoreThanSaleTotal: //no due to total
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.Red)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.Red;
                        }
                    }
                    break;

                    case CouponStatus.Selected: //yes and selected
                    {
                        if (item.ImageIndex != 0)
                            item.ImageIndex = 0;

                        if (item.BackColor != Color.Green || item.ForeColor != Color.White || item.ImageIndex != 0)
                        {
                            item.BackColor = Color.Green;
                            item.ForeColor = Color.White;
                            item.ImageIndex = 0;
                        }
                    }
                    break;

                    case CouponStatus.CouponMinimumSpendNotReached: //no, minimum spend not reached
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.Red)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.Red;
                        }
                    }
                    break;

                    case CouponStatus.MultiPackageCouponUsedByAnotherPackage: //no, coupon used by another product
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.DarkOrange)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.DarkOrange;
                        }
                    }
                    break;

                    case CouponStatus.AddAndUse: //no, Add+Use coupon will add package and use coupon
                    {
                        if (item.ImageIndex != 1)
                            item.ImageIndex = 1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.Black)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.Black;
                        }
                    }
                    break;

                    case CouponStatus.PackageAlreadyHasACoupon: //no, the coupon's package is already being used by another coupon
                    {
                        if (item.ImageIndex != -1)
                            item.ImageIndex = -1;

                        if (item.BackColor != Color.White || item.ForeColor != Color.LightSlateGray)
                        {
                            item.BackColor = Color.White;
                            item.ForeColor = Color.LightSlateGray;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the ContinueSaleButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ContinueSaleButton_Click(object sender, EventArgs e)//Cancel
        {
            if (m_parent.WeAreAPOSKiosk) //help
            {
                m_kioskIdleTimer.Stop();
                NotIdle();

                HelpForm help = new HelpForm(m_parent, GTI.Modules.POS.UI.HelpForm.HelpTopic.Coupons);

                bool timedOut = help.ShowDialog(this) == DialogResult.Abort;

                m_kioskIdleTimer.Start();

                if (timedOut)
                    m_idleFor = m_parent.SellingForm.KioskIdleLimitInSeconds * 1000; //force a continue question
            }
            else //continue sale
            {
                FinishSale = false;
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the FinishSaleButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FinishSaleButton_Click(object sender, EventArgs e)//OK//Finish sale
        {
            FinishSale = true;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the CouponListUpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CouponListUpButton_Click(object sender, EventArgs e)
        {
            UserActivityDetected(sender, e);
            gtiListViewPlayerCoupon.Up();
        }

        /// <summary>
        /// Handles the Click event of the CouponListDownButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CouponListDownButton_Click(object sender, EventArgs e)
        {
            UserActivityDetected(sender, e);
            gtiListViewPlayerCoupon.Down();
        }

        /// <summary>
        /// Loads the player comp.
        /// </summary>
        public bool LoadPlayerComp(bool checking = false, bool buildAutoApplyList = false)
        {
            if (m_parent.CurrentMenu.Name == Resources.B3SessionString)
                return false;

            if (m_playerCompLoaded)
            {
                if (checking)
                    return gtiListViewPlayerCoupon.Items.Count != 0;

                gtiListViewPlayerCoupon.Items.Clear();
            }

            m_playerCompLoaded = true;

            CouponListUpButton.Hide();
            CouponListDownButton.Hide();

            //add the check image
            m_imageListSmall.Images.Add(Resources.CouponCheck);

            //add the "Add + Use" image
            m_imageListSmall.Images.Add(Resources.AddAndUse);

            m_imageListSmall.ImageSize = new Size(50, 75);
            m_imageListSmall.ColorDepth = ColorDepth.Depth24Bit;

            gtiListViewPlayerCoupon.SmallImageList = m_imageListSmall;

            gtiListViewPlayerCoupon.LabelWrap = true;

            if (m_playerComps == null)
            {
                //get player comps message
                var message = new GetPlayerCompsMessage(m_parent.CurrentSale.Player.Id);

                message.SplitMultiPackageCoupons = true;

                try
                {
                    //send message to server
                    message.Send();
                }
                catch (Exception ex)
                {
                    m_parent.ShowMessage(this, m_parent.Settings.DisplayMode, string.Format(Resources.FailedToGetCouponsFromServer, ex.Message));

                    //close and return
                    return false;
                }

                m_playerComps = message.Comps.ToList();
                
                //give multi-product coupons unique names
                List<PlayerComp> multiPackageCoupons = m_playerComps.Where(c => c.IsPartOfMultiPackageCoupon).ToList();

                //add the button text to the coupon names
                foreach (var coupon in multiPackageCoupons)
                {
                    PackageButton packageButton = m_parent.GetMenuButtonForPackage(coupon.PackageID, false);

                    if(packageButton != null)
                        coupon.Name += "\r\n-" + packageButton.Package.DisplayText;
                }

                //find any multiple package coupons with the same name
                List<PlayerComp> duplicateNames = multiPackageCoupons.FindAll(a => multiPackageCoupons.Find(b => b.Name == a.Name && b.PackageID != a.PackageID) != null);
                Dictionary<int, string> packageNames = new Dictionary<int, string>();
                
                //rename duplicate named multiple package coupons using their package names
                foreach(PlayerComp coupon in duplicateNames)
                {
                    string name = null;

                    //see if we already looked this one up
                    try
                    {
                        name = packageNames[coupon.PackageID];
                    }
                    catch (Exception)
                    {
                    }

                    //ask the server if we need to 
                    if (string.IsNullOrEmpty(name))
                    {
                        GetPackageItemMessage getPackage = new GetPackageItemMessage(coupon.PackageID);

                        try
                        {
                            getPackage.Send();

                            //add the package name to the dictionary so we won't bother the server again for it
                            name = getPackage.PackageItems[0].PackageName;

                            packageNames.Add(coupon.PackageID, name);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    //change the coupon name
                    if (!string.IsNullOrEmpty(name))
                    {
                        int lineBreak = coupon.Name.IndexOf("\r\n-");

                        coupon.Name = coupon.Name.Substring(0, lineBreak + 3) + name;
                    }
                }
            }

            //if no comps, then close
            if (m_playerComps.Count == 0)
                return false;

            if (buildAutoApplyList)
                m_autoApplyComps = new List<PlayerComp>();

            //iterate through coupons from the server
            foreach (var coupon in m_playerComps.Distinct(new PlayerCompComparer()).OrderBy(e => e, new PlayerCompExpirationComparer()))
            {
                bool packageAssigned = coupon.CouponType == PlayerComp.CouponTypes.PercentPackage || coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage; //is this coupon attached to one or more package?
                PackageButton packageButton = null;

                if (packageAssigned) //see if the item is on the menu for any session so we can build the coupon name.
                {
                    packageButton = m_parent.GetMenuButtonForPackage(coupon.PackageID);

                    if (packageButton == null) //not in this session, check other sessions
                    {
                        packageButton = m_parent.GetMenuButtonForPackage(coupon.PackageID, false);

                        if (packageButton == null) //there is no way they can use this
                            continue;
                    }
                }

                //if the percentage is being passed, calculate the value 
                if (coupon.CouponType == PlayerComp.CouponTypes.PercentPackage)
                {
                    //show the coupon value based on the package button
                    if (packageButton != null)
                        coupon.Value = packageButton.Package.Price * coupon.PercentDiscount;
                    else
                        coupon.Value = 0;
                }

                //create list view item
                var listItem = new ListViewItem(coupon.Name);

                double fDays = (coupon.EndDate - DateTime.Now).TotalDays;
                int days = (fDays > 45 ? 100 : (int)fDays);
                string daysLeft = string.Empty;

                if (days == 0)
                {
                    daysLeft = "\r\n(Today)";
                }
                else if ((int)days == 1)
                {
                    daysLeft = "\r\n(Tomorrow)";
                }
                else if (days < 46)
                {
                    daysLeft = "\r\n(" + ((int)days).ToString() + " days)";
                }

                listItem.SubItems.Add(coupon.EndDate.ToString("MM/dd/yy hh:mm tt") + daysLeft);

                if (coupon.CouponType != PlayerComp.CouponTypes.AltPricePackage)
                    listItem.SubItems.Add(coupon.Value.ToString("N2"));
                else
                    listItem.SubItems.Add(string.Empty);

                listItem.Tag = coupon;

                CouponStatus status = GetCouponStatus(coupon, true);

                if (status != CouponStatus.Selected) //not selected, make sure it isn't in the AddedAndUsed list
                {
                    if (m_addedAndUsedPlayerComps.Contains(coupon))
                        m_addedAndUsedPlayerComps.Remove(coupon);
                }

                if (buildAutoApplyList && status != CouponStatus.PackageNotAllowed)
                    m_autoApplyComps.Add(coupon);

                switch (GetCouponStatus(coupon, true))
                {
                    case CouponStatus.PackageNotAllowed:
                    case CouponStatus.NeedToAddPackageFromAnotherSessionToUseCoupon: //no, no product
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.DarkGray;
                        listItem.ImageIndex = -1;
                    }
                    break;

                    case CouponStatus.Valid: //yes
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.Black;
                        listItem.ImageIndex = -1;
                    }
                    break;

                    case CouponStatus.CouponValueIsMoreThanSaleTotal: //no due to total
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.Red;
                        listItem.ImageIndex = -1;
                    }
                    break;

                    case CouponStatus.Selected: //yes and selected
                    {
                        listItem.BackColor = Color.Green;
                        listItem.ForeColor = Color.White;
                        listItem.ImageIndex = 0;
                        SelectedCoupons.Add(coupon);
                    }
                    break;

                    case CouponStatus.CouponMinimumSpendNotReached: //no, minimum spend not reached
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.Red;
                        listItem.ImageIndex = -1;
                    }
                    break;

                    case CouponStatus.MultiPackageCouponUsedByAnotherPackage: //no, coupon used by another product
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.DarkOrange;
                        listItem.ImageIndex = -1;
                    }
                    break;

                    case CouponStatus.AddAndUse: //no, coupon package must be added to use the coupon
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.Black;
                        listItem.ImageIndex = 1;
                    }
                    break;

                    case CouponStatus.PackageAlreadyHasACoupon: //no, coupon's package is being used by another coupon
                    {
                        listItem.BackColor = Color.White;
                        listItem.ForeColor = Color.LightSlateGray;
                        listItem.ImageIndex = -1;
                    }
                    break;
                }

                //add to UI list View
                gtiListViewPlayerCoupon.Items.Add(listItem);
            }

            lblStatus.Hide();

            //determine if we need up/down buttons displayed
            if (!m_parent.WeAreAHybridKiosk && gtiListViewPlayerCoupon.Items.Count > 0) //something to scroll through
            {
                if (!gtiListViewPlayerCoupon.ClientRectangle.Contains(gtiListViewPlayerCoupon.GetItemRect(0)) || !gtiListViewPlayerCoupon.ClientRectangle.Contains(gtiListViewPlayerCoupon.GetItemRect(gtiListViewPlayerCoupon.Items.Count - 1))) //top and bottom items are not both visible, we need up/down buttons
                {
                    CouponListUpButton.Show();
                    CouponListDownButton.Show();
                }
            }

            return true;
        }

        /// <summary>
        /// Applies coupons automatically based on expiration date.
        /// </summary>
        /// <returns>True if any coupons were added.</returns>
        public bool AutoApply()
        {
            bool couponAdded = false;

            LoadPlayerComp(false, true);

            foreach (PlayerComp coupon in m_autoApplyComps)
            {
                if(GetCouponStatus(coupon) == CouponStatus.Valid)
                {
                    //add coupon
                    SelectedCoupons.Add(coupon);
                    SetAltPriceForPackage(coupon, true);
                    m_parent.AddCouponToSale(coupon);
                    UpdateCalculations();
                    couponAdded = true;
                }
            }

            return couponAdded;
        }

        public void ClearSelectedCouponsList()
        {
            SelectedCoupons.Clear();
        }

        /// <summary>
        /// Calculates the coupon total.
        /// </summary>
        /// <returns></returns>
        private decimal CalculateCouponTotal()
        {
            return decimal.Negate(SelectedCoupons.Sum(coupon => coupon.Value));
        }

        /// <summary>
        /// Calculates the sub total.
        /// </summary>
        /// <returns></returns>
        private decimal CalculateSubTotal()
        {
            return m_parent.CurrentSale.CalculateSubtotal()- (m_parent.Settings.CouponTaxable? CalculateCouponTotal() : 0);
        }

        /// <summary>
        /// Calculates the tax and fees.
        /// </summary>
        /// <param name="subtotal">The subtotal.</param>
        /// <param name="couponTotal">The coupon total.</param>
        /// <returns></returns>
        private decimal CalculateTaxAndFees(decimal subtotal, decimal couponTotal)
        {
            return m_parent.CurrentSale.CalculateTaxes() + m_deviceFees;
        }
        
        /// <summary>
        /// Updates the calculations.
        /// </summary>
        private void UpdateCalculations()
        {
            //get calculations
            var subtotal = CalculateSubTotal(); //subtotal for this session with no coupons
            var couponTotal = CalculateCouponTotal(); //coupon total for this session
            var taxesAndFees = CalculateTaxAndFees(subtotal, couponTotal);  //handles taxable/non-taxable coupons
            var prepaid = m_parent.CurrentSale.CalculatePrepaidAmount() + m_parent.CurrentSale.CalculatePrepaidTaxTotal();
            var total = subtotal + couponTotal + taxesAndFees - prepaid;
            
            //update UI labels
            lblOrderSubTotals.Text = m_parent.DefaultCurrency.FormatCurrencyString(subtotal);

            lblTaxedCouponTotals.Text = m_parent.DefaultCurrency.FormatCurrencyString(couponTotal);

            lblTaxesAndFees.Text = m_parent.DefaultCurrency.FormatCurrencyString(taxesAndFees);

            lblNonTaxedCouponTotals.Text = m_parent.DefaultCurrency.FormatCurrencyString(couponTotal);

            lblPrepaidLabel.Visible = prepaid != 0M;
            lblPrepaid.Visible = prepaid != 0M;
            lblPrepaid.Text = m_parent.DefaultCurrency.FormatCurrencyString(-prepaid);

            lblTotal.Text = m_parent.CurrentCurrency.FormatCurrencyString(m_parent.CurrentCurrency.ConvertFromDefaultCurrencyToThisCurrency(total));
        }

        /// <summary>
        /// Checks for an alt price coupon and sets the alt price flag for the first matching package in the sale
        /// </summary>
        /// <param name="coupon">The coupon.</param>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        public void SetAltPriceForPackage(PlayerComp coupon, bool isEnabled)
        {
            //if coupon is an alt price coupon then update alt price for the package
            if (coupon.CouponType != PlayerComp.CouponTypes.AltPricePackage)
            {
                return;
            }

            //get the sale item for the corresponding coupon
            var saleItem = m_parent.CurrentSale.GetItems().FirstOrDefault(
                item => item.IsPackageItem &&
                        item.Session == m_parent.CurrentSession &&
                        item.Package.Id == coupon.PackageID &&
                        item.Package.AppliedDiscountId == 0 &&
                        item.Package.UseAltPrice == !isEnabled);

            //check for null
            if (saleItem == null)
            {
                return;
            }
            
            //If true, check if we need to split items
            //If false, check to see if we need to merge.
            if (isEnabled)
            {
                ////////////////////////////////////////////////////////
                //Check to see if there are multiple sale items.
                //If there are multiple sale items, we need to create 
                //a new seperate line item with alt price.
                ////////////////////////////////////////////////////////
                if (saleItem.Quantity > 1)
                {
                    //copy package
                    var package = new Package(saleItem.Package);

                    //copy products
                    package.CloneProducts(saleItem.Package, this, m_parent);
                    //set the alt price flag
                    package.UseAltPrice = true;

                    // Are there any Crystal Ball Bingo products we have to process 
                    // those before adding the package to the sale?
                    // Rally US505
                    IEnumerable<CrystalBallCardCollection> cbbCards = null;
                    if (package.HasCrystalBall)
                    {
                        cbbCards = m_parent.CrystalBallManager.ProcessCrystalBall(m_parent.CurrentSale,
                            package, 1, m_parent.WeAreAPOSKiosk && m_parent.SellingForm.KioskForm != null ? m_parent.SellingForm.KioskForm : (IWin32Window)this);
                    }

                    //update original sale item quantity. Decrement by one
                    saleItem.Quantity -= 1;

                    //add new sale item, with the alt price
                    m_parent.AddSaleItem(m_parent.CurrentSession,
                        package,
                        1,
                        saleItem.IsPlayerRequired,
                        cbbCards,
                        true,//US5117
                        true);
                }
                else
                {
                    //update alt price flag
                    saleItem.Package.UseAltPrice = true;
                }
            }
            else
            {
                //update the alt price flag for the packages in the session
                m_parent.MergeAltPricePackages(saleItem.Package.Id, saleItem.Session);
            }

            //update percent discounts since subtotal is changing
            m_parent.CurrentSale.UpdatePercentageDiscounts(m_parent.CurrentSession);
        }

        private bool IsCouponValid(PlayerComp coupon)
        {
            CouponStatus status = GetCouponStatus(coupon);

            return (status == CouponStatus.Selected || status == CouponStatus.Valid);
        }

        /// <summary>
        /// See if we can use this coupon.
        /// </summary>
        /// <param name="coupon">Coupon to check.</param>
        /// <returns>Status of the usability of the coupon.</returns>
        private CouponStatus GetCouponStatus(PlayerComp coupon, bool checkSale = false)
        {
            //calculate to see if coupon total is greater than subtotal
            var couponTotal = CalculateCouponTotal();
            var subtotal = CalculateSubTotal();
            var taxesAndFees = CalculateTaxAndFees(subtotal, couponTotal);
            var packageSaleItemsForThisSession = m_parent.CurrentSale.GetItems().Where(item => item.Session == m_parent.CurrentSession).Where(item => item.IsPackageItem).ToList();
            var restrictedSaleTotal = m_parent.CalculateTotalPriceWithRestrictions(packageSaleItemsForThisSession, coupon.RestrictedProductIds, coupon.RestrictedPackageIds, coupon.IgnoreValidationsForIgnoredPackages);

            lblStatus.Text = string.Empty;

            if (checkSale)
            {
                if (m_parent.CurrentSale.GetItems().Any(item => item.IsCoupon && item.Coupon.Id == coupon.Id && item.Coupon.PackageID == coupon.PackageID && m_parent.CurrentSession == item.Session))
                    return CouponStatus.Selected;
            }
            else
            {
                if (SelectedCoupons.Contains(coupon))
                    return CouponStatus.Selected;
            }

            //another coupon with this ID is already used
            if (checkSale)
            {
                SaleItem saleItem = m_parent.CurrentSale.GetItems().ToList().Find(item => item.IsCoupon && item.Coupon.Id == coupon.Id);
                
                if (saleItem != null)
                {
                    lblStatus.Text = string.Format(Resources.CouponUsedForOtherProduct, saleItem.Coupon.Name);
                    return CouponStatus.MultiPackageCouponUsedByAnotherPackage;
                }
            }
            else
            {
                PlayerComp usedCoupon = SelectedCoupons.Find(c => c.Id == coupon.Id);

                if (SelectedCoupons.Exists(c => c.Id == coupon.Id))
                {
                    lblStatus.Text = string.Format(Resources.CouponUsedForOtherProduct, usedCoupon.Name);
                    return CouponStatus.MultiPackageCouponUsedByAnotherPackage;
                }
            }

            //only one coupon per package
            if (coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage || coupon.CouponType == PlayerComp.CouponTypes.PercentPackage)
            {
                SaleItem saleItem = m_parent.CurrentSale.GetItems().ToList().Find(item => item.IsCoupon && item.Coupon.PackageID == coupon.PackageID);

                if (saleItem != null)
                {
                    string couponName = saleItem.Coupon.Name;

                    saleItem = m_parent.CurrentSale.GetItems().ToList().Find(item => item.IsPackageItem && item.Package.Id == coupon.PackageID);

                    lblStatus.Text = string.Format(Resources.PackageAlreadyHasACoupon, saleItem.Package.DisplayText, couponName);
                    return CouponStatus.PackageAlreadyHasACoupon;
                }
            }
            
            //if we don't have the required package and it isn't in our session
            if (coupon.CouponType == PlayerComp.CouponTypes.PercentPackage || coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage)
            {
                var package = m_parent.CurrentSale.GetItems().FirstOrDefault(
                    item => item.IsPackageItem &&
                        item.Package.Id == coupon.PackageID);

                if (package == null && m_parent.GetMenuButtonForPackage(coupon.PackageID) == null)
                {
                    lblStatus.Text = Resources.PackageMustBeInTheSaleToApplyThisCoupon;
                    return CouponStatus.NeedToAddPackageFromAnotherSessionToUseCoupon;
                }
            }

            //barcoded paper not allowed on kiosk
            if (m_parent.WeAreANonAdvancedPOSKiosk && !m_parent.Settings.AllowPaperOnKiosks)
            {
                PackageButton packageButton = m_parent.GetMenuButtonForPackage(coupon.PackageID);

                if (packageButton != null && packageButton.Package.HasBarcodedPaper)
                {
                    lblStatus.Text = Resources.PaperNotAllowedAtKiosk;
                    return CouponStatus.PackageNotAllowed;
                }
            }

            if (coupon.MinimumSpendToQualify > subtotal || coupon.MinimumSpendToQualify > restrictedSaleTotal)
            {
                lblStatus.Text = string.Format(Resources.MustMeetTheMinimumSpendRequirementToApplyThisCouponString, coupon.MinimumSpendToQualify.ToString("N2"));
                return CouponStatus.CouponMinimumSpendNotReached;
            }

            //if not in sale but in session, Add+Use
            if (coupon.CouponType == PlayerComp.CouponTypes.PercentPackage || coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage)
            {
                var package = m_parent.CurrentSale.GetItems().FirstOrDefault(
                        item => item.IsPackageItem &&
                        item.Package.Id == coupon.PackageID &&
                        item.Package.AppliedDiscountId == 0 &&
                        item.Session == m_parent.CurrentSession);

                if (package == null && m_parent.GetMenuButtonForPackage(coupon.PackageID) != null)
                    return CouponStatus.AddAndUse;
            }

            //if coupon total is greater than total
            if (decimal.Negate(couponTotal) + coupon.Value > subtotal) // + taxesAndFees) RAK
            {
                lblStatus.Text = string.Format(Resources.CouponValueExceedsTheTotalSaleValue, coupon.MinimumSpendToQualify.ToString("N2"));
                return CouponStatus.CouponValueIsMoreThanSaleTotal;
            }

            return CouponStatus.Valid;
        }

        private void gtiListViewPlayerCoupon_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (onlyUpdateThisItem != null && onlyUpdateThisItem != e.Item)
                return;

            using (var brush = new SolidBrush(e.Item.BackColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var sf = new StringFormat())
            {
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                    {
                        sf.Alignment = StringAlignment.Center;
                        break;
                    }

                    case HorizontalAlignment.Right:
                    {
                        sf.Alignment = StringAlignment.Far;
                        break;
                    }
                }

                Rectangle rect;

                if (e.ColumnIndex == 0)
                {
                    if (gtiListViewPlayerCoupon.SmallImageList != null) //make room for the picture
                    {
                        rect = new Rectangle(e.Bounds.X + gtiListViewPlayerCoupon.SmallImageList.ImageSize.Width, e.Bounds.Y, e.Bounds.Width - gtiListViewPlayerCoupon.SmallImageList.ImageSize.Width, e.Bounds.Height);

                        if (e.Item.ImageIndex != -1)
                        {
                            Rectangle imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, gtiListViewPlayerCoupon.SmallImageList.ImageSize.Width, gtiListViewPlayerCoupon.SmallImageList.ImageSize.Height);

                            gtiListViewPlayerCoupon.SmallImageList.Draw(e.Graphics, imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height, e.Item.ImageIndex);
                        }
                    }
                    else
                    {
                        rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                    }
                }
                else
                {
                    rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }

                using (var brush = new SolidBrush(e.Item.ForeColor))
                    e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, brush, rect, sf);

                //draw a line to separate the coupons
                if(e.ItemIndex < gtiListViewPlayerCoupon.Items.Count-1)
                {
                    using (var pen = new Pen(Color.LightGray))
                    {
                        e.Graphics.DrawLine(pen, new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Height - 1), new Point(e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Y + e.Bounds.Height - 1));
                    }
                }
            }
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            m_idleFor += m_kioskIdleTimer.Interval;

            if (m_idleFor > m_parent.SellingForm.KioskIdleLimitInSeconds * 1000 - m_timeoutProgress.Maximum)
            {
                if (!m_timeoutProgress.Visible)
                    m_timeoutProgress.Visible = true;

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    NotIdle();
                    DialogResult = System.Windows.Forms.DialogResult.Abort;
                    FinishSale = false;
                    Close();
                }
                else
                {
                    Application.DoEvents();
                }
            }
        }

        private void NotIdle()
        {
            m_idleFor = 0;
            m_timeoutProgress.Value = 0;
            m_timeoutProgress.Hide();
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }
        
        private void PayCouponForm4_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_parent.CouponScreenActive = false;

            if (m_parent.WeAreAPOSKiosk)
                m_kioskIdleTimer.Enabled = false;
        }

        private void PayCouponForm4_Shown(object sender, EventArgs e)
        {
            m_parent.CouponScreenActive = true;

            UserActivityDetected(sender, e);

            m_timeoutProgress.Hide();

            if (m_parent.WeAreAPOSKiosk)
            {
                m_timeoutProgress.Minimum = 0;
                m_timeoutProgress.Maximum = m_parent.SellingForm.KioskMessagePauseInMilliseconds;
                m_timeoutProgress.Value = 0;
                m_kioskIdleTimer.Enabled = true;
            }
        }
        #endregion
    }
}