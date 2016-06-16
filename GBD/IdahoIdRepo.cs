namespace GBD
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class IdahoIdRepo
    {
        private static IdahoIdRepo localInstance = new IdahoIdRepo();

        private static Dictionary<string, HashSet<string>> msIds = new Dictionary<string, HashSet<string>>();

        private static Dictionary<string, HashSet<string>> panIds = new Dictionary<string, HashSet<string>>();

        private static object msLockObject = new object();

        private static object panLockObject = new object();

        private IdahoIdRepo()
        {
        }

        public static IdahoIdRepo Instance
        {
            get
            {
                return localInstance;
            }
        }

        public static void ClearMsRepo()
        {
            msIds.Clear();
        }

        public static void ClearPanRepo()
        {
            panIds.Clear();
        }

        public static HashSet<string> GetMsIdahoIds(string catalogId)
        {
            if (!msIds.ContainsKey(catalogId)) return null;
            return msIds[catalogId];
        }

        public static HashSet<string> GetPanIdahoIds(string catalogId)
        {
            if (!panIds.ContainsKey(catalogId)) return null;
            return panIds[catalogId];
        }

        public static void AddSingleMsId(string catalogId, string idahoId)
        {
            AddSingleId(catalogId, idahoId, "MS", msLockObject);
        }

        public static void AddSinglePanId(string catalogId, string idahoId)
        {
            AddSingleId(catalogId, idahoId, "PAN", panLockObject);
        }

        private static void AddSingleId(string catalogId, string idahoId, string colorInterp, object lockObject)
        {
            if (colorInterp.Equals("PAN"))
            {
                lock (lockObject)
                {
                    if (panIds.ContainsKey(catalogId))
                    {
                        panIds[catalogId].Add(idahoId);
                    }
                    else
                    {
                        panIds.Add(catalogId, new HashSet<string> { idahoId });
                    }
                }
            }
            else if (colorInterp.Equals("MS"))
            {
                lock (lockObject)
                {
                    if (msIds.ContainsKey(catalogId))
                    {
                        msIds[catalogId].Add(idahoId);
                    }
                    else
                    {
                        msIds.Add(catalogId, new HashSet<string> { idahoId });
                    }
                }
            }
        }
    }
}