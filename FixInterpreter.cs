namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class FixInterpreter
    {
        protected FixMessage message;

        public string Interpretation
        {
            get { return GetInterpretation(); }
        }

        public static FixInterpreter FromMessage(FixMessage msg)
        {
            FixInterpreter intr;

            if ((msg == null) || (msg[35] == null))
                intr = new FixInterpreterUnknown();
            else
            {
                Type t = Type.GetType("FixAnalyzer.FixInterpreter" + msg[35].Value);

                if (t == null)
                    t = typeof (FixInterpreterUnknown);
                intr = (FixInterpreter) Activator.CreateInstance(t);
            }


            intr.SetMessage(msg);
            return intr;
        }

        private static string GetClassName(string msgType)
        {
            return "FixAnalyzer.FixInterpreter" + msgType;
        }

        public abstract string GetInterpretation();

        protected abstract void Read(FixMessage msg);

        private void SetMessage(FixMessage msg)
        {
            this.message = msg;
            Read(msg);
        }
    }

    public class FixInterpreterUnknown : FixInterpreter
    {
        public override string GetInterpretation()
        {
            return this.message.Raw;
        }

        protected override void Read(FixMessage msg)
        {
            // no interpretation
        }
    }

    public enum OrderSide
    {
        Buy,
        Sell,
        Undefined
    };

    public enum CallPut
    {
        Call,
        Put
    };

    public class FixInterpreterD : FixInterpreter
    {
        private string clOrdId;
        private InstrumentInterpreter instrument;
        private OrderSide side;

        public override string GetInterpretation()
        {
            return string.Format("NewOrder ({0}) - {1} {2}",
                this.clOrdId,
                this.side.ToString(),
                this.instrument.GetInterpretation()
                );
        }

        protected override void Read(FixMessage msg)
        {
            this.clOrdId = msg[11].Value;
            if (msg[54] == null)
                this.side = OrderSide.Undefined;
            else if (msg[54].Value == "1")
                this.side = OrderSide.Buy;
            else
                this.side = OrderSide.Sell;


            this.instrument = InstrumentInterpreter.FromMessage(msg);
        }
    }

    public class FixInterpreter0 : FixInterpreter
    {
        public override string GetInterpretation()
        {
            return "Heart Beat";
        }

        protected override void Read(FixMessage msg)
        {
            //
        }
    }

    public class FixInterpreter1 : FixInterpreter
    {
        public override string GetInterpretation()
        {
            return "Test Request";
        }

        protected override void Read(FixMessage msg)
        {
            //
        }
    }

    public class FixInterpreterAB : FixInterpreter
    {
        private readonly List<LegInterpreter> legs = new List<LegInterpreter>();
        private long clOrdId;
        private string fix;

        public override string GetInterpretation()
        {
            return string.Format("NewMultiLegOrder ({0}) -- Leg1[ {1} ] -- Leg2[ {2} ]",
                this.clOrdId,
                this.legs[0].GetInterpretation(),
                this.legs[1].GetInterpretation()
                );
        }

        protected override void Read(FixMessage msg)
        {
            List<FixTag> leg = new List<FixTag>();
            this.fix = msg.Raw;
            bool readingLeg = false;
            this.clOrdId = Int64.Parse(msg[11].Value);

            foreach (FixTag tag in msg.Tags)
            {
                if (tag.Tag == 600)
                {
                    if (readingLeg)
                        this.legs.Add(new LegInterpreter(msg, leg.ToArray()));
                    leg.Clear();
                    readingLeg = true;
                }
                if (readingLeg)
                    leg.Add(tag);
            }
            if (readingLeg)
                this.legs.Add(new LegInterpreter(msg, leg.ToArray()));
        }
    }

    public class FixInterpreterF : FixInterpreter
    {
        private string clOrdId;
        private string orgClOrdId;

        public override string GetInterpretation()
        {
            return string.Format("OrderCancelRequest (ClOrdId={0} OrigClOrdId={1})", this.clOrdId, this.orgClOrdId);
        }

        protected override void Read(FixMessage msg)
        {
            this.orgClOrdId = msg[41].Value;
            this.clOrdId = msg[11].Value;
        }
    }

    public enum OrderStatus
    {
        Unknown,
        NotFilled,
        New,
        Partial,
        Filled,
        Done,
        Canceled,
        Stopped,
        Rejected,
        Suspended,
        PendingNew,
        Calculated,
        Expired,
        AcceptBidding
    };

    public enum ExecType
    {
        New,
        Unused1,
        Unsued2,
        Done,
        Canceled,
        Replaced,
        PendingCxl,
        Stopped,
        Rejected,
        Suspended,
        Unused10,
        PendingNew,
        Calculated,
        Expired,
        Restated,
        PendingReplace,
        Trade,
        TradeCorrect,
        TradeCancel,
        OrderStatus,
        Unknown
    };

    public class FixInterpreter8 : FixInterpreter
    {
        private string clOrdId;
        private ExecType execType;
        private string orderId;
        private OrderStatus orderStatus;

        public override string GetInterpretation()
        {
            return string.Format("ExecutionReport/{0} ({1}) - OrderStatus: {2}",
                this.execType.ToString(),
                this.clOrdId,
                this.orderStatus.ToString()
                );
        }

        protected override void Read(FixMessage msg)
        {
            this.clOrdId = (msg[11] != null) ? msg[11].Value : "";
            this.orderId = (msg[37] != null) ? msg[37].Value : "";
            this.orderStatus = msg[39] != null ? GetOrderStatus(msg[39].Value) : OrderStatus.NotFilled;
            if (msg[150] != null)
            {
                this.execType = GetExecType(msg[150].Value);
            }
            else
            {
                this.execType = ExecType.Unknown;
            }
        }

        private ExecType GetExecType(string value)
        {
            char c = value[0];

            if (char.IsDigit(c))
            {
                return (ExecType) (c - '0');
            }
            else
            {
                return (ExecType) (11 + c - 'A');
            }
        }

        private OrderStatus GetOrderStatus(string value)
        {
            switch (value.ToUpper())
            {
                case "0":
                    return OrderStatus.New;
                case "1":
                    return OrderStatus.Partial;
                case "2":
                    return OrderStatus.Filled;
                case "3":
                    return OrderStatus.Done;
                case "4":
                    return OrderStatus.Canceled;
                case "7":
                    return OrderStatus.Stopped;
                case "8":
                    return OrderStatus.Rejected;
                case "9":
                    return OrderStatus.Suspended;
                case "A":
                    return OrderStatus.PendingNew;
                case "B":
                    return OrderStatus.Calculated;
                case "C":
                    return OrderStatus.Expired;
                case "D":
                    return OrderStatus.AcceptBidding;
            }
            return OrderStatus.Unknown;
        }
    }

    public class FixInterpreterA : FixInterpreter
    {
        public override string GetInterpretation()
        {
            return "Logon";
        }

        protected override void Read(FixMessage msg)
        {
            //
        }
    }

    public class FixInterpreterAN : FixInterpreter
    {
        private string reqId = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("RequestForPosition (PosReqID={0})", this.reqId);
        }

        protected override void Read(FixMessage msg)
        {
            FixTag reqIdTag = msg.Tags.FirstOrDefault(x => x.Tag == 710);
            this.reqId = reqIdTag == null ? string.Empty : reqIdTag.Value;
            if (this.reqId.Length > 6)
            {
                this.reqId = this.reqId.Substring(0, 6) + "...";
            }
        }
    }

    public class FixInterpreterAO : FixInterpreter
    {
        private string reports = string.Empty;
        private string reqId = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("RequestForPosition-Ack (PosReqID={0} | TotalNumPosReports={1})", this.reqId,
                this.reports);
        }

        protected override void Read(FixMessage msg)
        {
            FixTag reqIdTag = msg.Tags.FirstOrDefault(x => x.Tag == 710);
            this.reqId = reqIdTag == null ? string.Empty : reqIdTag.Value;
            if (this.reqId.Length > 6)
            {
                this.reqId = this.reqId.Substring(0, 6) + "...";
            }

            FixTag numRepTag = msg.Tags.FirstOrDefault(x => x.Tag == 727);
            this.reports = numRepTag == null ? "0" : numRepTag.Value;
        }
    }

    public class FixInterpreterAP : FixInterpreter
    {
        private string isin = string.Empty;
        private string reqId = string.Empty;
        private string srdInd = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("RequestForPosition-Report (PosReqID={0} | Isin={1} | SRD={2})", this.reqId, this.isin,
                this.srdInd);
        }

        protected override void Read(FixMessage msg)
        {
            FixTag reqIdTag = msg.Tags.FirstOrDefault(x => x.Tag == 710);
            this.reqId = reqIdTag == null ? string.Empty : reqIdTag.Value;
            if (this.reqId.Length > 6)
            {
                this.reqId = this.reqId.Substring(0, 6) + "...";
            }

            FixTag isinTag = msg.Tags.FirstOrDefault(x => x.Tag == 48);
            this.isin = isinTag == null ? "?" : isinTag.Value;

            FixTag srdTag = msg.Tags.FirstOrDefault(x => x.Tag == 7000);
            this.srdInd = srdTag == null ? "?" : (srdTag.Value == "SRD" ? "Yes" : "No");
        }
    }

    public class FixInterpreterBB : FixInterpreter
    {
        private string inqId = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("CollateralInquiry (CollInquiryID={0})", this.inqId);
        }

        protected override void Read(FixMessage msg)
        {
            this.inqId = msg.TagValue(909);
            if (this.inqId.Length > 8)
            {
                this.inqId = this.inqId.Substring(0, 8) + "... ";
            }
        }
    }

    public class FixInterpreterBG : FixInterpreter
    {
        private string inqId = string.Empty;
        private string total = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("Collateral-Ack (CollInquiryID={0} | TotNumReports={1})", this.inqId, this.total);
        }

        protected override void Read(FixMessage msg)
        {
            this.inqId = msg.TagValue(909);
            if (this.inqId.Length > 8)
            {
                this.inqId = this.inqId.Substring(0, 8) + "... ";
            }

            this.total = msg.TagValue(911);
        }
    }

    public class FixInterpreterBA : FixInterpreter
    {
        private string cash = string.Empty;
        private string inqId = string.Empty;
        private string srd = string.Empty;

        public override string GetInterpretation()
        {
            return string.Format("Collateral-Report (CollInquiryID={0} | StartCash={1} | SRD={2})", this.inqId,
                this.cash, this.srd);
        }

        protected override void Read(FixMessage msg)
        {
            this.inqId = msg.TagValue(909);
            if (this.inqId.Length > 8)
            {
                this.inqId = this.inqId.Substring(0, 8) + "... ";
            }

            this.cash = msg.TagValue(921);

            this.srd = msg.TagValue(7000) == "SRD" ? "Yes" : "No";
        }
    }

    public class FixInterpreter5 : FixInterpreter
    {
        public override string GetInterpretation()
        {
            return "Logout";
        }

        protected override void Read(FixMessage msg)
        {
            //
        }
    }

    public enum SessionRejectReason
    {
        InvalidTagNumber,
        RequiredTagMissing,
        TagNotDefinedForThisMessageType,
        UndefinedTag,
        TagSpecifiedWithoutAValue,
        ValueIsIncorrectOutOfRangeForThisTag,
        IncorrectDataFormatForValue,
        DecryptionProblem,
        SignatureProblem,
        CompIDProblem,
        SendingTimeAccuracyProblem,
        InvalidMsgType,
        XMLValidationError,
        TagAppearsMoreThanOnce,
        TagSpecifiedOutOfRequiredOrder,
        RepeatingGroupFieldsOutOfOrder,
        IncorrectNumInGroupCountForRepeatingGroup,
        NonDataValueIncludesFieldDelimiterSOHCharacter,
        Others
    }

    public class FixInterpreter3 : FixInterpreter
    {
        /*
         * 8=FIX.4.4|35=3|34=341|49=QUODDERSTA|52=20100125-13:02:28|
         * 56=ALEXDERSTA|372=AB|45=337|373=15|58=Repeating group tag 608 is out of order|
         * */

        private SessionRejectReason reason;
        private string refType;
        private string text;

        public override string GetInterpretation()
        {
            return string.Format("Rejected {0} ({1}) - {2}",
                this.refType,
                this.reason.ToString(),
                this.text
                );
        }

        protected override void Read(FixMessage msg)
        {
            this.text = msg[58] == null ? "" : msg[58].Value;
            this.refType = msg[372] == null ? "" : msg[372].Value;
            try
            {
                this.reason = (SessionRejectReason) (Byte.Parse((msg[373].Value)));
            }
            catch (Exception)
            {
                this.reason = SessionRejectReason.Others;
            }
        }
    }

    public class LegInterpreter
    {
        private readonly InstrumentInterpreter instrument;

        public LegInterpreter(FixMessage msg, FixTag[] legTags)
        {
            this.instrument = InstrumentInterpreterOPT.ToLegInstrument(msg, legTags);
        }

        public string GetInterpretation()
        {
            return this.instrument.GetInterpretation();
        }
    }

    public enum InstrumentType
    {
        Option,
        CommonStock,
        Unknown
    };

    public abstract class InstrumentInterpreter
    {
        public abstract string GetInterpretation();

        public static InstrumentInterpreter FromMessage(FixMessage msg)
        {
            InstrumentInterpreter intr = null;

            if ((msg[167] != null) && (msg[167].Value != null))
            {
                Type t = Type.GetType("FixAnalyzer.InstrumentInterpreter" + msg[167].Value);
                if (t != null)
                    intr = (InstrumentInterpreter) Activator.CreateInstance(t);
            }

            if (intr == null)
                intr = new InstrumentInterpreterUnknown();

            intr.ReadMessage(msg);
            return intr;
        }

        public abstract InstrumentType GetInstrumentType();

        protected abstract void ReadMessage(FixMessage msg);
    }

    public enum OrderType
    {
        Market,
        Limit
    };

    public class InstrumentInterpreterOPT : InstrumentInterpreter
    {
        private CallPut callPut;
        private string currency;
        private DateTime maturity;
        private OrderTypeInterpreter orderType;
        private int qty;
        private double strikePx;
        private string symbol;

        private int tCallPut = 461;
        private int tCurrency = 15;
        private int tMaturity = 200;
        private int tQty = 38;
        private int tStrikePx = 202;
        private int tSymbol = 55;

        private void Read(IEnumerable<FixTag> tags)
        {
            foreach (FixTag tag in tags)
            {
                if (tag.Tag == this.tSymbol)
                    this.symbol = tag.Value;
                else if (tag.Tag == this.tMaturity)
                    this.maturity = new DateTime(
                        Convert.ToInt32(tag.Value.Substring(0, 4)),
                        Convert.ToInt32(tag.Value.Substring(4, 2)),
                        1);
                else if (tag.Tag == this.tStrikePx)
                    this.strikePx = Convert.ToDouble(tag.Value);
                else if (tag.Tag == this.tCallPut)
                    this.callPut = tag.Value.Contains("OC") ? CallPut.Call : CallPut.Put;
                else if (tag.Tag == this.tCurrency)
                    this.currency = tag.Value;
                else if (tag.Tag == this.tQty)
                    this.qty = Convert.ToInt32(tag.Value);
            }
        }

        private void InitLegReading()
        {
            this.tSymbol = 600;
            this.tMaturity = 610;
            this.tStrikePx = 612;
            this.tCallPut = 608;
            this.tQty = 623;
        }

        public static InstrumentInterpreter ToLegInstrument(FixMessage msg, FixTag[] leg)
        {
            InstrumentInterpreterOPT opt = new InstrumentInterpreterOPT();
            opt.InitLegReading();
            opt.currency = msg[opt.tCurrency].Value;
            opt.Read(leg);
            opt.orderType = OrderTypeInterpreter.FromMessage(msg);
            return opt;
        }

        public override string GetInterpretation()
        {
            return string.Format("{0} {1} {2} {3} {4} {5:0.#####} - {6} {7}",
                this.callPut.ToString(),
                GetInstrumentType().ToString(),
                this.symbol,
                this.maturity.ToString("MMM yyyy").ToUpper(),
                this.strikePx,
                this.currency,
                this.qty,
                (this.orderType != null) ? this.orderType.GetInterpretation() : ""
                );
        }

        public override InstrumentType GetInstrumentType()
        {
            return InstrumentType.Option;
        }

        protected override void ReadMessage(FixMessage msg)
        {
            this.orderType = OrderTypeInterpreter.FromMessage(msg);
            Read(msg.Tags);
        }
    }

    public enum SecuritySourceId
    {
        Unknown0,
        CUSIP,
        SEDOL,
        QUIK,
        ISIN,
        RIC,
        ISOCurr,
        ISOCountry,
        ExchSymb,
        CTA,
        Blmbrg,
        Wertpapier,
        Dutch,
        Valoren,
        Sicovam,
        Belgian,
        Common,
        ClearingHouse,
        FpML,
        OptionPriceReportingAuthority
    }

    public class InstrumentInterpreterCS : InstrumentInterpreter
    {
        /*&
         * 8=FIX.4.4|35=D|34=0|49=ALEXDERSTA|56=QUODDERSTA|11=406379331|1=PAC677|21=2|55=WHV|48=NL0000289213|22=4|
         * 461=EXXXXX|167=CS|54=1|60=20100218-09:02:53|38=11|15=EUR|40=1|59=0|528=A|
         */

        private string code;
        private SecuritySourceId codeType;
        private OrderTypeInterpreter orderType;
        private int qty;
        private string symbol;

        public override string GetInterpretation()
        {
            return string.Format("Stock {0} ({1}={2}) - {3} {4}",
                this.symbol,
                this.codeType,
                this.code,
                this.qty,
                this.orderType.GetInterpretation()
                );
        }

        public override InstrumentType GetInstrumentType()
        {
            return InstrumentType.CommonStock;
        }

        protected override void ReadMessage(FixMessage msg)
        {
            this.symbol = msg[55].Value;
            this.code = msg[48].Value;
            if (msg[22] != null)
                this.codeType = GetSecuritySourceId(msg[22].Value[0]);
            else
                this.codeType = SecuritySourceId.Unknown0;

            this.orderType = OrderTypeInterpreter.FromMessage(msg);
            this.qty = Int32.Parse(msg[38].Value);
        }

        private SecuritySourceId GetSecuritySourceId(char value)
        {
            if (char.IsDigit(value))
            {
                return (SecuritySourceId) (value - '0');
            }
            try
            {
                return (SecuritySourceId) (10 + value - 'A');
            }
            catch (Exception)
            {
                return SecuritySourceId.Unknown0;
            }
        }
    }

    public class InstrumentInterpreterUnknown : InstrumentInterpreter
    {
        private int idSource;
        private string security_id;
        private string symbol = null;

        public override string GetInterpretation()
        {
            string inst = "";

            inst += this.symbol == "" ? "" : "(Symbol=" + this.symbol + ")";

            switch (this.idSource)
            {
                case 4:
                    inst += string.Format(" ISIN {0}", this.security_id);
                    break;

                case 5:
                    inst += string.Format(" RIC {0}", this.security_id);
                    break;
            }

            return inst == "" ? "<unknown instrument>" : inst;
        }

        public override InstrumentType GetInstrumentType()
        {
            return InstrumentType.Unknown;
        }

        protected override void ReadMessage(FixMessage msg)
        {
            this.symbol = msg.TagValue(55).Trim(); // get symbol
            int.TryParse(msg.TagValue(22), out this.idSource);
            this.security_id = msg.TagValue(48);
        }
    }

    public abstract class OrderTypeInterpreter
    {
        public static OrderTypeInterpreter FromMessage(FixMessage msg)
        {
            OrderTypeInterpreter intr = null;

            if ((msg[40] != null) && (msg[40].Value != null))
            {
                Type t = Type.GetType("FixAnalyzer.OrderTypeInterpreter" + msg[40].Value);
                if (t != null)
                {
                    intr = (OrderTypeInterpreter) Activator.CreateInstance(t);
                }
            }

            if (intr == null)
                intr = new OrderTypeInterpreterUnknown();
            intr.Read(msg);
            return intr;
        }

        public abstract string GetInterpretation();
        protected abstract void Read(FixMessage msg);
    }

    public class OrderTypeInterpreter1 : OrderTypeInterpreter
    {
        public override string GetInterpretation()
        {
            return "MKT";
        }

        protected override void Read(FixMessage msg)
        {
            //
        }
    }

    public class OrderTypeInterpreter2 : OrderTypeInterpreter
    {
        private double price;

        public override string GetInterpretation()
        {
            return string.Format("LIMIT {0}", this.price);
        }

        protected override void Read(FixMessage msg)
        {
            if (!double.TryParse(msg[44].Value, out this.price))
                this.price = 0;
        }
    }

    public class OrderTypeInterpreterUnknown : OrderTypeInterpreter
    {
        private string orderType;

        public override string GetInterpretation()
        {
            return this.orderType;
        }

        protected override void Read(FixMessage msg)
        {
            if ((msg[40] == null) || (msg[40].Value == null))
            {
                this.orderType = "<undefined ordertype>";
            }
            else
            {
                this.orderType = "OrderType: " + msg[40].Value;
            }
        }
    }
}