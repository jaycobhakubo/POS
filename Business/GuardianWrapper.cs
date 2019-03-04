using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuardianClientServer;
using GuardianClientServer.Messages;
using System.Net;
using Board;
using DataIntegrity = Board.DataIntegrity;
using System.IO;

namespace GTI.Modules.POS.Business
{
    class GuardianWrapper : IDisposable
    {
        private const uint NVDataBlocksStart = 2000;
        private const uint NVDataBlockHeaderSize = 2 * sizeof(uint);
        private bool m_disposed = false;
        private bool m_pretendingWeHaveAGuardian = false;
        private FileStream m_pretendGuardianMemory = null;
        private Guid? m_lastAcceptorRequest = null;

        public enum DeviceState
        {
            Unknown = -1,
            NotInstalled = 0,
            Ready = 1,
            Active = 2,
            Suspended = 3
        }

        private enum CancelState
        {
            Canceled = 0,
            Waiting = 1,
            NotCanceled = 2
        }

        private const uint LastAcceptorRequest = 1;
        private const uint LastDispenserRequest = 2;

        private PointOfSale m_pos = null;
        private GuardianClientServer.ConsumerClient m_consumer = null;
        private object m_guardianLock = new object();
        private IPEndPoint m_guardianEndPoint = null;
        private string m_clientName = string.Empty;
        private DeviceState m_acceptorState = DeviceState.Unknown;
        private DeviceState m_dispenserState = DeviceState.Unknown;
        private decimal m_amountToCover = 0M;
        private decimal m_amountAccepted = 0M;
        private decimal m_suspendedAmountToCover = 0M;
        private decimal m_suspendedAmountAccepted = 0M;
        private decimal m_amountDispensed = 0M;
        private CancelState m_waitingForAcceptorCancel = CancelState.Canceled;
        private bool m_initialized = false;
        private uint? m_userNVMSize = null;
        private uint? m_userMeters = null;

