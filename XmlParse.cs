using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace RdfsBeautyDoc
{
	internal class XmlParse
	{
		Dictionary<string, Class> _classes = new();
		XmlNamespaceManager _xmlNs = new XmlNamespaceManager(new NameTable());
		private readonly Program.Options _options;
		XElement _root;

		public XmlParse(Program.Options options)
		{
			_options = options;

			var doc = XDocument.Load(options.RdfsPath);
			if (doc == null || doc.Root == null)
				throw new Exception($"Не удалось разобрать xml файла {options.RdfsPath}");
			_root = doc.Root;

			// Регистрируем неймспейсы (из твоего файла)
			_xmlNs.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			_xmlNs.AddNamespace("rdfs", "http://www.w3.org/TR/1999/PR-rdf-schema-19990303#");
			_xmlNs.AddNamespace("cims", "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#");
			_xmlNs.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
		}

		string getResource(XElement el, bool useFirstChar = false)
		{
			var rdfNs = _root.GetNamespaceOfPrefix("rdf");
			if ( rdfNs == null)
				throw new Exception($"Не зарегистрирован namespace rdf");

			var resourceAttr = el.Attribute(rdfNs.GetName("resource"));
			if (resourceAttr == null)
				throw new Exception($"Отсутствует rdf:resource у элемента {el.Name}");

			if (useFirstChar)
				return resourceAttr.Value;
			else 
				return resourceAttr.Value.Substring(1); // убираем # в начале

		}

		string getId(XElement el)
		{
			var rdfNs = _root.GetNamespaceOfPrefix("rdf");
			if (rdfNs == null)
				throw new Exception($"Не зарегистрирован namespace rdf");

			var idAttr = el.Attribute(rdfNs.GetName("ID"));
			var aboutAttr = el.Attribute(rdfNs.GetName("about"));

			if (idAttr == null && aboutAttr == null)
				throw new Exception($"Отсутствует rdf:ID или rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr != null)
				throw new Exception($"Неоднозначность между rdf:ID и rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr == null)
				return idAttr.Value;
			else if (idAttr == null && aboutAttr != null)
				return aboutAttr.Value;

			throw new UnreachableException();
		}

		Class GetOrCreateClass(string name)
		{
			Class? obj;

			if (!_classes.TryGetValue(name, out obj))
			{
				obj = new Class { Name = name };
				_classes[name] = obj;
			}
			return obj;
		}
		private void HandleClass(XElement el)
		{
			Class cls = GetOrCreateClass(getId(el));
			cls.Namespace = _options.CommonNamespace;
			foreach (var child in el.Elements())
			{
				if (child.Name.LocalName == "label")
					cls.Label = child.Value;
				else if (child.Name.LocalName == "comment")
					cls.Comment = child.Value;
				else if (child.Name.LocalName == "stereotype")
				{
					cls.Stereotype = getResource(child) switch
					{
						"Enumeration" => Stereotype.Enum,
						"Datatype" => Stereotype.DataType,
						"Primitive" => Stereotype.Primitive,
						_ => Stereotype.Class
					};
				}
				else if (child.Name.LocalName == "subClassOf")
					cls.SubClass = GetOrCreateClass(getResource(child)); // без # в начале
			}
		}

		private void HandleDescription(XElement el)
		{
			string id = getId(el);
			string[] splitName = id.Split('.');
			if (splitName.Length != 2)
				return;
			Class? enumClass = GetOrCreateClass(splitName.First());
			if (enumClass == null)
				return;
			enumClass.Stereotype = Stereotype.Enum;
			if (enumClass.Descriptions.ContainsKey(splitName.Last()))
				return;
			Description descr = new Description
			{
				Name = splitName.Last(),
				Domain = enumClass,
				Namespace = _options.CommonNamespace
			};
			enumClass.Descriptions.Add(descr.Id, descr);
			foreach (var child in el.Elements())
			{
				if (child.Name.LocalName == "label")
					descr.Label = child.Value;
			}
		}

		private void HandleProperty(XElement el)
		{
			Class? domainClass = null;
			foreach (var child in el.Elements())
			{
				if (child.Name.LocalName == "domain")
					domainClass = GetOrCreateClass(getResource(child));
			}
			if (domainClass == null)
				return;
			string[] splitName = getResource(el).Split('.');
			if (splitName.Length != 2)
				return;
			if (splitName.First() != domainClass.Id)
				return;
			if (domainClass.Properties.ContainsKey(splitName.Last()))
				return;
			Property prop = new Property
			{
				Domain = domainClass,
				Name = splitName.Last(),
				Namespace = _options.CommonNamespace,
			};
			domainClass.Properties.Add(prop.Id, prop);

			foreach (var child in el.Elements())
			{
				if (child.Name.LocalName == "label")
					prop.Label = child.Value;
				else if (child.Name.LocalName == "range")
				{
					string range = getResource(child);
					prop.Range = GetOrCreateClass(range);
				}
				else if (child.Name.LocalName == "multiplicity")
					prop.Multiplicity = getResource(child).Substring(2); // убираем лишнее "M:"
				else if (child.Name.LocalName == "inverseRoleName")
					prop.InverseRoleName = getResource(child, useFirstChar : true);
			}
		}

		public Dictionary<string, Class> Work()
		{
			foreach (var el in _root.Elements())
			{
				if (el == null)
					continue;
				if (el.Name.LocalName == "Property")
					HandleProperty(el);
				else if (el.Name.LocalName == "Class")
					HandleClass(el);
				else if (el.Name.LocalName == "Description")
					HandleDescription(el);
			}
			return _classes;
		}
	}
}
