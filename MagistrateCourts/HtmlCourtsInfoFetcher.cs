﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoCompany.Interfaces;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using NoCompany.Data.Properties;
using System.Threading;
using log4net;
using System.Reflection;
using NoCompany.Data.Parsers;
using CodeContracts;

namespace NoCompany.Data
{
    public class HtmlCourtsInfoFetcher : CancelableBase, IDataProvider
    {
        public static ILog logger = LogManager.GetLogger(typeof(HtmlCourtsInfoFetcher));

        public IDataParserHandler Parser { get; private set; }
        private Action CacelOperation { get; set; }

        public event EventHandler ImStillAlive;
       
        public HtmlCourtsInfoFetcher(IDataParserHandler parser)
        {
            Requires.NotNull(parser, "parser");
            Parser = parser;
            Parser.ImStillAlive += Parser_ImStillAlive;
            CacelOperation = () => Parser.Cancel();
        }

        private void Parser_ImStillAlive(object sender, EventArgs e) 
        {
            ImStillAlive(sender, e);
        }

        public IEnumerable<IChangeableData> GetData()
        {
            logger.Debug(MethodBase.GetCurrentMethod().Name);

            const string sudRF = "https://sudrf.ru";

            return Parser.Parce(sudRF);
        }

        public override void Cancel()
        {
            base.Cancel();
            CacelOperation();
        }
    }
}
