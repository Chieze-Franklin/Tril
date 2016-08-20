using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;

namespace Tril.App
{
    /// <summary>
    /// Helper functions to interact with the file system
    /// </summary>
    public static class FileSystemServices
    {
        /// <summary>
        /// Appends bytes to a file in a safe way
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void AppendBytesToFile(string filePath, byte[] bytes) //confirm
        {
            FileStream fs = null;

            try
            {
                CreateFile(filePath, false, "");

                #region METHOD1
                fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None);

                fs.Write(bytes, 0, bytes.Length);
                #endregion
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        /// <summary>
        /// Appends text to a file in a safe way
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileText"></param>
        public static void AppendTextToFile(string filePath, string fileText)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(fileText);
            AppendBytesToFile(filePath, bytes);
        }
        /// <summary>
        /// Creates a directory
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="canOverride"></param>
        public static void CreateDirectory(string directoryPath, bool canOverride)
        {
            //try
            //{
            if (Directory.Exists(directoryPath) && canOverride)
            {
                Directory.Delete(directoryPath, true);
            }
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            //}
            //catch { throw; }
            //finally { }
        }
        /// <summary>
        /// Creates a file in a safe way
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="canOverride"></param>
        /// <param name="defaultText"></param>
        public static void CreateFile(string filePath, bool canOverride, string defaultText)
        {
            //try
            //{
            if (File.Exists(filePath) && canOverride)
            {
                File.Delete(filePath);
            }
            if (!File.Exists(filePath))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                    //Convert defaultText to a byte array, 
                    //and make the byteText reference to point to that byte array
                    byte[] byteText = System.Text.Encoding.ASCII.GetBytes(defaultText);

                    fs.Write(byteText, 0, byteText.Length);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }
            //}
            //catch { throw; }
            //finally { }
        }
        /// <summary>
        /// deserializes an object from binary format
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static object DeserializeFromBinary(string filePath)
        {
            IFormatter serializer = new BinaryFormatter();
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return serializer.Deserialize(stream);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// deserializes an object from soap format
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static object DeserializeFromSoap(string filePath)
        {
            IFormatter serializer = new SoapFormatter();
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return serializer.Deserialize(stream);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// deserializes an object from xml format
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="type"></param>
        /// <param name="subTypes"></param>
        /// <returns></returns>
        public static object DeserializeFromXml(string filePath, Type type, Type[] subTypes)
        {
            XmlSerializer serializer;
            if (subTypes != null)
                serializer = new XmlSerializer(type, subTypes);
            else
                serializer = new XmlSerializer(type);
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return serializer.Deserialize(stream);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// Returns the absolute path of the specified (relative) path with respect to the base directory
        /// </summary>
        /// <param name="mayBeRelativePath"></param>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string mayBeRelativePath, string baseDirectory = null)
        {
            if (baseDirectory == null)
                baseDirectory = Environment.CurrentDirectory;
            else if (File.Exists(baseDirectory)) //if the base dir is actually a file and not a dir
                baseDirectory = new FileInfo(baseDirectory).DirectoryName;

            var root = Path.GetPathRoot(mayBeRelativePath);
            if (string.IsNullOrEmpty(root))
                return Path.GetFullPath(Path.Combine(baseDirectory, mayBeRelativePath));
            if (root == "\\")
                return Path.GetFullPath(Path.Combine(Path.GetPathRoot(baseDirectory), mayBeRelativePath.Remove(0, 1)));

            return mayBeRelativePath;
        }
        /// <summary>
        /// Reads bytes from a file
        /// </summary>
        /// <param name="filePath">The file to be read</param>
        /// <returns>The bytes returned from the file</returns>
        public static byte[] ReadBytesFromFile(string filePath)
        {
            FileStream fs = null;

            try
            {
                CreateFile(filePath, false, "");

                //open the file
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

                //create an array of byte of the size of file, and read file content into it
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                return bytes;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        /// <summary>
        /// Reads text from a file
        /// </summary>
        /// <param name="filePath">The file to be read</param>
        /// <returns>The text returned from the file</returns>
        public static string ReadTextFromFile(string filePath)
        {
            byte[] bytes = ReadBytesFromFile(filePath);
            //convert byte array to a string
            string fileText = System.Text.Encoding.ASCII.GetString(bytes);
            return fileText;
        }
        /// <summary>
        /// serializes an object to binary format
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        public static void SerializeToBinary(string filePath, object obj)
        {
            IFormatter serializer = new BinaryFormatter();
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                serializer.Serialize(stream, obj);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// serializes an object to soap format
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        public static void SerializeToSoap(string filePath, object obj)
        {
            IFormatter serializer = new SoapFormatter();
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                serializer.Serialize(stream, obj);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// serializes an object to XML format
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="subTypes"></param>
        public static void SerializeToXml(string filePath, object obj, Type type, Type[] subTypes)
        {
            XmlSerializer serializer;
            if (subTypes != null)
                serializer = new XmlSerializer(type, subTypes);
            else
                serializer = new XmlSerializer(type);
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                serializer.Serialize(stream, obj);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        /// <summary>
        /// Writes bytes to a file in a safe way
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteBytesToFile(string filePath, byte[] bytes)
        {
            FileStream fs = null;

            try
            {
                CreateFile(filePath, false, "");

                #region METHOD1
                fs = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None);

                fs.Write(bytes, 0, bytes.Length);
                #endregion
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        /// <summary>
        /// Writes text to a file in a safe way
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileText"></param>
        public static void WriteTextToFile(string filePath, string fileText)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(fileText);
            WriteBytesToFile(filePath, bytes);
        }
    }
}
