﻿using System;
using System.Collections.Generic;
using System.Linq;
using NoCompany.Interfaces;
using CodeContracts;
using GenesisTrialTest.Properties;
using log4net;
using System.Reflection;

namespace GenesisTrialTest
{
    public class ChangesNotifierFacade
    {
        public static ILog logger = LogManager.GetLogger(typeof(ChangesNotifierFacade)); 

        private List<string> listOfChanges = new List<string>();
        private List<string> listOfErrors = new List<string>();
        private int _defaultTimeOut = 30 * 1000;
        public IDataAnalyzer Analyzer { get; private set; }
        public IDataProvider ExternalSource { get; private set; }
        public IDataStorageProvider DataStorage { get; private set; }
        
        public INotificationManager Notificator { get; private set; }

        public ChangesNotifierFacade(IDataAnalyzer analyzer, 
                                   IDataProvider externalSource, 
                                   IDataStorageProvider dataStorage, 
                                   INotificationManager notificator)
        {
            Requires.NotNull(analyzer, "analyzer");
            Requires.NotNull(externalSource, "externalSource");
            Requires.NotNull(dataStorage, "dataStorage");
            Requires.NotNull(notificator, "notificator");

            Analyzer = analyzer;
            Analyzer.DetectedDifferenceEvent += Analyzer_DetectedDifferenceEvent;

            ExternalSource = externalSource;
            DataStorage = dataStorage;
            Notificator = notificator;
        }

        protected virtual void Notify()
        {

            if (listOfChanges.Any())
            {
                logger.Debug(MethodBase.GetCurrentMethod().Name);
                Notificator.NotifyAbout(listOfChanges);
            }
        }

        protected virtual void Analyzer_DetectedDifferenceEvent(object sender, string e)
        {
            if (!String.IsNullOrEmpty(e))
                listOfChanges.Add(e);
        }

        protected virtual IEnumerable<IChangeableData> GetExternalData()
        {
            logger.Debug(MethodBase.GetCurrentMethod().Name);
            logger.InfoFormat(Resources.Info_TimeOut, OperationHangTimeOut);

            using (HangWatcher watcher = new HangWatcher(OperationHangTimeOut))
            {
                watcher.Token.Register(() => ExternalSource.Cancel());
                ExternalSource.ImStillAlive += (o, e) => watcher.PostPone(OperationHangTimeOut);
                
                return ExternalSource.GetData();
            }
        }

        /// <summary>
        /// Limits a time consumable operation in Milliseconds. 
        /// If time is out the OperationCanceledException will be raised.
        /// </summary>
        public int OperationHangTimeOut
        {
            get { return _defaultTimeOut; }
            set
            {
                _defaultTimeOut = value;
                logger.InfoFormat(Resources.Info_TimeOutChange, value);

            }
        }
        public void FindAndNotify()
        {
            logger.Debug(MethodBase.GetCurrentMethod().Name);

            //Get Fresh data
            var receivedData = GetExternalData();
            //Get old data
            var presavedData = DataStorage.GetData();

            Assumes.True(receivedData != null && receivedData.Any(), Resources.Error_LoadExternalData) ;
            try
            {
                if (presavedData != null)
                {
                    Analyzer.Analyze(receivedData, presavedData);
                    Notify();
                }
                else
                {
                    logger.InfoFormat(Resources.Info_NoPreservedData);

                }
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                logger.Error(ex);
            }
            finally
            {
                // Clear All old data
                DataStorage.CleanStorage();
                // Save new data
                DataStorage.SaveData(receivedData);
            }
            
        }
    }
}
