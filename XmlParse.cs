using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RdfsBeautyDoc
{
	internal class XmlParse
	{
		static T? GetOrCreate<T>(Dictionary<string, T> objects, string? key) where T : Identified, new()
		{
			if (key == null)
				return default(T);

			T? obj;

			if (!objects.TryGetValue(key, out obj))
			{
				obj = new T { Id = key };
				objects[key] = obj;
			}
			return obj;
		}
		public static Dictionary<string, Class> Work(string filePath)
		{
			var doc = XDocument.Load(filePath);
			if (doc == null || doc.Root == null)
				return new();

			var classes = new Dictionary<string, Class>();

			foreach (var el in doc.Root.Elements())
			{
				if (el.Name.LocalName == "Property")
				{
					if (el.FirstAttribute == null)
						continue;
					Class? domainClass = null;
					foreach (var child in el.Elements())
					{
						if (child.Name.LocalName == "Domain")
							domainClass = GetOrCreate(classes, child.Value.Substring(1));
					}
					if (domainClass == null)
						continue;
					string[] splitName = el.FirstAttribute.Value.Split('.');
					if (splitName.Length != 2)
						continue;
					if (splitName.First() != domainClass.Id)
						continue;
					if (domainClass.Properties.ContainsKey(splitName.Last()))
						continue;
					Property prop = new Property
					{
						Domain = domainClass,
						Id = splitName.Last(),
					};
					domainClass.Properties.Add(prop.Id, prop);

					foreach (var child in el.Elements())
					{
						if (child.Name.LocalName == "Label")
							prop.Label = child.Value;
						else if (child.Name.LocalName == "Range")
						{
							prop.Range = child.Value.Substring(1);
							prop.RangeClass = GetOrCreate(classes, prop.Range);
						}
						else if (child.Name.LocalName == "Multiplicity")
							prop.Multiplicity = child.Value.Substring(1);
						else if (child.Name.LocalName == "InverseRoleName")
							prop.InverseRoleName = child.Value;
					}
				}
				else if (el.Name.LocalName == "Class")
				{
					Class? cls = GetOrCreate(classes, el.FirstAttribute?.Value);
					if (cls == null)
						continue;

					foreach (var child in el.Elements())
					{
						if (child.Name.LocalName == "Label")
							cls.Label = child.Value;
						else if (child.Name.LocalName == "Comment")
							cls.Comment = child.Value;
						else if (child.Name.LocalName == "Stereotype")
						{
							cls.Stereotype = child.Value switch
							{
								"Enumeration" => Stereotype.Enum,
								"Datatype" => Stereotype.DataType,
								"Primitive" => Stereotype.Primitive,
								_ => Stereotype.Class
							};
						}
						else if (child.Name.LocalName == "subClassOf")
							cls.SubClass = GetOrCreate(classes, child.FirstAttribute?.Value.Substring(1)); // без # в начале
					}
				}
				else if (el.Name.LocalName == "Description")
				{
					if (el.FirstAttribute == null)
						continue;
					string[] splitName = el.FirstAttribute.Value.Split('.');
					if (splitName.Length != 2)
						continue;
					Class? enumClass = GetOrCreate(classes, splitName.First());
					if (enumClass == null)
						continue;
					enumClass.Stereotype = Stereotype.Enum;
					if (enumClass.Descriptions.ContainsKey(splitName.Last()))
						continue;
					Description descr = new Description { Id = splitName.Last() };
					enumClass.Descriptions.Add(descr.Id, descr);
					foreach (var child in el.Elements())
					{
						if (child.Name.LocalName == "Label")
							descr.Label = child.Value;
					}
				}
			}
			return classes;
		}
	}
}