        public GuardianWrapper(PointOfSale pos, string clientName, IPEndPoint ep)
        {
            m_pos = pos;
            m_clientName = clientName;
            m_guardianEndPoint = ep;
            m_pretendingWeHaveAGuardian = pos.Settings.KioskTestNoGuardian;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!m_disposed)
            {
                if(disposing)
                {
                    if (!m_pretendingWeHaveAGuardian)
                    {
                        try
                        {
                            lock (m_guardianLock)
                            {
                                //handle any wrapper events that were subscribed to
                                GuardianRequestedControl = null;
                                GuardianReleasedControl = null;
                                GuardianRequestedShutdown = null;

                                //unsubscribe from Guardian events we subscribed to
                                if (m_consumer != null)
                                {
                                    if (m_initialized)
                                        m_consumer.FinalizeConsumerStorage();

                                    m_consumer.ConnectedChanged -= client_ConnectedChanged;
                                    m_consumer.StartupComplete -= consumer_StartupComplete;
                                    m_consumer.StartupFailed -= consumer_StartupFailed;
                                    m_consumer.StorageStatusChanged -= consumer_StorageStatusChanged;
                                    m_consumer.ConsumerShutdownRequested -= consumer_ConsumerShutdownRequested;
                                    m_consumer.ConsumerSuspendRequested -= consumer_ConsumerSuspendRequested;
                                    m_consumer.ConsumerResumeRequested -= consumer_ConsumerResumeRequested;
                                    m_consumer.ActionCompleted -= consumer_ActionCompleted;
                                    m_consumer.ShutdownComplete -= consumer_ShutdownComplete;
                                    m_consumer.ShutdownFailed -= consumer_ShutdownFailed;
                                    m_consumer.TamperActivatedChanged -= consumer_TamperActivatedChanged;
                                    m_consumer.ActionStateChanged -= consumer_ActionStateChanged;
                                    
                                    if (m_initialized)
                                        m_consumer.Shutdown();

                                    m_consumer = null;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            m_pretendGuardianMemory.Close();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                m_disposed = true;
            }
        }

        ~GuardianWrapper()
        {
            Dispose(false);
        }

        public void ConnectToGuardian(bool waitForConnection = false)
        {
            try
            {
                if (m_pretendingWeHaveAGuardian)
                {
                    if(!File.Exists("c:\\PretendGuardian.mem"))
                    {
                        m_pretendGuardianMemory = File.Create("c:\\PretendGuardian.mem", 4420, FileOptions.WriteThrough);
                        m_pretendGuardianMemory.Write(Enumerable.Repeat((byte)0, 4420).ToArray(), 0, 4420);
                        m_pretendGuardianMemory.Close();
                    }

                    m_pretendGuardianMemory = File.Open("c:\\PretendGuardian.mem", FileMode.Open, FileAccess.ReadWrite);
                    ConnectedToGuardian = true;
                    m_initialized = true;
                    UpdateDeviceStates();
                    return;
                }

                if(m_guardianEndPoint != null)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem((p) =>
                    {
                        lock(m_guardianLock)
                        {
                            if(m_consumer == null)
                            {

                                var client = new GuardianClientServer.ConsumerClient(m_clientName, m_guardianEndPoint);
                                client.ConnectedChanged += new EventHandler<Guardian.Support.ValueChangedEventArgs<bool>>(client_ConnectedChanged);

                                m_consumer = client;
                                m_consumer.StartupComplete += new EventHandler(consumer_StartupComplete);
                                m_consumer.StartupFailed += new EventHandler<Guardian.Support.TagEventArgs<string>>(consumer_StartupFailed);
                                m_consumer.StorageStatusChanged += new EventHandler(consumer_StorageStatusChanged);
                                m_consumer.ConsumerShutdownRequested += new EventHandler(consumer_ConsumerShutdownRequested);
                                m_consumer.ConsumerSuspendRequested += new EventHandler(consumer_ConsumerSuspendRequested);
                                m_consumer.ConsumerResumeRequested += new EventHandler(consumer_ConsumerResumeRequested);
                                m_consumer.ActionCompleted += new EventHandler<Guardian.Guardian2ActionCompletedEventArgs>(consumer_ActionCompleted);
                                m_consumer.ShutdownComplete += new EventHandler(consumer_ShutdownComplete);
                                m_consumer.ShutdownFailed += new EventHandler<Guardian.Support.TagEventArgs<string>>(consumer_ShutdownFailed);
                                m_consumer.TamperActivatedChanged += new EventHandler<Guardian.TamperIoManagement.TamperActivatedChangedEventArgs>(consumer_TamperActivatedChanged);
                                m_consumer.ActionStateChanged += new EventHandler<Guardian.Guardian2ActionStateEventArgs>(consumer_ActionStateChanged);
                                m_consumer.Start();
                            }
                        }
                    });

                    if(waitForConnection) //wait for at least 30 seconds for a connection to the Guardian
                    {
                        int milliseconds = 30000;

                        while (milliseconds > 0 && !(ConnectedToGuardian && m_initialized))
                        {
                            System.Threading.Thread.Sleep(100);
                            m_pos.PumpMessages();
                            milliseconds -= 200; //from our sleep and the one in PumpMessages
                        }

                        if(ConnectedToGuardian && m_initialized)// && !m_GuardianIsSuspended)
                        {
                            try
                            {
                                UpdateDeviceStates();
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            throw new Exception("Timeout.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Invalid Guardian address.");
                }
            }
            catch(Exception e)
            {
                throw new Exception("ConnectToGuardian failed.  " + e.Message);
            }
        }

        /// <summary>
        /// Writes the 16 bytes for a decimal type to a given user memory block in non-volitile memory.
        /// </summary>
        /// <param name="blockNumber">0 based block number to use.</param>
        /// <param name="value">value to write.</param>
        public void NVMWriteDecimal(int blockNumber, decimal value, bool testMeter = false)
        {
            GetUserNVMemorySize();

            if((blockNumber + 1) * 16 <= m_userNVMSize)
            {
                Int32[] decimalAsDWORDs = decimal.GetBits(value); //get 4 DWORDs representing the decimal
                byte[] writeBuffer = new byte[16]; //get a place to put the 16 bytes for writing
                byte[] tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[0]); //temp array for 4 bytes at a time, get the first 4 bytes

                //put the bytes in the write buffer
                writeBuffer[0] = tempBuffer[0];
                writeBuffer[1] = tempBuffer[1];
                writeBuffer[2] = tempBuffer[2];
                writeBuffer[3] = tempBuffer[3];

                tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[1]); //get the next 4 bytes

                //put the bytes in the write buffer
                writeBuffer[4] = tempBuffer[0];
                writeBuffer[5] = tempBuffer[1];
                writeBuffer[6] = tempBuffer[2];
                writeBuffer[7] = tempBuffer[3];

                tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[2]); //get the next 4 bytes

                //put the bytes in the write buffer
                writeBuffer[8] = tempBuffer[0];
                writeBuffer[9] = tempBuffer[1];
                writeBuffer[10] = tempBuffer[2];
                writeBuffer[11] = tempBuffer[3];

                tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[3]); //get the next 4 bytes

                //put the bytes in the write buffer
                writeBuffer[12] = tempBuffer[0];
                writeBuffer[13] = tempBuffer[1];
                writeBuffer[14] = tempBuffer[2];
                writeBuffer[15] = tempBuffer[3];

                if (m_pretendingWeHaveAGuardian)
                {
                    if (testMeter)
                        WriteData(writeBuffer, 0, 4096 + (uint)(blockNumber * 16), 16); //write to file (only one meter value)
                    else
                        WriteData(writeBuffer, 0, (uint)(blockNumber * 16), 16); //write to file
                }
                else
                {
                    m_consumer.WriteConsumerData(writeBuffer, 0, (uint)(blockNumber * 16), 16); //write the buffer to NVRAM
                }
            }
            else
            {
                throw new Exception("NVMWriteDecimal: Block exceeds user data.");
            }
        }

        public void NVMWriteDecimal(int firstBlock, int lastBlock, Tuple<decimal, bool>[] data)
        {
            if(firstBlock > lastBlock) //make sure we are moving down the list
            {
                int tmp = lastBlock;

                lastBlock = firstBlock;
                firstBlock = tmp;
            }

            GetUserNVMemorySize();

            if((lastBlock + 1) * 16 <= m_userNVMSize)
            {
                byte[] writeBuffer = new byte[16 * ((lastBlock - firstBlock) + 1)]; //get a place to put the 16 bytes per element for writing
                int offset = 0;
                byte[] tempBuffer = null;

                for(int x = firstBlock; x <= lastBlock; x++)
                {
                    Int32[] decimalAsDWORDs = decimal.GetBits(data[x].Item1); //get 4 DWORDs representing the decimal

                    tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[0]); //temp array for 4 bytes at a time, get the first 4 bytes

                    //put the bytes in the write buffer
                    writeBuffer[offset++] = tempBuffer[0];
                    writeBuffer[offset++] = tempBuffer[1];
                    writeBuffer[offset++] = tempBuffer[2];
                    writeBuffer[offset++] = tempBuffer[3];

                    tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[1]); //get the next 4 bytes

                    //put the bytes in the write buffer
                    writeBuffer[offset++] = tempBuffer[0];
                    writeBuffer[offset++] = tempBuffer[1];
                    writeBuffer[offset++] = tempBuffer[2];
                    writeBuffer[offset++] = tempBuffer[3];

                    tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[2]); //get the next 4 bytes

                    //put the bytes in the write buffer
                    writeBuffer[offset++] = tempBuffer[0];
                    writeBuffer[offset++] = tempBuffer[1];
                    writeBuffer[offset++] = tempBuffer[2];
                    writeBuffer[offset++] = tempBuffer[3];

                    tempBuffer = BitConverter.GetBytes(decimalAsDWORDs[3]); //get the next 4 bytes

                    //put the bytes in the write buffer
                    writeBuffer[offset++] = tempBuffer[0];
                    writeBuffer[offset++] = tempBuffer[1];
                    writeBuffer[offset++] = tempBuffer[2];
                    writeBuffer[offset++] = tempBuffer[3];
                }

                if(m_pretendingWeHaveAGuardian)
                    WriteData(writeBuffer, 0, (uint)(firstBlock * 16), (uint)offset); //write to file
                else
                    m_consumer.WriteConsumerData(writeBuffer, 0, (uint)(firstBlock * 16), (uint)offset); //write the buffer to NVRAM
            }
            else
            {
                throw new Exception("NVMWriteDecimal: Block exceeds user data.");
            }
        }

