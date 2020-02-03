namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using Oracle.ManagedDataAccess.Client;

    public enum Gateway
    {
        TOM_DRV,
        TOM_EQ,
        UTP_EQ,
        UTP,
        PERSHING,
        UTP_FUNDS,
        BEST,
        GS,
        NSCALEX,
        NSCBINCK,
        FORTIS,
        UTP_DRE
    };

    public class CmfLog
    {
        private readonly OracleConnection connection;
        private readonly Gateway gateway;

        public CmfLog(Gateway gateway, DbSettings dbSetting)
        {
            this.gateway = gateway;
            this.connection = new OracleConnection(
                string.Format("Data Source={0};User ID={1};Password={2}", dbSetting.Name, dbSetting.User,
                    dbSetting.Password));
            this.connection.Open();
        }

        public DataTable GetLogDataSet(DateTime from)
        {
            DataSet ds = new DataSet();

            OracleDataAdapter adapter = new OracleDataAdapter(
                GetQueryFromTime(from),
                this.connection);

            adapter.Fill(ds);

            return ds.Tables[0];
        }

        private string GetQueryFromTime(DateTime from)
        {
            double days = DateTime.Now.Subtract(from).TotalDays + TimeSpan.FromHours(1).TotalDays;
            return
                string.Format(CultureInfo.InvariantCulture,
                    "SELECT t.* FROM {0} t WHERE t.submit_time > sysdate - {1} ORDER BY t.submit_time",
                    CmfLogTables.Instance[this.gateway],
                    days);
        }
    }

    internal class CmfLogTables
    {
        private static readonly CmfLogTables instance = new CmfLogTables();

        private readonly Dictionary<Gateway, string> tables = new Dictionary<Gateway, string>();

        private CmfLogTables()
        {
            this.tables.Add(Gateway.TOM_DRV, "fintexch.TOM_DRV_MSG_LOG");
            this.tables.Add(Gateway.UTP_EQ, "fintexch.UTP_EQ_MSG_LOG");
            this.tables.Add(Gateway.UTP, "fintexch.UTP_MSG_LOG");
            this.tables.Add(Gateway.TOM_EQ, "fintexch.TOM_EQ_MSG_LOG");
            this.tables.Add(Gateway.UTP_FUNDS, "fintexch.UTP_FUNDS_MSG_LOG");
            this.tables.Add(Gateway.PERSHING, "fintexch.PERSHING_MSG_LOG");
            this.tables.Add(Gateway.BEST, "fintexch.CMF_BEST_LOG");
            this.tables.Add(Gateway.GS, "fintexch.CMF_GS_LOG");
            this.tables.Add(Gateway.NSCALEX, "fintexch.CMF_NSCALEX_LOG");
            this.tables.Add(Gateway.NSCBINCK, "fintexch.CMF_NSCBINCK_LOG");
            this.tables.Add(Gateway.FORTIS, "fintexch.FORTIS_MSG_LOG");
            this.tables.Add(Gateway.UTP_DRE, "fintexch.UTP_DRE_MSG_LOG");
        }

        public static CmfLogTables Instance
        {
            get { return instance; }
        }

        public string this[Gateway gateway]
        {
            get { return this.tables[gateway]; }
        }
    }
}