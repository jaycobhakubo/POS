#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using GTI.Modules.Shared;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// The implementation of the IGTIModule COM interface for the POS.
    /// </summary>
    [
        ComVisible(true),
        Guid("579CD3C2-C6B2-46f1-AEC1-D0EC59C87F04"),
        ClassInterface(ClassInterfaceType.None),
        ComSourceInterfaces(typeof(_IGTIModuleEvents)),
        ProgId("GTI.Modules.POS.POSModule")
    ]
    public sealed class POSModule : IGTIModule
    {
        #region Constants and Data Types
        private const string ModuleName = "GameTech Edge Bingo System Point of Sale Module"; // FIX: TA5079, TA8833
        #endregion

        #region Events
        /// <summary>
        /// The signature of the 'Stopped' COM connection point handler.
        /// </summary>
        /// <param name="moduleId">The id of the stopped module.</param>
        public delegate void IGTIModuleStoppedEventHandler(int moduleId);

        /// <summary>
        /// The event that will translate to the COM connection point.
        /// </summary>
        public event IGTIModuleStoppedEventHandler Stopped;

        /// <summary>
        /// Occurs when something wants the POS to stop itself.
        /// </summary>
        internal event EventHandler StopPOS;

        /// <summary>
        /// Occurs when something wants the POS to come to the front of the 
        /// screen.
        /// </summary>
        internal event EventHandler BringToFront;

        // PDTS 966
        /// <summary>
        /// Occurs when a server initiated message was received from the 
        /// server.
        /// </summary>
        internal event MessageReceivedEventHandler ServerMessageReceived; 
        #endregion

        #region Member Variables
        private object m_syncRoot = new object();
        private int m_moduleId;
        private static bool m_isStopped = true;
        private Thread m_posThread;
        #endregion

        #region Member Methods
        /// <summary>
        /// Starts the module.  If the module is already started nothing
        /// happens.  This method will block if another thread is currently
        /// executing it.
        /// </summary>
        /// <param name="moduleId">The id to be given to the module.</param>
        public void StartModule(int moduleId)
        {
            lock(m_syncRoot)
            {
                // Don't start again if we are already started.
                if(!m_isStopped)
                    return;

                // Assign the id.
                m_moduleId = moduleId;

                // Create a thread to run the POS.
                m_posThread = new Thread(Run);
                m_posThread.SetApartmentState(ApartmentState.STA);

                // Change the thread regional settings to the current OS 
                // globalization info.
                m_posThread.CurrentUICulture = CultureInfo.CurrentCulture;

                // Start it.
                m_posThread.Start();
                
                // Mark the module as started.
                m_isStopped = false;
            }
        }

        /// <summary>
        /// Creates the POS object and blocks until the POS is told to close
        /// or the user closes the POS.
        /// </summary>
        private void Run()
        {
            PointOfSale pos = null;

            try
            {
                // Create and initialize new POS object.
                pos = new PointOfSale();
                pos.Initialize(m_moduleId);

                // Listen for the events where something wants the POS to stop,
                // come to front, or a message was received..
                StopPOS += new EventHandler(pos.ClosePOS);
                BringToFront += new EventHandler(pos.BringToFront);
                ServerMessageReceived += new MessageReceivedEventHandler(pos.ServerMessageReceived); // PDTS 966

                if(pos.IsInitialized)
                    pos.Start(); // Show the POS and block.
            }
            catch(MissingMethodException e) // Rally DE1813
            {
                MessageBoxOptions options = 0;

                if(CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

                MessageBox.Show(e.Message + Environment.NewLine + ((e.InnerException != null) ? e.InnerException.Message : string.Empty),
                                Resources.POSName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                options);
            }
            catch(Exception e)
            {
                MessageBoxOptions options = 0;

                if(CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.POSError, e.Message + Environment.NewLine + e.StackTrace),
                                Resources.POSName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                options);
            }
            finally
            {
                try
                {
                    // Shutdown the POS.
                    if(pos != null)
                    {
                        pos.Shutdown();
                        pos = null;
                    }

                    OnStop();
                }
                catch
                {
                }

                lock(m_syncRoot)
                {
                    // Mark the module as stopped.
                    m_isStopped = true;
                }
            }
        }
        
        /// <summary>
        /// This method blocks until the module is stopped.  If the module is 
        /// already stopped nothing happens.
        /// </summary>
        public void StopModule()
        {
            if(m_posThread != null)
            {
                // Send the stop event to module's controller.
                EventHandler stopHandler = StopPOS;

                if(stopHandler != null)
                    stopHandler(this, new EventArgs());

                m_posThread.Join();
            }
        }

        /// <summary>
        /// Signals the COM connection point that we have stopped.
        /// </summary>
        internal void OnStop()
        {           
            IGTIModuleStoppedEventHandler handler = Stopped;

            if(handler != null)
                handler(m_moduleId);
        }

        /// <summary>
        /// Returns the name of this GTI module.
        /// </summary>
        /// <returns>The module's name.</returns>
        public string QueryModuleName()
        {
            return ModuleName;
        }

        /// <summary>
        /// Tells the module to bring itself to the front of the screen.
        /// </summary>
        public void ComeToFront()
        {
            EventHandler handler = BringToFront;

            if(handler != null)
                handler(this, new EventArgs());
        }

        // PDTS 966
        /// <summary>
        /// Tells the module that a server initiated message was received.
        /// </summary>
        /// <param name="commandId">The id of the message received.</param>
        /// <param name="messageData">The payload data of the message or null 
        /// if the message has no data.</param>
        public void MessageReceived(int commandId, object msgData)
        {
            MessageReceivedEventArgs args = new MessageReceivedEventArgs(commandId, msgData);

            MessageReceivedEventHandler handler = ServerMessageReceived;

            if(handler != null)
                handler(this, args);
        }
        #endregion
    }
}
