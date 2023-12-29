using DomainDefinitions.Enums;
using DomainDefinitions.Interfaces;
using DomainDefinitions.AuxStaticCode;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace DomainDefinitions.CryptoDataSubscriber.AbstractClasses
{
    public abstract class AbstractDataSubscriber<T> : ICryptoDataSource
        where T : AbstractDataSubscriber<T>, new()
    {
        #region PublicInterfaceProperties
        public int cryptoDataSourceID { get; private set; }
        public string cryptoDataSourceName { get; private set; }
        public List<string> listValidCryptoPairs { get; private set; }
        #endregion


        #region PrivateProperties
        //The lock is necessary in case there are multiple subscribers to the Crypto Data. However, the current code application uses only one DBCallback, so the lock is redundant.
        private object callbackDictLock = new object();
        private Dictionary<string, Dictionary<int, long>> lastCallbackCalled = new Dictionary<string, Dictionary<int, long>>();
        private Dictionary<string, Dictionary<EFrequency, List<ICryptoDataCallback>>> callbackDict = new Dictionary<string, Dictionary<EFrequency, List<ICryptoDataCallback>>>();

        private Dictionary<string, long> lastUpdateTimer = new Dictionary<string, long>();

        //Used to optimize requests in case there's more than one frequency subscribed for the same CryptoPair.
        private Dictionary<string, EFrequency> minTimerSubscribed = new Dictionary<string, EFrequency>();

        //Using blockingCollection to ensure syncronicity between events for the same CryptoPair. In the context of this application might not be necessary.
        //however, for faster market data, using async methods inside the callbacks can lead to inversions.
        private Dictionary<string, BlockingCollection<(string, decimal[])>> blockingCollectionUpdates = new Dictionary<string, BlockingCollection<(string, decimal[])>>();


        //Singleton implementation
        protected static T instance;
        protected static object lockInstance = new object();
        #endregion

        #region PubicMethods
        public void StartDataSource()
        {
            BaseStartDataSource();
            AdditionalStartDataSource();
        }

        public virtual void AdditionalStartDataSource()
        {
            //Do nothing by default, but in case some child class needs to do something before starting, it can override this method.
        }

        private async Task BaseStartDataSource()
        {
            //Saving the valid subscribeable cryptoPairs.
            listValidCryptoPairs = AuxStaticCode.AuxStaticCode.validCryptoPairs;
        }

        public async Task<bool> SubscribeCryptoData(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback)
        {
            if (!listValidCryptoPairs.Contains(cryptoPair))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error, subscription [{cryptoPair}-{frequency}] is not valid for [{cryptoDataSourceName}]");
                return false;
            }

            bool startSubscription = CheckStartSubscription(cryptoPair, frequency, callback);

            SetOptimizedBookSubscriptions(cryptoPair, frequency);

            if (startSubscription)
            {
                //Using a separated thread, since this method can run for an indefinate amount of time
                Task.Factory.StartNew(() => LoopMakeHTTPRequest(cryptoPair));
            }

            return true;
        }

        public async Task<bool> UnsubscribeCryptoData(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback)
        {
            if (!callbackDict.ContainsKey(cryptoPair))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error,there is no subscription for [{cryptoPair}-{frequency}]");
                return false;
            }

            if (!callbackDict[cryptoPair].ContainsKey(frequency))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error,there is no subscription for [{cryptoPair}-{frequency}]");
                return false;
            }

            if (!callbackDict[cryptoPair][frequency].Contains(callback))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error, [{callback.cryptoCallbackID}] is not subscribed for [{cryptoPair}-{frequency}]");
                return false;
            }

            SetOptimizedBookSubscriptions(cryptoPair, frequency);
            RemoveCallbackSubscriber(cryptoPair, frequency, callback);

            return true;
        }

        public async Task<bool> UpdateSubscriptionCryptoData(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback)
        {
            if (!callbackDict.ContainsKey(cryptoPair))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error,there is no subscription for [{cryptoPair}-{frequency}]");
                return false;
            }

            if (!callbackDict[cryptoPair].ContainsKey(frequency))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error,there is no subscription for [{cryptoPair}-{frequency}]");
                return false;
            }

            if (!callbackDict[cryptoPair][frequency].Contains(callback))
            {
                AuxStaticCode.AuxStaticCode.Print($"Error, [{callback.cryptoCallbackID}] is not subscribed for [{cryptoPair}-{frequency}]");
                return false;
            }

            SetOptimizedBookSubscriptions(cryptoPair, frequency);
            return true;
        }
        #endregion

        #region PrivateMethods
        private bool CheckStartSubscription(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback)
        {
            lock (callbackDictLock)
            {
                if (!blockingCollectionUpdates.ContainsKey(cryptoPair))
                {
                    blockingCollectionUpdates.Add(cryptoPair, new BlockingCollection<(string, decimal[])>());

                    //Using longrunning task because the process part can take a long time, since it will loop all subscribers
                    Task.Factory.StartNew(() => LoopProcessBlockingCollection(cryptoPair, frequency), TaskCreationOptions.LongRunning);
                }

                if (!callbackDict.ContainsKey(cryptoPair))
                {
                    callbackDict.Add(cryptoPair, new Dictionary<EFrequency, List<ICryptoDataCallback>>());
                    callbackDict[cryptoPair].Add(frequency, new List<ICryptoDataCallback>());
                    callbackDict[cryptoPair][frequency].Add(callback);
                    lastCallbackCalled.Add(cryptoPair, new Dictionary<int, long>());
                    lastCallbackCalled[cryptoPair].Add(callback.cryptoCallbackID, 0);
                    return true;
                }

                else if (!callbackDict[cryptoPair].ContainsKey(frequency))
                {
                    callbackDict[cryptoPair].Add(frequency, new List<ICryptoDataCallback>());
                    callbackDict[cryptoPair][frequency].Add(callback);
                    lastCallbackCalled[cryptoPair].Add(callback.cryptoCallbackID, 0);
                }

                else if (!callbackDict[cryptoPair][frequency].Contains(callback))
                {
                    callbackDict[cryptoPair][frequency].Add(callback);
                    lastCallbackCalled[cryptoPair].Add(callback.cryptoCallbackID, 0);
                }

                //This means that callbackID is already subscrribed
                else
                {
                    AuxStaticCode.AuxStaticCode.Print($"Error, callback with ID [{callback.cryptoCallbackID}] has already subscribed for [{cryptoPair}-{frequency}]");
                }

                return false;
            }
        }

        private void SetOptimizedBookSubscriptions(string cryptoPair, EFrequency frequency)
        {
            //Using as default the min length for max length, and the max frequency for min frequency, in order to test if the optimized is better than the worst case scenario
            //This was designed for having multiple callback frequencies for the same pair, allowing only one request to be performed for all frequencies.

            EFrequency minFrequency = Enum.GetValues(typeof(EFrequency)).Cast<EFrequency>().Max();

            lock (callbackDictLock)
            {
                foreach (var subscriber in callbackDict[cryptoPair])
                {
                    minFrequency = (EFrequency)Math.Min((int)minFrequency, (int)(subscriber.Key));
                }
            }

            minTimerSubscribed[cryptoPair] = minFrequency;
        }

        private void RemoveCallbackSubscriber(string cryptoPair,  EFrequency frequency, ICryptoDataCallback callback)
        {
            lock (callbackDictLock)
            {
                callbackDict[cryptoPair][frequency].Remove(callback);
            }
        }

        private void AddDataUpdate(string cryptoPair, decimal[] data)
        {
            //Using blocking collection to guarantee that the updates are processed sequentially and done in a separated thread
            blockingCollectionUpdates[cryptoPair].Add((cryptoPair, data));
        }

        private async Task LoopProcessBlockingCollection(string cryptoPair, object data)
        {
            while (blockingCollectionUpdates.ContainsKey(cryptoPair))
            {
                (string, decimal[]) dataUpdate;

                if (blockingCollectionUpdates[cryptoPair].TryTake(out dataUpdate, -1))
                {
                    foreach (var callbackSpecsAndPointer in callbackDict[cryptoPair])
                    {
                        int thisFrequency = (int)callbackSpecsAndPointer.Key;

                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        foreach (var callback in callbackSpecsAndPointer.Value)
                        {
                            if (now - lastCallbackCalled[cryptoPair][callback.cryptoCallbackID] > thisFrequency)
                            {
                                lastCallbackCalled[cryptoPair][callback.cryptoCallbackID] = now;

                                //Not awaited on purpouse, in order for multiple callbacks to be called in parallel
                                await callback.OnDataUpdateCallback(cryptoDataSourceName, dataUpdate.Item1, dataUpdate.Item2);
                            }
                        }
                    }

                    lastUpdateTimer[cryptoPair] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
            }
        }

        #endregion



        #region ProtectedMethods
        protected void SetSelfNameAndID(string name, int ID)
        {
            cryptoDataSourceName = name;
            cryptoDataSourceID = ID;
        }

        protected async void LoopMakeHTTPRequest(string cryptoPair)
        {
            while (minTimerSubscribed.ContainsKey(cryptoPair))
            {
                decimal[] data = await ImplementationGetCryptoData(cryptoPair);

                AddDataUpdate(cryptoPair, data);

                await Task.Delay((int)minTimerSubscribed[cryptoPair]);
            }
        }
        #endregion


        #region AbstractMethods
        //Return type is Task, because the methods can need async implementations
        protected abstract Task<decimal[]> ImplementationGetCryptoData(string cryptoPair);
        protected abstract string GetBookHTTPURI(string bookDepthPath, string cryptoPair);
        protected abstract string ConvertExternalToInternalCryptoPairSymbol(string cryptoPair);
        protected abstract string ConvertInternalToExternalCryptoPairSymbol(string cryptoPair);
        #endregion
    }
}