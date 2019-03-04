#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 Fortunet
// International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using GTI.Modules.Shared;
using System.Linq;
using System.Globalization;


namespace GTI.Modules.POS.Business
{
    internal class PaperUsageReceipt : Receipt
    {

        #region Local and Constants Variables

        protected const int SmallHeaderColumn1Length = 8;
        protected const int SmallHeaderColumn2Length = 11;
        protected const int SmallHeaderColumn3Length = 7;
        protected const int SmallHeaderColumn4Length = 5;
        protected const int SmallBodyColumn1Length = 20; 
        protected List<PaperUsageItem> m_paperUsageItems;
        protected int m_Session;
        protected string m_OperatorName;
        protected DateTime m_startDateTime = new DateTime();
        protected DateTime m_TodaysDateTime = new DateTime();
        protected int m_machineId;
        protected string m_version;
        protected string m_staffName;
        protected int m_staffId;
        protected string m_operatorHeaderLine1;
        protected string m_operatorHeaderLine2;
        protected string m_operatorHeaderLine3;

        #endregion        

        #region Constructor

        internal PaperUsageReceipt(List<PaperUsageItem> items)
        {
            ModuleComm modComm = null;

            // Get the system related ids.
            try
            {
                modComm = new ModuleComm();

                m_machineDesc = modComm.GetMachineDescription();
            }
            catch (Exception)
            {
            }

            m_paperUsageItems = items;
        }

        #endregion

        #region Properties

        public int Session
        {
            get { return m_Session; }
            set{m_Session = value;}
        }


        public string OperatorName
        {
            get { return m_OperatorName; }
            set { m_OperatorName = value; }
        }

        public DateTime StartDateTime
        {
            get { return m_startDateTime; }
            set { m_startDateTime = value; }
        }

        public DateTime TodaysDateTime
        {
            get { return m_TodaysDateTime; }
            set { m_TodaysDateTime = value; }
        }


        public int MachineId
        {
            get { return m_machineId; }
            set { m_machineId = value; }
        }

        public string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        public string StaffName
        {
            get { return m_staffName; }
            set { m_staffName = value; }
        }

        public int StaffId
        {
            get { return m_staffId; }
            set { m_staffId = value; }
        }

        /// <summary>
        /// Gets/sets the operator's address1
        /// </summary>
        public string OperatorAddress1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the operator's address2
        /// </summary>
        public string OperatorAddress2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the operator's city, state, zip
        /// </summary>
        public string OperatorCityStateZip
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the operator's phone number
        /// </summary>
        public string OperatorPhoneNumber
        {
            get;
            set;
        }

        public string OperatorHeader_Line1
        {
            get { return m_operatorHeaderLine1; }
            set { m_operatorHeaderLine1 = value; }
        }

        public string OperatorHeader_Line2
        {
            get { return m_operatorHeaderLine2; }
            set { m_operatorHeaderLine2 = value; }
        }

        public string OperatorHeader_Line3
        {
            get { return m_operatorHeaderLine3; }
            set { m_operatorHeaderLine3 = value; }
        }

     
        #endregion

        #region Methods  
      
        public string FixLongText(string detail, int CharacterLength)
        {
            if (detail.Length > CharacterLength)
            {
                detail = detail.Substring(0, CharacterLength - 1);
                detail = detail + "…";
            } 
          
            return detail;
        }

        protected void PrintOperatorInfo()
        {
            if (!string.IsNullOrEmpty(OperatorName))
                m_printer.AddLine(OperatorName, StringAlignment.Center, m_fontBig);

            if (!string.IsNullOrEmpty(OperatorAddress1))
                m_printer.AddLine(OperatorAddress1, StringAlignment.Center, m_fontMedium);

            if (!string.IsNullOrEmpty(OperatorAddress2))
                m_printer.AddLine(OperatorAddress2, StringAlignment.Center, m_fontMedium);

            if (!string.IsNullOrEmpty(OperatorCityStateZip))
                m_printer.AddLine(OperatorCityStateZip, StringAlignment.Center, m_fontMedium);

            if (!string.IsNullOrEmpty(OperatorPhoneNumber))
                m_printer.AddLine(OperatorPhoneNumber, StringAlignment.Center, m_fontMedium);

            // If anything was printed, add another line
            if (!string.IsNullOrEmpty(OperatorName) ||
               !string.IsNullOrEmpty(OperatorAddress1) ||
               !string.IsNullOrEmpty(OperatorAddress2) ||
               !string.IsNullOrEmpty(OperatorCityStateZip) ||
               !string.IsNullOrEmpty(OperatorPhoneNumber))
            {
                m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            }
        }

