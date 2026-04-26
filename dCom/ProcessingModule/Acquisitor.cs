using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for periodic polling.
    /// </summary>
    public class Acquisitor : IDisposable
	{
		private AutoResetEvent acquisitionTrigger;
        private IProcessingManager processingManager;
        private Thread acquisitionWorker;
		private IStateUpdater stateUpdater;
		private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Acquisitor"/> class.
        /// </summary>
        /// <param name="acquisitionTrigger">The acquisition trigger.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="stateUpdater">The state updater.</param>
        /// <param name="configuration">The configuration.</param>
		public Acquisitor(AutoResetEvent acquisitionTrigger, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.acquisitionTrigger = acquisitionTrigger;
			this.processingManager = processingManager;
			this.configuration = configuration;
			this.InitializeAcquisitionThread();
			this.StartAcquisitionThread();
		}

		#region Private Methods

        /// <summary>
        /// Initializes the acquisition thread.
        /// </summary>
		private void InitializeAcquisitionThread()
		{
			this.acquisitionWorker = new Thread(Acquisition_DoWork);
			this.acquisitionWorker.Name = "Acquisition thread";
		}

        /// <summary>
        /// Starts the acquisition thread.
        /// </summary>
		private void StartAcquisitionThread()
		{
			acquisitionWorker.Start();
		}

        /// <summary>
        /// Periodicno ocitavanje podataka sa Modbus uredjaja.
        /// U posebnoj niti salje read komande za sve tacke
        /// u zavisnosti od njihovog intervala ocitavanja.
        /// </summary>
        private void Acquisition_DoWork()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    List<IConfigItem> configItems = configuration.GetConfigurationItems();

                    foreach (IConfigItem configItem in configItems)
                    {
                        configItem.SecondsPassedSinceLastPoll++;

                        if (configItem.SecondsPassedSinceLastPoll >= configItem.AcquisitionInterval)
                        {
                            ushort transactionId = configuration.GetTransactionId();
                            byte unitAddress = configuration.UnitAddress;
                            ushort startAddress = configItem.StartAddress;
                            ushort numberOfRegisters = configItem.NumberOfRegisters;

                            processingManager.ExecuteReadCommand(
                                configItem,
                                transactionId,
                                unitAddress,
                                startAddress,
                                numberOfRegisters);

                            configItem.SecondsPassedSinceLastPoll = 0;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                stateUpdater.LogMessage(ex.Message);
            }
        }

        #endregion Private Methods

        /// <inheritdoc />
        public void Dispose()
		{
			acquisitionWorker.Abort();
        }
	}
}