        public void NVMClearDecimal(int firstBlock, int lastBlock, ref Tuple<decimal, bool>[] data)
        {
            if(firstBlock > lastBlock) //make sure we are moving down the list
            {
                int tmp = lastBlock;

                lastBlock = firstBlock;
                firstBlock = tmp;
            }

            GetUserNVMemorySize();

            if((lastBlock + 1) * 16 <= m_userNVMSize)
            {
                byte[] writeBuffer = new byte[16 * ((lastBlock - firstBlock) + 1)]; //get a place for the 16 bytes per element for writing (initialized to '\x00')

                if(m_pretendingWeHaveAGuardian)
                    WriteData(writeBuffer, 0, (uint)(firstBlock * 16), (uint)(16 * (lastBlock - firstBlock) + 16)); //write to file
                else
                    m_consumer.WriteConsumerData(writeBuffer, 0, (uint)(firstBlock * 16), (uint)(16 * (lastBlock - firstBlock) + 16)); //write the buffer to NVRAM

                for(int x = firstBlock; x <= lastBlock; x++)
                    data[x] = new Tuple<decimal, bool>(0, false);
            }
            else
            {
                throw new Exception("NVMWriteDecimal: Block exceeds user data.");
            }
        }

        public decimal NVMReadDecimal(int blockNumber, bool testMeter = false)
        {
            decimal result = 0; //reconstructed decimal

            GetUserNVMemorySize();

            if((blockNumber + 1) * 16 <= m_userNVMSize)
            {
                byte[] readBuffer = m_pretendingWeHaveAGuardian? ReadData((uint)(((testMeter? 4096 : 0) + blockNumber) * 16), 16).Data : m_consumer.ReadConsumerData((uint)(blockNumber * 16), 16).Data; //read decimal from NVRAM
                Int32[] decimalAsDWORDs = new Int32[4]; //4 DWORDs representing the decimal
                byte[] tempBuffer = new byte[4]; //temp array for 4 bytes at a time

                //get the first 4 bytes from the buffer
                tempBuffer[0] = readBuffer[0];
                tempBuffer[1] = readBuffer[1];
                tempBuffer[2] = readBuffer[2];
                tempBuffer[3] = readBuffer[3];

                //use the 4 bytes as the first DWORD of the decimal
                decimalAsDWORDs[0] = BitConverter.ToInt32(tempBuffer, 0);

                //get the next 4 bytes from the buffer
                tempBuffer[0] = readBuffer[4];
                tempBuffer[1] = readBuffer[5];
                tempBuffer[2] = readBuffer[6];
                tempBuffer[3] = readBuffer[7];

                //use the 4 bytes as the second DWORD of the decimal
                decimalAsDWORDs[1] = BitConverter.ToInt32(tempBuffer, 0);

                //get the next 4 bytes from the buffer
                tempBuffer[0] = readBuffer[8];
                tempBuffer[1] = readBuffer[9];
                tempBuffer[2] = readBuffer[10];
                tempBuffer[3] = readBuffer[11];

                //use the 4 bytes as the third DWORD of the decimal
                decimalAsDWORDs[2] = BitConverter.ToInt32(tempBuffer, 0);

                //get the next 4 bytes from the buffer
                tempBuffer[0] = readBuffer[12];
                tempBuffer[1] = readBuffer[13];
                tempBuffer[2] = readBuffer[14];
                tempBuffer[3] = readBuffer[15];

                //use the 4 bytes as the fourth DWORD of the decimal
                decimalAsDWORDs[3] = BitConverter.ToInt32(tempBuffer, 0);

                result = new decimal(decimalAsDWORDs); //use the 4 DWORDs to construct a decimal
            }
            else
            {
                throw new Exception("NVMReadDecimal: Block exceeds user data.");
            }

            return result;
        }


