using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                                // Verificar si se proporcionó el nombre del archivo como argumento
                if (args.Length == 0)
                {
                    Console.WriteLine("Por favor, ingrese el nombre del archivo XML como argumento.");
                    return;
                }

                 // Ruta del archivo XML de entrada (proporcionado por consola)
                string inputXmlPath = args[0];

                // Verificar si el archivo existe
                if (!File.Exists(inputXmlPath))
                {
                    Console.WriteLine("El archivo especificado no existe: " + inputXmlPath);
                    return;
                }

                // Verificar si el archivo tiene la extensión .xml
                if (Path.GetExtension(inputXmlPath).ToLower() != ".xml")
                {
                    Console.WriteLine("El archivo especificado no tiene la extensión .xml: " + inputXmlPath);
                    return;
                }

                // Ruta del archivo XML de entrada (XMLSUNAT)
                // string inputXmlPath = "input.xml"; // Cambiar por la ruta correcta
                
                // Ruta del archivo XML de salida (XMLEJEMPLO con valores reemplazados)
                string outputXmlPath = "output.xml"; // Cambiar por la ruta deseada

                // Cargar el XML de entrada
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(inputXmlPath);

                // Crear el namespace manager para manejar los namespaces del XML
                XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                nsManager.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

                // Extraer los valores necesarios según las reglas
                string v_tipoCDP = ExtractTipoCDP(xmlDoc, nsManager);
                string v_serieCDP = ExtractSerieCDP(xmlDoc, nsManager);
                string v_numeroCDP = ExtractNumeroCDP(xmlDoc, nsManager);
                string v_tipoDocldReceptor = ExtractTipoDocldReceptor(xmlDoc, nsManager);
                string v_numeroDocldReceptor = ExtractNumeroDocldReceptor(xmlDoc, nsManager);
                string v_fechaEmision = ExtractFechaEmision(xmlDoc, nsManager);
                string v_importeTotal = ExtractImporteTotal(xmlDoc, nsManager);
                string v_nroAutorizacion = ExtractNroAutorizacion(xmlDoc, nsManager);
                string rucEmisor = ExtractRucEmisor(xmlDoc, nsManager);

                // Generar el XML de salida con los valores extraídos
                GenerateOutputXml(outputXmlPath, rucEmisor, v_tipoCDP, v_serieCDP, v_numeroCDP, 
                                v_tipoDocldReceptor, v_numeroDocldReceptor, v_fechaEmision, 
                                v_importeTotal, v_nroAutorizacion);

                Console.WriteLine("Proceso completado exitosamente. Archivo generado en: " + outputXmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // Métodos para extraer cada valor del XML según las reglas especificadas

        static string ExtractTipoCDP(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<cbc:InvoiceTypeCode " y "<cbc:InvoiceTypeCode>"
            XmlNode node = xmlDoc.SelectSingleNode("//cbc:InvoiceTypeCode", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        static string ExtractSerieCDP(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<cbc:ID>" y "</cbc:ID>" y tomar los 4 primeros caracteres antes del "-"
            XmlNode node = xmlDoc.SelectSingleNode("//cbc:ID", nsManager);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                string[] parts = node.InnerText.Split('-');
                if (parts.Length > 0 && parts[0].Length >= 4)
                {
                    return parts[0].Substring(0, 4);
                }
            }
            return string.Empty;
        }

        static string ExtractNumeroCDP(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<cbc:ID>" y "</cbc:ID>" y tomar todo después del "-"
            XmlNode node = xmlDoc.SelectSingleNode("//cbc:ID", nsManager);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                string[] parts = node.InnerText.Split('-');
                if (parts.Length > 1)
                {
                    return parts[1];
                }
            }
            return string.Empty;
        }

        static string ExtractTipoDocldReceptor(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<cbc:PartyIdentification " y "<cbc:PartyIdentification>"
            // En el ejemplo, parece que el tipo está en el atributo schemeID
            // XmlNode node = xmlDoc.SelectSingleNode("//cac:AccountingCustomerParty//cac:PartyIdentification/cbc:ID", nsManager);
            // if (node != null && node.Attributes != null && node.Attributes["schemeID"] != null)
            // {
            //    return node.Attributes["schemeID"].Value;
            // }
            // return string.Empty;


            // < cbc:InvoiceTypeCode > atributo listID Valor = "1001"

            XmlNode node = xmlDoc.SelectSingleNode("//cbc:InvoiceTypeCode", nsManager);
            if (node != null && node.Attributes != null && node.Attributes["listID"] != null)
            {
                return node.Attributes["listID"].Value;
            }
            return string.Empty;

        }

        static string ExtractNumeroDocldReceptor(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<cac:PartyIdentification>" y "</cac:PartyIdentification>" dentro de "<cbc:ID>"
            XmlNode node = xmlDoc.SelectSingleNode("//cac:AccountingCustomerParty//cac:PartyIdentification/cbc:ID", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        static string ExtractFechaEmision(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar en "<cbc:IssueDate>" y "</cbc:IssueDate>"
            XmlNode node = xmlDoc.SelectSingleNode("//cbc:IssueDate", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        static string ExtractImporteTotal(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar en "<cbc:PayableAmount>" dentro de "<cac:LegalMonetaryTotal>"
            XmlNode node = xmlDoc.SelectSingleNode("//cac:LegalMonetaryTotal/cbc:PayableAmount", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        static string ExtractNroAutorizacion(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar entre "<ds:SignatureValue>" y "</ds:SignatureValue>"
            XmlNode node = xmlDoc.SelectSingleNode("//ds:SignatureValue", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        static string ExtractRucEmisor(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
        {
            // Buscar el RUC del emisor en AccountingSupplierParty
            XmlNode node = xmlDoc.SelectSingleNode("//cac:AccountingSupplierParty//cac:PartyIdentification/cbc:ID", nsManager);
            return node?.InnerText ?? string.Empty;
        }

        // Método para generar el XML de salida con los valores extraídos
        static void GenerateOutputXml(string outputPath, string rucEmisor, string tipoCDP, string serieCDP, 
                                    string numeroCDP, string tipoDocldReceptor, string numeroDocldReceptor, 
                                    string fechaEmision, string importeTotal, string nroAutorizacion)
        {
            StringBuilder xmlBuilder = new StringBuilder();
            
            xmlBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xmlBuilder.AppendLine("<root>");
            xmlBuilder.AppendLine($"  <rucEmisor>{rucEmisor}</rucEmisor>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <tipoCDP>{tipoCDP}</tipoCDP>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <serieCDP>{serieCDP}</serieCDP>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <numeroCDP>{numeroCDP}</numeroCDP>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <tipoDocldReceptor>{tipoDocldReceptor}</tipoDocldReceptor>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <numeroDocldReceptor>{numeroDocldReceptor}</numeroDocldReceptor>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <fechaEmision>{fechaEmision}</fechaEmision>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <importeTotal>{importeTotal}</importeTotal>");
            xmlBuilder.AppendLine("  <!--Optional:-->");
            xmlBuilder.AppendLine($"  <nroAutorizacion>{nroAutorizacion}</nroAutorizacion>");
            xmlBuilder.AppendLine("</root>");

            File.WriteAllText(outputPath, xmlBuilder.ToString());
        }
    }
}
