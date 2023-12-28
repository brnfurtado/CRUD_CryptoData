using DomainDefinitions.Interfaces;
using System.Data.Common;
using System.Reflection;
using DomainDefinitions.Enums;

namespace DomainDefinitions.Factories
{
    public class CryptoDataSourceFactory : ICryptoDataSourceFactory
    {
        //Lock to guartantee thread-safety
        private object instanciationLock = new object();
        private Dictionary<string, ICryptoDataSource> instanciatedDataSourceDict = new Dictionary<string, ICryptoDataSource>();
        private const string assemblyName = "DomainDefinitions";
        private const string namespaceName = "CryptoDataSubscriber.Implementations";
        private const string classNameSuffix = "_DataSubscriber";
        private const string methodName = "CreateInstance";

        //Singleton Implementation
        private static object instanceLock = new object();
        private static CryptoDataSourceFactory _instance;

        private CryptoDataSourceFactory()
        {

        }

        public static CryptoDataSourceFactory GetCryptoDataSourceFactory()
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new CryptoDataSourceFactory();
                }

                return _instance;
            }
        }

        public async Task<ICryptoDataSource> GetDataSourceByName(string sourceName)
        {
            sourceName = GetFormattedSourceName(sourceName);

            await CheckDataSourceCreation(sourceName);

            return instanciatedDataSourceDict[sourceName];
        }

        private string GetFormattedSourceName(string sourceName)
        {
            return sourceName[0].ToString().ToUpper() + sourceName.Substring(1, sourceName.Length - 1).ToLower();
        }


        //Using Assembly and MeethodInfo in order not no need to reference the actual class when it's implemented.
        //this way, if a new exchange is added to implemented, there's no need to change the factory code.
        private async Task CheckDataSourceCreation(string sourceName)
        {
            bool instanciate = false;

            lock (instanciationLock)
            {
                if (!instanciatedDataSourceDict.ContainsKey(sourceName))
                {
                    instanciate = true;
                }

                ICryptoDataSource dataSource = null;

                if (instanciate)
                {
                    Assembly.Load(assemblyName);

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        MethodInfo createInstanceMethod;

                        if (assembly.FullName.Contains(assemblyName))
                        {
                            var type = assembly.GetType(assemblyName + "." + namespaceName + "." + sourceName + classNameSuffix);

                            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                            dataSource = (ICryptoDataSource)method.Invoke(null, null);

                            dataSource.StartDataSource();
                        }
                    }

                    if (dataSource == null)
                    {
                        throw new Exception($"Unable to find: [{assemblyName + "." + sourceName + classNameSuffix}]");
                    }

                    instanciatedDataSourceDict.Add(sourceName, dataSource);
                }
            }

        }
    }
}