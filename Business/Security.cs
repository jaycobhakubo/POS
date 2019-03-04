#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Properties;

// PDTS 693
// US1955
//US5709: Pre-Sales - User permissions. Reworked in order to reuse code

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Performs security related functions for the POS.
    /// </summary>
    internal class POSSecurity
    {
        #region Static Variables
        protected static WaitForm m_waitForm;
        protected static object m_lastPermissionResult;
        protected static Exception m_lastException;
        #endregion

        #region Static Methods
        // Rally TA1583
        /// <summary>
        /// Gets the current staff's permissions (module & features) from the 
        /// server.
        /// </summary>
        /// <param name="staff">An instance of the Staff class to retrieve 
        /// permissions for.</param>
        public static void GetStaffPermissionList(Staff staff, bool isB3Enabled)
        {
            // First get all the modules this staff has permission to.
            GetStaffModulesMessage modMsg = new GetStaffModulesMessage(staff.Id, 0);

            try
            {
                modMsg.Send();
            }
            catch(Exception e)
            {
                PointOfSale.ReformatException(e);
            }

            // Parse which values we retrieved from the server.
            foreach(int moduleId in modMsg.ModuleList)
            {
                staff.AddModule((EliteModule)moduleId);
            }

            // Get all the POS permissions for the staff.
            GetStaffModuleFeaturesMessage permMsg = new GetStaffModuleFeaturesMessage(staff.Id, (int)EliteModule.POS, 0);

            try
            {
                permMsg.Send();
            }
            catch(Exception e)
            {
                PointOfSale.ReformatException(e);
            }

            // Parse which values we retrieved from the server.
            foreach(int moduleFeatureId in permMsg.ModuleFeatureList)
            {
                staff.AddModuleFeature(EliteModule.POS, moduleFeatureId);
            }

            // Get all the Money Center permissions for the staff.
            permMsg = new GetStaffModuleFeaturesMessage(staff.Id, (int)EliteModule.MoneyCenter, 0);

            try
            {
                permMsg.Send();
            }
            catch(Exception e)
            {
                PointOfSale.ReformatException(e);
            }

            // Parse which values we retrieved from the server.
            foreach (int moduleFeatureId in permMsg.ModuleFeatureList)
            {
                staff.AddModuleFeature(EliteModule.MoneyCenter, moduleFeatureId);
            }

            // Get all the Receipt Management permissions for the staff.
            permMsg = new GetStaffModuleFeaturesMessage(staff.Id, (int)EliteModule.ReceiptManagement, 0);

            try
            {
                permMsg.Send();
            }
            catch (Exception e)
            {
                PointOfSale.ReformatException(e);
            }

            // Parse which values we retrieved from the server.
            foreach (int moduleFeatureId in permMsg.ModuleFeatureList)
            {
                staff.AddModuleFeature(EliteModule.ReceiptManagement, moduleFeatureId);
            }

            //US5192/DE13378 Add B3 Sales permission per staff  //Add B3 only if it is enabled
            if (isB3Enabled)
            {
                // Get all the B3 Center permissions for the staff.
                permMsg = new GetStaffModuleFeaturesMessage(staff.Id, (int)EliteModule.B3Center, 0);

                try
                {
                    permMsg.Send();
                }
                catch (Exception e)
                {
                    PointOfSale.ReformatException(e);
                }

                // Parse which values we retrieved from the server.
                foreach (int moduleFeatureId in permMsg.ModuleFeatureList)
                {
                    staff.AddModuleFeature(EliteModule.B3Center, moduleFeatureId);
                }
            }
        }

        //US5709: Pre-Sales - User permissions
        /// <summary>
        /// Get override staff module feature permissions
        /// </summary>
        /// <param name="pos">POS</param>
        /// <param name="waitForm">wait form</param>
        /// <param name="owner">owner</param>
        /// <param name="moduleIds">module IDs to determine module features</param>
        /// <returns></returns>
        public static Staff GetOverrideStaffPermissions(PointOfSale pos, WaitForm waitForm, IWin32Window owner, List<int> moduleIds)
        {
            Staff staff = new Staff{Id = 0};
            try
            {   
                int loginNumber;
                string magCardNumber;
                string password;

                // Ask for login.
                PromptForLogin(pos, owner, out loginNumber, out magCardNumber, out password);

                foreach (var moduleId in moduleIds)
                {
                     SendMessage(pos, waitForm, owner, moduleId, 0, loginNumber, magCardNumber, password);

                    // Did the check succeed?
                    if (m_lastException != null)
                    {
                        POSMessageForm.Show(owner, pos, m_lastException.Message);
                        return null;
                    }

                    //make sure we can unbox the module features from the message
                    var staffModuleFeatures = m_lastPermissionResult as Tuple<int, int[]>;
                    if (staffModuleFeatures == null)
                    {
                        POSMessageForm.Show(owner, pos, "Unable to unbox staff module feature permissions");
                        return null;
                    }

                    //set staff ID
                    if (staff.Id == 0)
                    {
                        staff.Id = staffModuleFeatures.Item1;
                    }

                    //add module features
                    foreach (var featureId in staffModuleFeatures.Item2)
                    {
                        staff.AddModuleFeature((EliteModule)moduleId, featureId);
                    }
                }
            }
            catch (POSUserCancelException)
            {
                staff = null;
            }

            return staff;
        }


        /// <summary>
        /// Prompts the user to enter login credentials in order to override the 
        /// lack of permissions specified.
        /// </summary>
        /// <param name="pos">The PointOfSale to which the keypad form will 
        /// belong.</param>
        /// <param name="displayMode">The DisplayMode used for showing 
        /// forms.</param>
        /// <param name="waitForm">The wait form to use while sending server 
        /// messages.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="message">A message to display about what the user 
        /// does not have permission to do.</param>
        /// <param name="moduleId">The id of the module which contains the 
        /// module feature to override.</param>
        /// <param name="moduleFeatureId">The id of the module feature to 
        /// override.</param>
        /// <param name="promptFirst">True=Ask if they want to override, False=Go right to
        /// getting the user ID and password.</param>
        /// <returns>true if the user entered valid credentials for the specified 
        /// permission; otherwise false.</returns>
        /// <remarks>This method will block while sending any server messages
        /// on a seperate thread.</remarks>
        /// <exception cref="GTI.Modules.Shared.ServerCommException">
        /// Communication with the server failed.</exception>
        public static bool TryOverride(PointOfSale pos, WaitForm waitForm, IWin32Window owner, string message, int moduleId, int moduleFeatureId, bool promptFirst = true)
        {
            // For this override, we don't care about these parameters.
            int loginNum, staffID;
            string magCardNum, password;

            return TryOverride(pos, waitForm, owner, message, moduleId, moduleFeatureId, out loginNum, out magCardNum, out password, out staffID, promptFirst);           
        }

        /// <summary>
        /// Prompts the user to enter login credentials in order to override the 
        /// lack of permissions specified.
        /// </summary>
        /// <param name="pos">The PointOfSale to which the keypad form will 
        /// belong.</param>
        /// <param name="displayMode">The DisplayMode used for showing 
        /// forms.</param>
        /// <param name="waitForm">The wait form to use while sending server 
        /// messages.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="message">A message to display about what the user 
        /// does not have permission to do.</param>
        /// <param name="moduleId">The id of the module which contains the 
        /// module feature to override.</param>
        /// <param name="moduleFeatureId">The id of the module feature to 
        /// override.</param>
        /// <param name="loginNumber">The parameter to store the login number 
        /// (or 0 if mag. card number is used).</param>
        /// <param name="magCardNumber">The parameter to store the mag. card 
        /// number (or empty if the login number is used).</param>
        /// <param name="password">The parameter to store the password (or null
        /// if the password is blank).</param>
        /// <param name="promptFirst">True=Ask if they want to override, False=Go right to
        /// getting the user ID and password.</param>
        /// <returns>true if the user entered valid credentials for the specified 
        /// permission; otherwise false.</returns>
        /// <remarks>This method will block while sending any server messages
        /// on a seperate thread.</remarks>
        /// <exception cref="GTI.Modules.Shared.ServerCommException">
        /// Communication with the server failed.</exception>
        public static bool TryOverride(PointOfSale pos, WaitForm waitForm, IWin32Window owner, string message, int moduleId, int moduleFeatureId, out int loginNumber, out string magCardNumber, out string password, bool promptFirst = true)
        {
            int staffID;
            int myLoginNum;
            string myMagCardNum, myPassword;
            bool result = TryOverride(pos, waitForm, owner, message, moduleId, moduleFeatureId, out myLoginNum, out myMagCardNum, out myPassword, out staffID, promptFirst);

            loginNumber = myLoginNum;
            magCardNumber = myMagCardNum;
            password = myPassword;

            return result;
        }

        ///// <summary>
        ///// Prompts the user to enter login credentials in order to override the 
        ///// lack of permissions specified.
        ///// </summary>
        ///// <param name="pos">The PointOfSale to which the keypad form will 
        ///// belong.</param>
        ///// <param name="displayMode">The DisplayMode used for showing 
        ///// forms.</param>
        ///// <param name="waitForm">The wait form to use while sending server 
        ///// messages.</param>
        ///// <param name="owner">Any object that implements IWin32Window 
        ///// that represents the top-level window that will own any modal 
        ///// dialog boxes.</param>
        ///// <param name="message">A message to display about what the user 
        ///// does not have permission to do.</param>
        ///// <param name="moduleId">The id of the module which contains the 
        ///// module feature to override.</param>
        ///// <param name="moduleFeatureId">The id of the module feature to 
        ///// override.</param>
        ///// <param name="promptFirst">True=Ask if they want to override, False=Go right to
        ///// getting the user ID and password.</param>
        ///// <returns>The staff ID of the authorized staff member (0=not authorized).</returns>
        ///// <remarks>This method will block while sending any server messages
        ///// on a seperate thread.</remarks>
        ///// <exception cref="GTI.Modules.Shared.ServerCommException">
        ///// Communication with the server failed.</exception>
        //public static int TryOverrideAndGetID(PointOfSale pos, WaitForm waitForm, IWin32Window owner, string message, int moduleId, int moduleFeatureId, bool promptFirst = true)
        //{
        //    int loginNum, staffID;
        //    string magCardNum, password;
        //    bool authorized = TryOverride(pos, waitForm, owner, message, moduleId, moduleFeatureId, out loginNum, out magCardNum, out password, out staffID, promptFirst);

        //    if (authorized)
        //        return staffID;
        //    else
        //        return 0;
        //}

        /// <summary>
        /// Prompts the user to enter login credentials in order to override the 
        /// lack of permissions specified.
        /// </summary>
        /// <param name="pos">The PointOfSale to which the keypad form will 
        /// belong.</param>
        /// <param name="displayMode">The DisplayMode used for showing 
        /// forms.</param>
        /// <param name="waitForm">The wait form to use while sending server 
        /// messages.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="message">A message to display about what the user 
        /// does not have permission to do.</param>
        /// <param name="moduleId">The id of the module which contains the 
        /// module feature to override.</param>
        /// <param name="moduleFeatureId">The id of the module feature to 
        /// override.</param>
        /// <param name="loginNumber">The parameter to store the login number 
        /// (or 0 if mag. card number is used).</param>
        /// <param name="magCardNumber">The parameter to store the mag. card 
        /// number (or empty if the login number is used).</param>
        /// <param name="password">The parameter to store the password (or null
        /// if the password is blank).</param>
        /// <param name="staffID">The parameter to store the staff ID (0=failure).</param>
        /// <param name="promptFirst">True=Ask if they want to override, False=Go right to
        /// getting the user ID and password.</param>
        /// <returns>true if the user entered valid credentials for the specified 
        /// permission; otherwise false.</returns>
        /// <remarks>This method will block while sending any server messages
        /// on a seperate thread.</remarks>
        /// <exception cref="GTI.Modules.Shared.ServerCommException">
        /// Communication with the server failed.</exception>
        public static bool TryOverride(PointOfSale pos, WaitForm waitForm, IWin32Window owner, string message, int moduleId, int moduleFeatureId, out int loginNumber, out string magCardNumber, out string password, out int staffID, bool promptFirst = true)
        {
            bool returnVal = false;
            loginNumber = 0;
            staffID = 0;
            magCardNumber = null;
            password = null;

            string prompt = string.Empty;

            if(!string.IsNullOrEmpty(message))
                prompt = message + Environment.NewLine;
            
            prompt += Resources.NotAllowed + Environment.NewLine + Resources.EnterOverride;

            if(!promptFirst || POSMessageForm.Show(owner, pos, prompt, POSMessageFormTypes.YesNo) == DialogResult.Yes)
            {
                try
                {
                    // Ask for login.
                    PromptForLogin(pos, owner, out loginNumber, out magCardNumber, out password);

                    SendMessage(pos, waitForm, owner, moduleId, moduleFeatureId, loginNumber, magCardNumber, password);
                    
                    // Did the check succeed?
                    if (m_lastException != null)
                    {
                        POSMessageForm.Show(owner, pos, m_lastException.Message);
                        return false;
                    }
                    else
                    {
                        var staffPermission = m_lastPermissionResult as Tuple<int, int[]>;
                        if (staffPermission == null)
                        {
                            POSMessageForm.Show(owner, pos, "Unable to parse staff permissions");
                            return false;
                        }
                        
                        staffID = staffPermission.Item1;
                        returnVal = staffPermission.Item2.Any(moduleFeature => moduleFeature == moduleFeatureId);

                        if (!returnVal)
                        {
                            POSMessageForm.Show(owner, pos, Resources.StaffNotAuthorized);
                        }
                    }
                }
                catch(POSUserCancelException)
                {
                }
            }

            return returnVal;
        }

        /// <summary>
        /// Prompts the user for a login number/mag. card number and password.
        /// </summary>
        /// <param name="pos">The PointOfSale to which the keypad form will 
        /// belong.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="loginNumber">The parameter to store the login number 
        /// (or 0 if mag. card number is used).</param>
        /// <param name="magCardNumber">The parameter to store the mag. card 
        /// number (or empty if the login number is used).</param>
        /// <param name="password">The parameter to store the password (or null
        /// if the password is blank).</param>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user canceled the prompting process.</exception>
        public static void PromptForLogin(PointOfSale pos, IWin32Window owner, out int loginNumber, out string magCardNumber, out string password)
        {
            loginNumber = 0;
            magCardNumber = string.Empty;
            password = null;
            KeypadForm keyForm = null;
            
            lock(pos.Settings.SyncRoot)
            {
                keyForm = new KeypadForm(pos, pos.Settings.DisplayMode, true);
            }

            keyForm.NumberDisplayMode = Keypad.NumberMode.Integer;
            keyForm.ShowOptionButtons(true, true, true, false, false);
            keyForm.Option1Text = Resources.ButtonOk;
            keyForm.Option2Text = Resources.ButtonCancel;
            keyForm.SetNewMessageFont(new System.Drawing.Font("Trebuchet MS", 16, System.Drawing.FontStyle.Bold));
            keyForm.Message = Resources.PermissionLogin;
            keyForm.EnterPressesOption1 = true;
            keyForm.ShowDialog(owner);
            Application.DoEvents();
            KeypadResult result = keyForm.Result;

            // Did they type in a login number or swipe a card?
            if(result == KeypadResult.Option1)
                loginNumber = Convert.ToInt32((decimal)keyForm.Value, CultureInfo.CurrentCulture);
            else if(KeypadResult.MagneticCard == result)
                magCardNumber = (string)keyForm.Value;
            else
            {
                keyForm.Dispose();
                throw new POSUserCancelException();
            }

            keyForm.Dispose();

            // Ask for the password.
            // US2057 - Support keypad or keyboard password entry.
            if(pos.Settings.UsePasswordKeypad)
            {
                lock(pos.Settings.SyncRoot)
                {
                    keyForm = new KeypadForm(pos, pos.Settings.DisplayMode, false);
                }

                keyForm.NumberDisplayMode = Keypad.NumberMode.Password;
                keyForm.ShowOptionButtons(true, true, true, false, false);
                keyForm.Option1Text = Resources.ButtonOk;
                keyForm.Option2Text = Resources.ButtonCancel;
                keyForm.SetNewMessageFont(new System.Drawing.Font("Trebuchet MS", 16, System.Drawing.FontStyle.Bold));
                keyForm.Message = Resources.PermissionPassword;
                keyForm.EnterPressesOption1 = true;
                keyForm.ShowDialog(owner);
                Application.DoEvents();
                result = keyForm.Result;

                if(KeypadResult.Option1 == result)
                    password = ((decimal)keyForm.Value).ToString(CultureInfo.CurrentCulture);
                else
                {
                    keyForm.Dispose();
                    throw new POSUserCancelException();
                }

                keyForm.Dispose();
            }
            else
            {
                PasswordForm passForm = null;

                lock(pos.Settings.SyncRoot)
                {
                    passForm = new PasswordForm(pos, pos.Settings.DisplayMode);
                }

                passForm.Message = Resources.PermissionPassword;
                passForm.EnterPressesOK = true;

                if (passForm.ShowDialog(owner) == DialogResult.OK)
                {
                    Application.DoEvents();
                    password = passForm.Input;
                }
                else
                {
                    Application.DoEvents();
                    passForm.Dispose();
                    throw new POSUserCancelException();
                }

                passForm.Dispose();
            }
        }

        //US5709
        private static void SendMessage(PointOfSale pos, WaitForm waitForm, IWin32Window owner, int moduleId, int moduleFeatureId, int loginNumber, string magCardNumber, string password)
        {                
            // Check the login with the specified permission.
            waitForm.Message = Resources.WaitFormCheckingPermission;

            BackgroundWorker permWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };

            permWorker.DoWork += GetPermissionDoWork;
            permWorker.ProgressChanged += waitForm.ReportProgress;
            permWorker.RunWorkerCompleted += GetPermissionCompleted;

            object[] args = new object[7];

            lock (pos.Settings.SyncRoot)
            {
                args[0] = pos.Settings;
            }

            lock (pos.CurrentOperator.SyncRoot)
            {
                args[1] = pos.CurrentOperator.Id;
            }

            args[2] = moduleId;
            args[3] = moduleFeatureId;
            args[4] = loginNumber;
            args[5] = magCardNumber;
            args[6] = password;

            permWorker.RunWorkerAsync(args);

            m_waitForm = waitForm;

            if (m_waitForm.WaitToShow())
                m_waitForm.ShowDialog(owner);

            m_waitForm = null;
        }

        //US5709
        /// <summary>
        /// Checks to see if a staff has permission to a module feature.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private static void GetPermissionDoWork(object sender, DoWorkEventArgs e)
        {
            // Wait a couple of ticks to let the wait form display.
            Application.DoEvents();

            bool hasPermission = false;
            string message = null;

            // Get the arguments.
            object[] args = (object[])e.Argument;
            POSSettings settings = (POSSettings)args[0];
            int operatorId = (int)args[1];
            int moduleId = (int)args[2];
            int moduleFeatureId = (int)args[3];
            int loginNumber = (int)args[4];
            string magCardNumber = (string)args[5];
            string password = (string)args[6];

            // Set the language.
            lock(settings.SyncRoot)
            {
                if(settings.ForceEnglish)
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            }

            // Prepare the message.
            GetStaffModuleFeaturesMessage permMsg = new GetStaffModuleFeaturesMessage();

            if(loginNumber != 0)
                permMsg.LoginNumber = loginNumber;
            else
                permMsg.MagneticCardNumber = magCardNumber;

            if(!string.IsNullOrEmpty(password))
                permMsg.PasswordHash = SecurityHelper.HashPassword(password);
            else
                permMsg.PasswordHash = null;

            permMsg.ModuleId = moduleId;
            permMsg.ModuleFeatureId = moduleFeatureId;

            // Send the message.
            try
            {
                permMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException)
            { 
                // We'll handle server exceptions below.
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CheckPermissionFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Check for positive return codes.
            if(permMsg.ServerReturnCode != GTIServerReturnCode.Success)
            {
                switch((GetStaffModuleFeaturesReturnCode)permMsg.ReturnCode)
                {
                    case GetStaffModuleFeaturesReturnCode.StaffNotFound:
                    case GetStaffModuleFeaturesReturnCode.IncorrectPassword:
                        throw new Exception(Resources.InvalidStaff);
                    case GetStaffModuleFeaturesReturnCode.PasswordHasExpired:
                        throw new Exception(Resources.StaffPasswordExpired);
                    case GetStaffModuleFeaturesReturnCode.InactiveStaff:
                        throw new Exception(Resources.StaffInactive);
                    case GetStaffModuleFeaturesReturnCode.StaffLocked:
                        throw new Exception(Resources.StaffLocked);
                    default:
                        throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.CheckPermissionFailed, ServerExceptionTranslator.GetServerErrorMessage(permMsg.ReturnCode)));
                }
            }

            ////Do they have the feature?
            //if (permMsg.ModuleFeatureList.Any(moduleFeature => moduleFeature == moduleFeatureId))
            //{
            //    hasPermission = true;
            //}

            //message = Resources.StaffNotAuthorized;

            //if (hasPermission)
            //    e.Result = new Tuple<int, int[]>(permMsg.StaffId, permMsg.ModuleFeatureList);
            //else
            //    e.Result = message;

            e.Result = new Tuple<int, int[]>(permMsg.StaffId, permMsg.ModuleFeatureList);
        }

        /// <summary>
        /// Handles the event when the check permission BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private static void GetPermissionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            // Or set the results
            if (e.Error != null)
            {
                m_lastException = e.Error;
                m_lastPermissionResult = null;
            }
            else
            {
                m_lastException = null;
                m_lastPermissionResult = e.Result;
            }

            // Close the wait form.
            m_waitForm.CloseForm(); // TTP 50135
            Application.DoEvents();
        }
        #endregion
    }

    /// <summary>
    /// A delegate that allows cross-thread calls to PromptForLogin on the 
    /// POSSecurity class.
    /// </summary>
    /// <param name="pos">The PointOfSale to which the keypad form will 
    /// belong.</param>
    /// <param name="owner">Any object that implements IWin32Window 
    /// that represents the top-level window that will own any modal 
    /// dialog boxes.</param>
    /// <param name="loginNumber">The parameter to store the login number 
    /// (or 0 if mag. card number is used).</param>
    /// <param name="magCardNumber">The parameter to store the mag. card 
    /// number (or empty if the login number is used).</param>
    /// <param name="password">The parameter to store the password (or null
    /// if the password is blank).</param>
    internal delegate void PromptForLoginDelegate(PointOfSale pos, IWin32Window owner, out int loginNumber, out string magCardNumber, out string password);
}