        public ulong NVReadUserMeter(int meterNumber)
        {
            GetUserMetersAvail();

            if(meterNumber > m_userMeters)
                throw new Exception("NVReadUserMeter: User meter number exceeds max.");

            if (m_pretendingWeHaveAGuardian)
                return (ulong)NVMReadDecimal((byte)meterNumber, true);
            else
                return m_consumer.ReadConsumerMeterValue((byte)meterNumber).LifetimeValue;
        }

        public void NVIncrementUserMeter(int meterNumber, ulong value = 1)
        {
            GetUserMetersAvail();

            if(meterNumber > m_userMeters)
                throw new Exception("NVIncrementUserMeter: User meter number exceeds max.");

            if (m_pretendingWeHaveAGuardian)
            {
                decimal work = NVReadUserMeter(meterNumber) + value;
                NVMWriteDecimal(meterNumber, work, true);
            }
            else
            {
                m_consumer.IncrementConsumerMeter((byte)meterNumber, value);
            }
        }

        private void WriteData(byte[] buffer, uint sourceStart, uint memStart, uint bytes)
        {
            m_pretendGuardianMemory.Seek((long)memStart, SeekOrigin.Begin);
            m_pretendGuardianMemory.Write(buffer, (int)sourceStart, (int)bytes);
        }

        private Guardian.NonVolatileStore.DataReadResult ReadData(uint memStart, uint bytes)
        {
            byte[] buffer = new byte[bytes];

            m_pretendGuardianMemory.Seek((long)memStart, SeekOrigin.Begin);
            m_pretendGuardianMemory.Read(buffer, 0, (int)bytes);
            Guardian.NonVolatileStore.DataReadResult result = new Guardian.NonVolatileStore.DataReadResult(DataIntegrity.FULL, buffer, 0, (int)bytes);
            return result;
        }

        public bool TellGuardianWeSuspended(string reason = null)
        {
            return m_consumer.ReportSuspended(reason);
        }

        public bool TellGuardianWeResumed()
        {
            return m_consumer.ReportResumed();
        }

        /// <summary>
        /// Queries the Guardian to see what devices are installed.
        /// Performed automatically as part of ConnectToGuardian(true).
        /// </summary>
        public void UpdateDeviceStates()
        {
            GetAcceptorState();
            GetDispensorState();
            GetUserNVMemorySize();
            GetUserMetersAvail();
        }

        private void GetUserMetersAvail()
        {
            if (m_userMeters == null)
            {
                if (m_pretendingWeHaveAGuardian)
                    m_userMeters = 10;
                else
                    m_userMeters = m_consumer.StorageStatus.MetersAvailable;
            }
        }

        private void GetUserNVMemorySize()
        {
            if(m_userNVMSize == null)
            {
                if (m_pretendingWeHaveAGuardian)
                {
                    m_userNVMSize = 4096;
                }
                else
                {
                    m_consumer.OpenConsumerStorage();
                    m_userNVMSize = m_consumer.StorageStatus.DataCapacity;
                }
            }
        }

        private void GetAcceptorState()
        {
            if(!m_pretendingWeHaveAGuardian && m_acceptorState != DeviceState.Active)
                m_acceptorState = m_consumer.AcceptsItemType(Guardian.Acceptors.AcceptorItemTypes.DENOMINATION) != Guardian.FeatureSupport.UNSUPPORTED || m_consumer.AcceptsItemType(Guardian.Acceptors.AcceptorItemTypes.BARCODE) != Guardian.FeatureSupport.UNSUPPORTED ? DeviceState.Ready : DeviceState.NotInstalled;
        }

