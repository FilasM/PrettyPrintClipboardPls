using MediatR;
using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Collections.Generic;

namespace PrettyPrintClipboardPls
{
    public class PrettyPrintHandler : IRequestHandler<PrettyPrintCommand, string>
	{
		public string Handle(PrettyPrintCommand message)
        {
            var sqlParser = new TSql150Parser(false, SqlEngineType.All);
            IList<ParseError> parseErrros = new List<ParseError>();
            TSqlFragment res = null;

            using (TextReader sr = new StringReader(message.Text))
            {
                res = sqlParser.Parse(sr, out parseErrros);
            }

            if(parseErrros.Count == 0)
            {
                return SQLFormatter.Format(res);
            }
            else
            {
                return this.prettyPrintXml(message.Text);
            }
		}
        
        private string prettyPrintXml(string source, XmlWriterSettings customSettings = null)
        {
            var stringBuilder = new StringBuilder();

            XElement element;
            try
            {
                element = XElement.Parse(source);
            }
            catch(Exception e)
            {
                Debug.Write(e.Message);
                return null;
            }

            var settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true
            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, customSettings ?? settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }
    }
}