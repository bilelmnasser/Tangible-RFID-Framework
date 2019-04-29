using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ColocartsTangibleFrameWork
{/// <summary> 
    /// This class used for Serialization/Deserialization of any list object into a file on hard disk.
    /// </summary>
    /// <author>Bilel Mnasser</author>
   public static class Serialization
   {     /// <summary> 
       ///static function to Serialize a list into a file on hard disk.
       ///<param name="List">list to Deserialize.</param>
       ///<param name="filename">File path.</param>
       ///<return> this function return type is bool. </return>
       ///<exception>cref="system.io.filenotfoundexception" .</exception>
       ///</summary>
        public static bool SerializeObject(ref List<String> List, String filename)
        {
            try
            {
                //objet de serialisation

                XmlSerializer xmlSerializer = new XmlSerializer(List.GetType());

                //ouverture de flux d'ecriture dans un fichier

                TextWriter textWriter = new StreamWriter(filename);

                //serialisation d'object { paramètres : 1.flux de fichier 2.object List}

                xmlSerializer.Serialize(textWriter, List);

                //fermeture de flux

                textWriter.Close();
                //serialization not failed
                return true;
            }
            catch (Exception e) 
            {
                //write the exception in terminal
                Console.WriteLine(e.StackTrace); 
                //serialization failed return false
                return false; 
            }
         
        }


        /// <summary> 
        ///static function to Deserialize a list from a file on hard disk.
        ///<param name="List">list to Deserialize.</param>
        ///<param name="filename">File path.</param>
        ///<return> this function return type is bool. </return>
        ///<exception>cref="system.io.filenotfoundexception". </exception>
        ///</summary>
        public static bool DeserializeObject(ref List<String> List, String filename)
        {try{
            //objet de serialisation
            XmlSerializer xmlSerializer = new XmlSerializer(List.GetType());
            //ouverture de flux de lecture d'un fichier
            TextReader textReader = new StreamReader(filename);
            //desrialisation de l'objet{ a partir d'un flux de fichiser et une conversion de type est nécessaire}
            List = (List<String>)xmlSerializer.Deserialize(textReader);
            //fermeture de flux
            textReader.Close();
               //serialization not failed
                return true;
            }
            catch (Exception e) 
            {
                //write the exception in terminal
                Console.WriteLine(e.StackTrace); 
                //serialization failed return false
                return false; 
            }

        }
    }
}