        private void GetDispensorState()
        {
            if(!m_pretendingWeHaveAGuardian && m_dispenserState != DeviceState.Active)
                m_dispenserState = m_consumer.DispensesMoney != Guardian.FeatureSupport.UNSUPPORTED ? DeviceState.Ready : DeviceState.NotInstalled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activate"></param>
        /// <param name="amountToCover"></param>
        /// <returns>True if it worked.</returns>
        public bool ActivateBillAcceptor(bool activate = true, decimal amountToCover = 0M, bool suspendMode = false)
        {
            if (m_pretendingWeHaveAGuardian)
                return true;

            if(!suspendMode && AcceptorState == DeviceState.Suspended)
                return false; //can't activate or deactivate when suspended

            if(suspendMode && AcceptorState == DeviceState.Suspended && !activate)
                return true; //already suspended

            if(AcceptorState != DeviceState.Suspended)
                GetAcceptorState(); //used in case the state is unknown

            if(AcceptorState == DeviceState.NotInstalled)
                return !activate;

            if(m_waitingForAcceptorCancel == CancelState.Waiting) //shouldn't enter while waiting for cancel
                return false;

            bool result = true;
            
            if(activate) //start or resume
            {
                Guardian.Acceptors.ItemAcceptRequest req = null;

                m_amountToCover = suspendMode ? m_suspendedAmountToCover : amountToCover;

                if(AcceptorState == DeviceState.Ready || (suspendMode && AcceptorState == DeviceState.Suspended))
                {
                    m_amountAccepted = suspendMode ? m_suspendedAmountAccepted : 0M;

                    req = new Guardian.Acceptors.ItemAcceptRequest(new List<Guardian.Acceptors.AcceptorItemTypes> { Guardian.Acceptors.AcceptorItemTypes.BARCODE, Guardian.Acceptors.AcceptorItemTypes.DENOMINATION });

                    if(!m_consumer.SubmitActionRequest(req))
                    {
                        //couldn't request bills, see if the acceptor is still usable
                        if(m_consumer.AcceptsItemType(Guardian.Acceptors.AcceptorItemTypes.DENOMINATION) == Guardian.FeatureSupport.UNAVAILABLE || m_consumer.AcceptsItemType(Guardian.Acceptors.AcceptorItemTypes.BARCODE) == Guardian.FeatureSupport.UNAVAILABLE)
                            return false; //off-line

                        //try to cancel the last request immediately and again if needed 6 seconds later
                        if(!CancelAcceptorRequest()) //couldn't cancel, give up
                        {
                            m_acceptorState = DeviceState.Ready;
                            return false;
                        }

                        if(!m_consumer.SubmitActionRequest(req))
                        {
                            m_acceptorState = DeviceState.Ready;
                            return false;
                        }
                    }
                }

                //save the request in NV ram in case we need to cancel after a restart.
                WriteNVDataBlock(LastAcceptorRequest, req.GetBytes());
                m_lastAcceptorRequest = req.RequestID;

                m_acceptorState = DeviceState.Active;
            }
            else //deactivate or suspend
            {
                if(AcceptorState == DeviceState.Active)
                {
                    result = CancelAcceptorRequest();

                    if(suspendMode && result)
                    {
                        m_acceptorState = DeviceState.Suspended;
                        m_suspendedAmountToCover = m_amountToCover;
                        m_suspendedAmountAccepted = m_amountAccepted;
                    }
                }
            }

            return result;
        }

        public bool SuspendBillAcceptor(bool suspend = true)
        {
            return ActivateBillAcceptor(!suspend, 0M, true);
        }

        private bool CancelAcceptorRequest()
        {
            if (m_pretendingWeHaveAGuardian)
                return true;

            Guardian.Acceptors.ItemAcceptRequest req = null;
            byte[] r = ReadNVDataBlock(LastAcceptorRequest);

            if(r.Length == 0) //can't cancel if I don't have the original request
                return false;

            Guardian.Acceptors.ItemAcceptRequest.FromBytes(out req, r);

            m_acceptorState = DeviceState.Active;
            m_waitingForAcceptorCancel = CancelState.Waiting;

            if(m_consumer.ConcludeActionRequest(req.RequestID)) //wait for the reply
            {
                while (m_waitingForAcceptorCancel == CancelState.Waiting)
                {
                    System.Threading.Thread.Sleep(100);
                    m_pos.PumpMessages();
                }

                if(m_waitingForAcceptorCancel == CancelState.NotCanceled)
                    return false;
            }
            else //couldn't request the cancel
            {
                m_waitingForAcceptorCancel = CancelState.NotCanceled;
                return false;
            }

            return true;
        }

        public decimal GetAmountAcceptedButNotReported()
        {
            if (m_pretendingWeHaveAGuardian)
                return 0M;

            Guardian.Acceptors.ItemAcceptRequest req = null;
            byte[] r = ReadNVDataBlock(LastAcceptorRequest);

            if(r.Length == 0) //no previous unfinished request 
                return 0M;

            Guardian.Acceptors.ItemAcceptRequest.FromBytes(out req, r);

            if(m_consumer.GetActionRequestState(req.RequestID).Concluded) //see what we missed
            {
                decimal? val = null;

                try
                {
                    val = ((Guardian.Acceptors.ItemAcceptResult)m_consumer.GetActionResult(req.RequestID)).Value;
                }
                catch(Exception)
                {
                }

                return val != null ? (decimal)val : 0M;
            }

            return 0M;
        }

        public decimal GetAmountDispensedButNotReported()
        {
            if (m_pretendingWeHaveAGuardian)
                return 0M;

            Guardian.Dispensers.ItemDispensationRequest req = null;
            byte[] r = ReadNVDataBlock(LastDispenserRequest);

            if(r.Length == 0) //no previous unfinished request 
                return 0M;

            Guardian.Dispensers.ItemDispensationRequest.FromBytes(out req, r);

            if(m_consumer.GetActionRequestState(req.RequestID).Concluded) //see what we missed
            {
                decimal? val = null;

                try
                {
                    val = ((Guardian.Dispensers.ItemDispensationResult2)m_consumer.GetActionResult(req.RequestID)).Amount;
                }
                catch(Exception)
                {
                }

                return val != null ? (decimal)val : 0M;
            }

            return 0M;
        }

        /// <summary>
        /// Dispenses money through the Guardian.
        /// </summary>
        /// <param name="amount">Amount to dispense.</param>
        /// <returns>Amount dispensed.</returns>
        public decimal DispenseMoney(decimal amount)
        {
            if (m_pretendingWeHaveAGuardian)
                return amount;

            GetDispensorState(); //used in case the state is unknown

            if(m_dispenserState == DeviceState.NotInstalled)
                return 0; //nothing dispensed

            if(m_dispenserState == DeviceState.Active) //still dispensing
                return 0;

            amount = decimal.Floor(Math.Abs(amount)); //can't dispense pennies
            m_amountDispensed = 0M;
            Guardian.Dispensers.ItemDispensationRequest req = new Guardian.Dispensers.ItemDispensationRequest(amount);

            m_dispenserState = DeviceState.Active;

            if(m_consumer.SubmitActionRequest(req)) //wait for the dispenser to finish
            {
                WriteNVDataBlock(LastDispenserRequest, req.GetBytes()); //save the request in case of power failure

                while (m_dispenserState == DeviceState.Active)
                {
                    System.Threading.Thread.Sleep(100);
                    m_pos.PumpMessages();
                }
            }
            else
            {
                m_dispenserState = DeviceState.Ready;
            }

            return m_amountDispensed;
        }

        public void ClearPreviousRequestsFromNVRam()
        {
            EraseNVDataBlock(LastAcceptorRequest);
            EraseNVDataBlock(LastDispenserRequest);
            m_lastAcceptorRequest = null;
        }

        public void WriteNVDataBlock(uint block, byte[] bytes)
        {
            uint size = 0;
            uint offset = 0;

            if(FindBlock(block, out offset, out size)) //see if it is the same size
            {
                if(size != (uint)bytes.Length) //not the same size, get rid of the old block
                {
                    EraseNVDataBlock(offset, size);

                    //find where the new block goes
                    FindBlock(block, out offset, out size);
                }
            }

            if (m_pretendingWeHaveAGuardian)
            {
                WriteData(BitConverter.GetBytes(block), 0, offset, sizeof(uint));
                WriteData(BitConverter.GetBytes((uint)bytes.Length), 0, offset + sizeof(uint), sizeof(uint));
                WriteData(bytes, 0, offset + NVDataBlockHeaderSize, (uint)bytes.Length);
            }
            else
            {
                m_consumer.WriteConsumerData(BitConverter.GetBytes(block), 0, offset, sizeof(uint));
                m_consumer.WriteConsumerData(BitConverter.GetBytes((uint)bytes.Length), 0, offset + sizeof(uint), sizeof(uint));
                m_consumer.WriteConsumerData(bytes, 0, offset + NVDataBlockHeaderSize, (uint)bytes.Length);
            }
        }

        public byte[] ReadNVDataBlock(uint block)
        {
            uint size = 0;
            uint offset = 0;

            if(FindBlock(block, out offset, out size))
            {
                Guardian.NonVolatileStore.DataReadResult readResult = m_pretendingWeHaveAGuardian? ReadData(offset + NVDataBlockHeaderSize, size) : m_consumer.ReadConsumerData(offset + NVDataBlockHeaderSize, size);

                if(readResult == null)
                    throw new Exception("Data read failure reading data block from NV ram.");

                if(readResult.Integrity != DataIntegrity.FULL)
                    throw new Exception("Data integrity error reading data block from NV ram.");

                return readResult.Data;
            }
            else
            {
                return new byte[0];
            }
        }

        public void EraseNVDataBlock(uint block)
        {
            if(block != 0) //block numbers start at 1
            {
                uint offset = 0, size = 0;

                if(FindBlock(block, out offset, out size))
                    EraseNVDataBlock(offset, size);
            }
        }

        private void EraseNVDataBlock(uint offset, uint size)
        {
            uint offsetToNewBlock = 0, bytesToMove = 0;

            FindBlock(0, out offsetToNewBlock, out bytesToMove);

            bytesToMove = offsetToNewBlock - offset;

            Guardian.NonVolatileStore.DataReadResult readResult = m_pretendingWeHaveAGuardian? ReadData(offset + NVDataBlockHeaderSize + size, bytesToMove) : m_consumer.ReadConsumerData(offset + NVDataBlockHeaderSize + size, bytesToMove);

            if(readResult == null)
                throw new Exception("Data read failure reading end of data blocks from NV ram.");

            if(readResult.Integrity != DataIntegrity.FULL)
                throw new Exception("Data integrity error reading end of data blocks from NV ram.");

            if(m_pretendingWeHaveAGuardian)
                WriteData(readResult.Data, 0, offset, bytesToMove);
            else
                m_consumer.WriteConsumerData(readResult.Data, 0, offset, bytesToMove);
        }

        /// <summary>
        /// Finds the offset and size of a data block in NV ram for the given data block.
        /// </summary>
        /// <param name="block">Data block to get offset for. Starts at 1.</param>
        /// <param name="offset">Offset where the data block header starts in NV ram.</param>
        /// <param name="size">Size in bytes of the data block.</param>
        /// <returns>True=Block found, False=Block not found and offset is set where the new block should start.</returns>
        private bool FindBlock(uint block, out uint offset, out uint size)
        {
            //block structure is as follows:
            //
            //uint for block number, uint for bytes in block, data bytes

            uint NVOffset = NVDataBlocksStart - NVDataBlockHeaderSize;
            uint NVBlock = 0, NVBlockLen = 0;
            Guardian.NonVolatileStore.DataReadResult readData = null;

            //walk the blocks looking for ours
            do
            {
                NVOffset += 2 * sizeof(uint) + NVBlockLen;
                readData = m_pretendingWeHaveAGuardian? ReadData(NVOffset, NVDataBlockHeaderSize) : m_consumer.ReadConsumerData(NVOffset, NVDataBlockHeaderSize);

                if(readData == null)
                    throw new Exception("Data read failure reading header for data block from NV ram.");

                if(readData.Integrity != DataIntegrity.FULL)
                    throw new Exception("Data integrity error reading header for data block from NV ram.");

                NVBlock = BitConverter.ToUInt32(readData.Data, 0);
                NVBlockLen = BitConverter.ToUInt32(readData.Data, sizeof(uint));
            } while(NVBlock != 0 && NVBlock != block);

            offset = NVOffset;
            size = NVBlockLen;
            return NVBlock != 0;
        }

        #region Consumer events

        void consumer_TamperActivatedChanged(object sender, Guardian.TamperIoManagement.TamperActivatedChangedEventArgs e)
        {
            //            throw new NotImplementedException();
        }

        void consumer_ShutdownFailed(object sender, Guardian.Support.TagEventArgs<string> e)
        {
            //            throw new NotImplementedException();
        }

        void consumer_ShutdownComplete(object sender, EventArgs e)
        {
            //            throw new NotImplementedException();
        }

        void consumer_ActionCompleted(object sender, Guardian.Guardian2ActionCompletedEventArgs e)
        {
            if(e.BaseResult is Guardian.Acceptors.ItemAcceptResult) //bill acceptor completed
            {
                EraseNVDataBlock(LastAcceptorRequest); //get rid of this request since it has been processed
                m_lastAcceptorRequest = null;

                Guardian.Acceptors.ItemAcceptResult result = e.BaseResult as Guardian.Acceptors.ItemAcceptResult;

                if(result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.CANCELLED)
                {
                    m_acceptorState = DeviceState.Ready;
                    m_waitingForAcceptorCancel = CancelState.Canceled;
                }
                else //let any subscribers know and start waiting for more if needed
                {
                    if(m_waitingForAcceptorCancel == CancelState.Waiting)
                    {
                        if(result.ResultState != Guardian.Acceptors.ItemAcceptResultStates.ACCEPTED)
                        {
                            m_acceptorState = DeviceState.Ready;
                            m_waitingForAcceptorCancel = CancelState.Canceled;
                            return;
                        }
                        else
                        {
                            m_waitingForAcceptorCancel = CancelState.NotCanceled;
                        }
                    }

                    OnMoneyAccepted(new Guardian.Acceptors.ItemAcceptResultEventArgs(result));

                    decimal valueAccepted = result.Value == null || result.ResultState != Guardian.Acceptors.ItemAcceptResultStates.ACCEPTED ? 0M : (decimal)result.Value;

                    if (result.Item != null && result.Item.ItemType == Guardian.Acceptors.AcceptorItemTypes.BARCODE && m_pos.Settings.KioskTestTreatTicketAs20 && result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.ACCEPTED)
                    {
                        if(result.Barcode == "000000000000000100")
                            valueAccepted = 1M;
                        else if(result.Barcode == "000000000000000500")
                            valueAccepted = 5M;
                        else if(result.Barcode == "000000000000001000")
                            valueAccepted = 10M;
                        else if(result.Barcode == "000000000000002000")
                            valueAccepted = 20M;
                        else if(result.Barcode == "000000000000005000")
                            valueAccepted = 50M;
                        else if(result.Barcode == "000000000000010000")
                            valueAccepted = 100M;
                        else
                            valueAccepted = 20M;
                    }

                    m_amountAccepted += valueAccepted;

                    if(m_amountToCover == 0M || m_amountAccepted < m_amountToCover) //need more or might need more
                    {
                        Guardian.Acceptors.ItemAcceptRequest req = new Guardian.Acceptors.ItemAcceptRequest(new List<Guardian.Acceptors.AcceptorItemTypes> { Guardian.Acceptors.AcceptorItemTypes.BARCODE, Guardian.Acceptors.AcceptorItemTypes.DENOMINATION });

                        if (!m_consumer.SubmitActionRequest(req))
                        {
                            m_acceptorState = DeviceState.Ready;
                        }
                        else //save the request in NV ram in case we need to cancel after a restart.
                        {
                            WriteNVDataBlock(LastAcceptorRequest, req.GetBytes());
                            m_lastAcceptorRequest = req.RequestID;
                        }
                    }
                    else //no more to get
                    {
                        m_acceptorState = DeviceState.Ready;
                    }
                }
            }
            else if(e.BaseResult is Guardian.Dispensers.ItemDispensationResult2) //bill dispenser completed
            {
                EraseNVDataBlock(LastDispenserRequest); //get rid of this request since it has been processed
                m_amountDispensed = ((Guardian.Dispensers.ItemDispensationResult2)e.BaseResult).Amount;
                m_dispenserState = DeviceState.Ready;
            }
        }

        void consumer_ActionStateChanged(object sender, Guardian.Guardian2ActionStateEventArgs e)
        {
            if (e.RequestID == m_lastAcceptorRequest && e.EventActionState.Processing)
                OnMoneyAccepted();
        }

        void consumer_ConsumerResumeRequested(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_ConsumerResumeRequested()"));
            OnGuardianReleasedControl();
        }

        void consumer_ConsumerSuspendRequested(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_ConsumerSuspendRequested()"));
            OnGuardianRequestedControl();
        }

        void consumer_ConsumerShutdownRequested(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_ConsumerShutdownRequested()"));
            OnGuardianRequestedShutdown();
        }

        void consumer_StorageStatusChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_StorageStatusChanged..."));

            var cc = sender as ConsumerClient;
            if(cc != null)
            {
                var ss = cc.StorageStatus;
                if(ss == null)
                    System.Diagnostics.Debug.WriteLine("\tStorageStatus cleared(null)");
                else
                {
                    var s = String.Format("\t Provided: {0}\r\n\t Data Capacity: {1}\r\n\t Meters: {2}\r\n\t Finalized: {3}"
                        , ss.Provided, ss.DataCapacity, ss.MetersAvailable, ss.FinalizedWhen.HasValue ? ss.FinalizedWhen.Value.ToString() : "open");
                    System.Diagnostics.Debug.WriteLine(s);
                }
            }
            //            throw new NotImplementedException();
        }

