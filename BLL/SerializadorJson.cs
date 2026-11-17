using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace BLL
{
    
    // (DataContractJsonSerializer), sin librerias externas.
    //
    // Que propiedades se escriben, y en que orden, lo indican los atributos [DataContract] y
    // [DataMember(Order = n)] de cada clase de BE.
    
    public static class SerializadorJson
    {
        private const string FormatoFecha = "yyyy-MM-dd'T'HH:mm:ss";

        private static DataContractJsonSerializer CrearSerializador(Type tipo)
        {
            return new DataContractJsonSerializer(tipo, new DataContractJsonSerializerSettings
            {
                DateTimeFormat = new DateTimeFormat(FormatoFecha)
            });
        }

        // Objeto -> texto JSON (indentado, para que se pueda leer).
        public static string Serializar<T>(T objeto)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter escritor = JsonReaderWriterFactory.CreateJsonWriter(
                           ms, Encoding.UTF8, false, true, "  "))
                {
                    CrearSerializador(typeof(T)).WriteObject(escritor, objeto);
                    escritor.Flush();
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        // Serializa el objeto y lo guarda en un archivo .json (UTF-8).
        public static void GuardarArchivo<T>(T objeto, string ruta)
        {
            File.WriteAllText(ruta, Serializar(objeto), new UTF8Encoding(false));
        }
    }
}
