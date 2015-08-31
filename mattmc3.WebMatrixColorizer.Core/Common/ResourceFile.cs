using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace mattmc3.Common {

	/// <summary>
	/// Handles embedded resources, similar to the way System.IO.File works.
	/// </summary>
	/// <remarks>
	/// Handles embedded resources.
	/// Be careful of Assembly.GetCallingAssembly()... you can't farm out that call to another
	/// method, or all the calling Assemblies will be this library instead of the caller.
	/// </remarks>
	public class ResourceFile {

		public static bool Exists(string resourceName) {
			return Exists(resourceName, Assembly.GetCallingAssembly());
		}

		public static bool Exists(string resourceName, Assembly asm) {
			var resources = GetResourceNames(asm);
			return resources.Contains(resourceName);
		}

		public static Stream Open(string resourceName) {
			return Open(resourceName, Assembly.GetCallingAssembly());
		}

		public static Stream Open(string resourceName, Assembly asm) {
			Stream dataStream = asm.GetManifestResourceStream(resourceName);
			if (dataStream == null) {
				string listOfResources = string.Join(",", GetResourceNames(asm));
				string errMsg = string.Format("The identifier specified is not a valid embedded resource ({0}).  The resources available are: {1}", resourceName, listOfResources);
				throw new ArgumentException(errMsg, resourceName);
			}
			return dataStream;
		}

		public static string ReadAllText(string resourceName) {
			return ReadAllText(resourceName, Assembly.GetCallingAssembly());
		}

		public static string ReadAllText(string resourceName, System.Text.Encoding encoding) {
			return ReadAllText(resourceName, Assembly.GetCallingAssembly(), encoding);
		}

		public static string ReadAllText(string resourceName, Assembly asm) {
			string result = null;
			using (Stream strm = Open(resourceName, asm)) {
				using (StreamReader rdr = new StreamReader(strm)) {
					result = rdr.ReadToEnd();
				}
			}
			return result;
		}

		public static string ReadAllText(string resourceName, Assembly asm, System.Text.Encoding encoding) {
			string result = null;
			using (Stream strm = Open(resourceName, asm)) {
				using (StreamReader rdr = new StreamReader(strm, encoding)) {
					result = rdr.ReadToEnd();
				}
			}
			return result;
		}

		public static byte[] ReadAllBytes(string resourceName) {
			return ReadAllBytes(resourceName, Assembly.GetCallingAssembly());
		}

		public static byte[] ReadAllBytes(string resourceName, Assembly asm) {
			var strm = Open(resourceName, asm);
			int bufSize = Convert.ToInt32(strm.Length);
			byte[] buffer = new byte[bufSize];
			strm.Read(buffer, 0, bufSize);
			strm.Close();
			return buffer;
		}

		public static string[] ReadAllLines(string resourceName) {
			return ReadAllLines(resourceName, Assembly.GetCallingAssembly());
		}

		public static string[] ReadAllLines(string resourceName, Assembly asm) {
			List<string> result = new List<string>();
			using (Stream strm = Open(resourceName, asm)) {
				using (StreamReader rdr = new StreamReader(strm)) {
					result.Add(rdr.ReadLine());
				}
			}
			return result.ToArray();
		}

		public static void WriteToFile(string resourceName, string destinationFilePath) {
			WriteToFile(resourceName, destinationFilePath, Assembly.GetCallingAssembly());
		}

		public static void WriteToFile(string resourceName, string destinationFilePath, Assembly asm) {
			File.WriteAllBytes(destinationFilePath, ReadAllBytes(resourceName, asm));
		}

		public static string[] GetResourceNames() {
			return GetResourceNames(Assembly.GetCallingAssembly());
		}

		public static string[] GetResourceNames(Assembly asm) {
			return asm.GetManifestResourceNames();
		}

	}
}