        void consumer_StartupFailed(object sender, Guardian.Support.TagEventArgs<string> e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_StartupFailed()"));
            //            throw new NotImplementedException();
        }

        void consumer_StartupComplete(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.consumer_StartupComplete()"));
            m_initialized = true;
        }

        void client_ConnectedChanged(object sender, Guardian.Support.ValueChangedEventArgs<bool> e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.client_ConnectedChanged: Connected = {0}", e.NewValue));
            lock(m_guardianLock)
            {
                this.ConnectedToGuardian = e.NewValue;
            }
        }

        #endregion

        #region Properties
        private bool m_connectedToGuardian;
        public bool ConnectedToGuardian
        {
            get
            {
                if(!m_connectedToGuardian)
                {
                    var st = Environment.StackTrace.ToString();
                    if(!st.Contains("ConnectToGuardian"))
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.ConnectedToGuardian accessed while false."));
                        System.Diagnostics.Debug.WriteLine(st);
                    }
                }

                return m_connectedToGuardian;
            }

            internal set
            {
                if(m_connectedToGuardian != value)
                {
                    if(m_pretendingWeHaveAGuardian)
                        System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.ConnectedToGuardian set to {0} (Pretend test connection)", value));
                    else
                        System.Diagnostics.Debug.WriteLine(String.Format("GuardianWrapper.ConnectedToGuardian set to {0}", value));
    
                    m_connectedToGuardian = value;
                }
            }
        }

        public DeviceState AcceptorState
        {
            get
            {
                if (m_pretendingWeHaveAGuardian)
                    return DeviceState.Ready;
                else
                    return m_acceptorState;
            }
        }

        public DeviceState DispenserState
        {
            get
            {
                if (m_pretendingWeHaveAGuardian)
                    return DeviceState.Ready;
                else
                    return m_dispenserState;
            }
        }

        public IPEndPoint GuardianEndPoint
        {
            get
            {
                return m_guardianEndPoint;
            }

            set
            {
                m_guardianEndPoint = value;
            }
        }

        public bool NeedToCheckForLateMoney
        {
            get
            {
                if (m_pretendingWeHaveAGuardian)
                    return false;
                else
                    return ReadNVDataBlock(LastAcceptorRequest).Length != 0 || ReadNVDataBlock(LastDispenserRequest).Length != 0;
            }
        }

        #endregion

        #region Events
#pragma warning disable 649

        public event EventHandler<Guardian.Acceptors.ItemAcceptResultEventArgs> MoneyAccepted;
        protected virtual void OnMoneyAccepted(Guardian.Acceptors.ItemAcceptResultEventArgs e = null)
        {
            var h = MoneyAccepted;

            if(h != null)
                h(this, e);
        }

        public event EventHandler GuardianRequestedControl;
        protected virtual void OnGuardianRequestedControl(EventArgs e = null)
        {
            var h = GuardianRequestedControl;

            if(h != null)
                h(this, e);
        }

        public event EventHandler GuardianReleasedControl;
        protected virtual void OnGuardianReleasedControl(EventArgs e = null)
        {
            var h = GuardianReleasedControl;

            if(h != null)
                h(this, e);
        }

        public event EventHandler GuardianRequestedShutdown;
        protected virtual void OnGuardianRequestedShutdown(EventArgs e = null)
        {
            var h = GuardianRequestedShutdown;

            if(h != null)
                h(this, e);
        }

#pragma warning restore 649
        #endregion Events

    }
}