        /// <summary>
        /// Prints the operator's header lines.
        /// </summary>
        private  void PrintOperatorLines()
        {
            if (!string.IsNullOrEmpty(m_operatorHeaderLine1))
                m_printer.AddLine(m_operatorHeaderLine1, StringAlignment.Center, m_fontBig);

            if (!string.IsNullOrEmpty(m_operatorHeaderLine2))
                m_printer.AddLine(m_operatorHeaderLine2, StringAlignment.Center, m_fontBig);

            if (!string.IsNullOrEmpty(m_operatorHeaderLine3))
                m_printer.AddLine(m_operatorHeaderLine3, StringAlignment.Center, m_fontBig);

            // Add some spaces.
            if (!string.IsNullOrEmpty(m_operatorHeaderLine1) ||
               !string.IsNullOrEmpty(m_operatorHeaderLine2) ||
               !string.IsNullOrEmpty(m_operatorHeaderLine3))
            {
                m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
                m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            }
        }


        protected virtual void PrintHeader()
        {
            m_printer.AddLine("Paper Usage for Session " + m_Session.ToString(), StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
        }

       private  void PrintHeader2()
        {
            m_printer.AddLine("Gaming Date: " + m_startDateTime.Date.ToShortDateString(), StringAlignment.Near, m_fontSmall);
            m_printer.AddLine("Station: " + m_machineId.ToString(), StringAlignment.Near, m_fontSmall);
            m_printer.AddLine("         " + m_machineDesc, StringAlignment.Near, m_fontSmall);
            m_printer.AddLine("Cashier: " + m_staffName.ToString(), StringAlignment.Near, m_fontSmall);

        }

        /// <summary>
        /// Prints the body for paper usage.
        /// </summary>
        protected virtual void PrintBody()
        {
            int TotalColumnLen = 9;
            int StartEndColumnLen = 7;
            int SRColumnLen = 6;
            int PackNameColumnLen = 11 ;
          
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            //Add item header
            m_printer.AddLine(" Pack", StringAlignment.Near, m_fontSmall);
            m_printer.AddLine(" Name".PadRight(PackNameColumnLen) + "Start".PadLeft(StartEndColumnLen) + "End".PadLeft(StartEndColumnLen) + "S/R".PadLeft(SRColumnLen) + "Total".PadLeft(TotalColumnLen), StringAlignment.Near, m_fontSmall);                       
            //Add item detail and save total quantity issued, total quantity sold and total gross
            string tempPaperDetail = "";
            int tempTotalIssued = 0;
            int tempTotalSold = 0;
            decimal tempTotalGross = 0M;
            foreach (PaperUsageItem pui in m_paperUsageItems)
            {
                tempPaperDetail = "";
                tempPaperDetail += FixLongText(pui.Name, PackNameColumnLen).PadRight(PackNameColumnLen);
                tempPaperDetail += FixLongText(pui.AuditStart.ToString(), StartEndColumnLen).PadLeft(StartEndColumnLen);
                tempPaperDetail += FixLongText((pui.AuditEnd + 1).ToString(), StartEndColumnLen).PadLeft(StartEndColumnLen);
                tempPaperDetail += FixLongText(pui.DamagedList.Count.ToString(), SRColumnLen).PadLeft(SRColumnLen);
                tempPaperDetail += FixLongText("$" + string.Format("{0}", decimal.Round((pui.Price * pui.Quantity), 2).ToString("F")), TotalColumnLen).PadLeft(TotalColumnLen);            
                m_printer.AddLine(tempPaperDetail, StringAlignment.Near, m_fontSmall);
                tempTotalIssued += (pui.AuditEnd - pui.AuditStart) + 1;
                tempTotalSold += ((pui.AuditEnd - pui.AuditStart) + 1) - (pui.SkipList.Count(i => i >= pui.AuditStart && i <= pui.AuditEnd)) - (pui.DamagedList.Count(i => i.AuditNumber >= pui.AuditStart && i.AuditNumber <= pui.AuditEnd));
                tempTotalGross += (pui.Price * pui.Quantity); 
                
            }

            int TempColumnLen_Issued = 12;
            int TempColumnLen_Sold = 10;
            int TempColumnLen_Gross = 18;

            string temp = "";
            temp = string.Empty.PadRight(m_fontMediumMaxChars, '-');
            m_printer.AddLine(temp, StringAlignment.Center, m_fontMedium);      
            tempPaperDetail = "";
            tempPaperDetail += FixLongText(("Issued:" + tempTotalIssued.ToString() + " "), TempColumnLen_Issued).PadRight(TempColumnLen_Issued); ;
            tempPaperDetail += FixLongText(("Sold:" + tempTotalSold.ToString() + " "), TempColumnLen_Sold).PadRight(TempColumnLen_Sold); 
            tempPaperDetail += FixLongText ("Gross: $"+string.Format("{0}", decimal.Round(( tempTotalGross), 2).ToString("F")).ToString(), TempColumnLen_Gross).PadLeft(TempColumnLen_Gross);
            m_printer.AddLine(tempPaperDetail, StringAlignment.Near, m_fontSmall);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            LineSignatureFooter();
        }


        private void LineSignatureFooter()
        {

            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine("X" + string.Empty.PadRight(20, '_'), StringAlignment.Near, m_fontMedium);
            m_printer.AddLine("Cashier", StringAlignment.Near, m_fontSmall);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine("X" + string.Empty.PadRight(20, '_'), StringAlignment.Near, m_fontMedium);
            m_printer.AddLine("Supervisor", StringAlignment.Near, m_fontSmall);
        }

        /// <summary>
        /// Prints the receipt's footer lines.
        /// </summary>
        private void PrintFooter()
        {
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontExtraHuge);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontExtraHuge);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontExtraHuge);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontExtraHuge);       
            m_printer.AddLine(m_TodaysDateTime.ToShortDateString() + " " + m_TodaysDateTime.ToString("hh:mm:ss tt"), StringAlignment.Center, m_fontSmall);
        }

        /// <summary>
        /// Print preview.
        /// </summary>
        /// <param name="printer">The printer.</param>
        /// <param name="merchantCopy">if set to <c>true</c> [merchant copy].</param>
        /// <exception cref="System.ArgumentNullException">printer</exception>
        public void PrintPreview(Printer printer, bool merchantCopy = false)
        {
            if (printer == null)
                throw new ArgumentNullException("printer");

            m_printer = printer;
            m_printer.Margins = new Margins((int)m_printer.HardMarginX, (int)m_printer.HardMarginX, (int)m_printer.HardMarginY, (int)m_printer.HardMarginY);
            SetTextSizes(m_printer.Using58mmPaper);
            m_printer.ClearLines();
            PrintOperatorInfo();
            PrintOperatorLines();
            PrintHeader();
            PrintHeader2();
            PrintBody();
            PrintFooter();           
        }

   
        /// <summary>
        /// Prints to the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        /// <param name="copies">The copies.</param>
        /// <param name="merchantCopy">if set to <c>true</c> [merchant copy].</param>
        public void Print(Printer printer, short copies, bool merchantCopy = false)
        {
            PrintPreview(printer,  merchantCopy);

            for (int x = 0; x < copies; x++)
            {
                m_printer.Print();
            }
        }
        
        #endregion
    }

    internal class PaperUsageUnscannedPacksReceipt : PaperUsageReceipt
    {
        private bool m_isThereUnscannedPack;

        #region Constructor

        internal PaperUsageUnscannedPacksReceipt(List<PaperUsageItem> items):base(items)
        {
            m_paperUsageItems = items;
            m_isThereUnscannedPack = checkUnscannedPacks();
        }

        #endregion

        #region Properties

        public bool IsThereUnscannedPack
        {
            get { return m_isThereUnscannedPack; }
        }

        #endregion

        #region Methods

        public bool checkUnscannedPacks()
        {
            bool result = false;
            //var tempResult = m_paperUsageItems.Exists(l => l.UnscannedList.Count > 0);
             foreach (PaperUsageItem pui in m_paperUsageItems)
            {
                if (pui.UnscannedList.Count != 0 && result == false)
                {
                    result = true;
                }
             }
            return result;
        }

        protected override void PrintHeader()
        {       
            m_printer.AddLine("Unscanned Pack for Session " + m_Session.ToString(), StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
        }

    
        /// <summary>
        /// Prints the body for unscanned packs.
        /// </summary>
        protected override void PrintBody()
        {
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            //Add item header
            m_printer.AddLine("These packs were not scanned or are", StringAlignment.Near, m_fontSmall);
            m_printer.AddLine("missing:", StringAlignment.Near, m_fontSmall);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);

            foreach (PaperUsageItem pui in m_paperUsageItems)
            {
                int startCount = 0;
                int endCount = 0;
                int counter = 0;

                foreach (var unscannedPackNumber in pui.UnscannedList)
                {
                    if (startCount == 0)
                    {
                        startCount = unscannedPackNumber;
                        continue;
                    }

                    counter += 1;

                    if (startCount + counter == unscannedPackNumber)
                    {
                        endCount = unscannedPackNumber;
                        continue;
                    }
                    else//End of the last record
                    {
                        PackDetailtItem(startCount, endCount, pui.Name, pui.Serial);
                        startCount = unscannedPackNumber;
                        counter = 0;
                        endCount = 0;
                    }
                }

                if (startCount != 0)
                {
                    PackDetailtItem(startCount, endCount, pui.Name, pui.Serial);
                }
            }
        }

        private void PackDetailtItem(int startCount, int endCount, string Name, string serialNumber)
        {
            string tempPaperDetail = "";
            if (endCount != 0)
            {
                tempPaperDetail = "";
                tempPaperDetail += FixLongText(Name, 12).PadRight(12);
                tempPaperDetail += " ";
                tempPaperDetail += FixLongText(serialNumber.ToString(), 11).PadRight(11);
                tempPaperDetail += " ";
                tempPaperDetail += (startCount.ToString() + " to " + endCount.ToString()).PadRight(14);
                m_printer.AddLine(tempPaperDetail, StringAlignment.Near, m_fontSmall);
            }
            else
            {
                tempPaperDetail = "";
                tempPaperDetail += FixLongText(Name, 12).PadRight(12);
                tempPaperDetail += " ";
                tempPaperDetail += FixLongText(serialNumber.ToString(), 11).PadRight(11);
                tempPaperDetail += " ";
                tempPaperDetail += (startCount.ToString()).PadRight(14);
                m_printer.AddLine(tempPaperDetail, StringAlignment.Near, m_fontSmall);
            }
        }
      
        #endregion
    }

    internal class PaperUsageStartReceipt : PaperUsageReceipt
    {

        #region Constructor

        internal PaperUsageStartReceipt(List<PaperUsageItem> items)
            : base(items)
        {
            m_paperUsageItems = items;
        }

        #endregion

   
        #region Methods

        protected override void PrintHeader()
        {
            m_printer.AddLine("Starting Paper for Session " + m_Session.ToString(), StringAlignment.Center, m_fontSmall);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
        }

        /// <summary>
        /// Prints the body for paper usage.
        /// </summary>
        protected override void PrintBody()
        {

            string tempPaperDetail = "";
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium); 

            foreach (PaperUsageItem pui in m_paperUsageItems)
            {
                tempPaperDetail = "";
                tempPaperDetail += FixLongText(pui.Name, 14).PadRight(14);
                tempPaperDetail += "(".PadRight(1);
                tempPaperDetail += FixLongText(pui.Serial.ToString(), 10).PadRight(10);
                tempPaperDetail += ")".PadRight(1);
                tempPaperDetail += " ".PadRight(1);
                tempPaperDetail += FixLongText((pui.AuditStart).ToString(), 10).PadLeft(10);
                m_printer.AddLine(tempPaperDetail, StringAlignment.Near,m_fontSmall);
               
            }
          
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
            m_printer.AddLine(string.Empty, StringAlignment.Center, m_fontMedium);
        }


        #endregion
    }


}
