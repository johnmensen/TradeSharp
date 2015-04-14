using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace TradeSharp.TradeSignal.BL
{
    public class XmlSchemaValidator
    {
        private bool isValidXml = true;
        private string validationError = "";

        /// <SUMMARY>
        /// Public get/set access to the validation error.
        /// </SUMMARY>
        public string ValidationError
        {
            get
            {
                return "<VALIDATIONERROR>" + validationError
                       + "</VALIDATIONERROR>";
            }
            set { validationError = value; }
        }

        /// <SUMMARY>
        /// Public get access to the isValidXml attribute.
        /// </SUMMARY>
        public bool IsValidXml
        {
            get { return isValidXml; }
        }

        /// <SUMMARY>
        /// This method is invoked when the XML does not match
        /// the XML Schema.
        /// </SUMMARY>
        /// <PARAM name="sender"></PARAM>
        /// <PARAM name="args"></PARAM>
        private void ValidationCallBack(object sender,
                                        ValidationEventArgs args)
        {
            // The xml does not match the schema.
            isValidXml = false;
            ValidationError = args.Message;
        }


        /// <SUMMARY>
        /// This method validates an xml string against an xml schema.
        /// </SUMMARY>
        /// <PARAM name="xml">XML string</PARAM>
        /// <PARAM name="schemaNamespace">XML Schema Namespace</PARAM>
        /// <PARAM name="schemaUri">XML Schema Uri</PARAM>
        /// <RETURNS>bool</RETURNS>
        public bool ValidXmlDoc(string xml,
                                string schemaNamespace, string schemaUri)
        {
            try
            {
                // Is the xml string valid?
                if (string.IsNullOrEmpty(xml))
                {
                    return false;
                }

                var srXml = new StringReader(xml);
                return ValidXmlDoc(srXml, schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// This method validates an xml document against an xml  
        /// </summary>
        public bool ValidXmlDoc(XmlDocument xml, string schemaNamespace, string schemaUri)
        {
            try
            {
                // Is the xml object valid?
                if (xml == null) return false;                

                // Create a new string writer.
                var sw = new StringWriter();
                // Set the string writer as the text writer 
                // to write to.
                var xw = new XmlTextWriter(sw);
                // Write to the text writer.
                xml.WriteTo(xw);
                // Get 
                var strXml = sw.ToString();

                var srXml = new StringReader(strXml);

                return ValidXmlDoc(srXml, schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
                return false;
            }
        }

        /// <SUMMARY>
        /// This method validates an xml string against an xml schema.
        /// </SUMMARY>
        /// <PARAM name="xml">StringReader containing xml</PARAM>
        /// <PARAM name="schemaNamespace">XML Schema Namespace</PARAM>
        /// <PARAM name="schemaUri">XML Schema Uri</PARAM>
        /// <RETURNS>bool</RETURNS>
        public bool ValidXmlDoc(StringReader xml,
                                string schemaNamespace, string schemaUri)
        {
            // Continue?
            if (xml == null || schemaNamespace == null || schemaUri == null)
            {
                return false;
            }

            isValidXml = true;
            XmlValidatingReader vr;
            XmlTextReader tr;
            var schemaCol = new XmlSchemaCollection();
            schemaCol.Add(schemaNamespace, schemaUri);

            try
            {
                // Read the xml.
                tr = new XmlTextReader(xml);
                // Create the validator.
                vr = new XmlValidatingReader(tr);
                // Set the validation tyep.
                vr.ValidationType = ValidationType.Auto;
                // Add the schema.
                if (schemaCol != null)
                {
                    vr.Schemas.Add(schemaCol);
                }
                // Set the validation event handler.
                vr.ValidationEventHandler +=
                    ValidationCallBack;
                // Read the xml schema.
                while (vr.Read())
                {
                }

                vr.Close();

                return isValidXml;
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
                return false;
            }
            finally
            {
                // Clean up...
                vr = null;
                tr = null;
            }
        }
    }
}