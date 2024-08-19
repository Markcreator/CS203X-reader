using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

using CSLibrary.Exception;

namespace CSLibrary
{
	class MacError
	{
		private uint _errorNumber;
		private string _name;
		private string _description;

		public MacError(uint number, string name, string description)
		{
			_errorNumber = number;
			_name = name;
			_description = description;
		}

		public uint ErrorNumber
		{
			get { return _errorNumber; }
			set { _errorNumber = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
	}


	class MacErrorList : Dictionary<uint, MacError>
	{
		static private Dictionary<uint, MacError> Errors = null;


		public MacErrorList()
			: base(Errors)
		{

		}
		
		
		static MacErrorList()
		{

			string name = "macErrors.config";
            string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), name);

			if (!File.Exists(fileName))
			{
                throw new ReaderException(String.Format("A critial configuration file ({0}) is missing.", fileName));
			}
			
			Dictionary<uint, MacError> errorList = new Dictionary<uint, MacError>();

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CheckCharacters = true;
			settings.CloseInput = true;
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.IgnoreComments = true;
			settings.IgnoreProcessingInstructions = true;
			settings.IgnoreWhitespace = true;

			using (XmlReader xmlReader = XmlReader.Create(new FileStream(fileName, FileMode.Open), settings))
			{
				xmlReader.MoveToContent();

				while (!xmlReader.IsStartElement())
				{
					xmlReader.Read();
				}

				if (xmlReader.Name != "MacErrors")
				{
					throw new ReaderException("The \"MacErrors\" element is missing.");
				}

				xmlReader.ReadToFollowing("error");
				do
				{
					string id = xmlReader.GetAttribute("id");
					if (id.StartsWith("0x"))
						id = id.Substring(2);
					UInt16 errorCode = UInt16.Parse(id, System.Globalization.NumberStyles.HexNumber);
					string errorName = xmlReader.GetAttribute("name");
					string errorDesc = xmlReader.ReadElementContentAsString();

					errorList.Add(errorCode, new MacError(errorCode, errorName, errorDesc));
				} while (xmlReader.IsStartElement("error"));
	
			}

			Errors = errorList;
		}
	}




}
