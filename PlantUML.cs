using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
	class PlantUmlBuilder
	{
		StringBuilder _decl = new();
		StringBuilder _main = new();

		static string PlantUmlId(Class cls)
		{
			return $"{cls.Namespace}.{cls.Name}";
		}

		void ParentClass(Class cls)
		{
			Class(cls);

			if (cls.SubClass == null)
				return;

			_main.AppendLine($"{PlantUmlId(cls)} <|-down- {PlantUmlId(cls.SubClass)}");
			ParentClass(cls.SubClass);
		}
		void Enum(Class cls)
		{
			_decl.AppendLine($"""enum "{cls.Id}" as {PlantUmlId(cls)} """);

			_main.AppendLine($"enum {PlantUmlId(cls)} {{");
			foreach (var descr in cls.Descriptions)
			{
				_main.AppendLine($"#{descr.Value.Name}");
			}
			_main.AppendLine("}");

		}
		void ClassRelations(Class cls)
		{
			foreach (var propKeyValue in cls.Properties)
			{
				var prop = propKeyValue.Value;
				var stereotype = prop.Range.Stereotype;

				if (stereotype != Stereotype.Enum
					&& stereotype != Stereotype.Class)
					continue;

				if (stereotype == Stereotype.Enum)
					Enum(prop.Range);
				else if (stereotype == Stereotype.Class)
					Class(prop.Range);

				_main.AppendLine($"{PlantUmlId(cls)}::{prop.Name} -- {PlantUmlId(prop.Range)}");
			}
		}
		void Class(Class cls)
		{
			_decl.AppendLine($"""class "{cls.Id}" as {PlantUmlId(cls)} """);

			foreach (var prop in cls.Properties)
			{
				string rangeModifier = prop.Value.Range.Stereotype switch
				{
					Stereotype.Class => "~",
					Stereotype.Enum => "#",
					_ => "+",
				};

				// для Primitive Id не включать namespace всегда
				_main.AppendLine($"{PlantUmlId(cls)} : {rangeModifier}{prop.Value.PropertyId} : {prop.Value.Range.Id}");
			}
		}

		public string Build(Class cls)
		{
			_decl.AppendLine("left to right direction");
			_decl.AppendLine("skinparam groupInheritance 6");
			_decl.AppendLine("set separator none");
			_decl.Append(
						"""
						annotation "Легенда" {
						  #ссылка на enum
						  ~ссылка на класс
						  +простое свойство
						}
						""");

			Class(cls);
			ClassRelations(cls);
			if (cls.SubClass != null)
				ParentClass(cls.SubClass);

			var result = new StringBuilder();
			result.Append(_decl.ToString());
			result.Append(_main.ToString());

			return result.ToString();
		}
	}


	internal class PlantUML(string remoteUrl)
	{
		private async Task RenderClass(Class cls)
		{
			var factory = new RendererFactory();
			var renderer = factory.CreateRenderer(new PlantUmlSettings { RemoteUrl = remoteUrl });
			var bytes = await renderer.RenderAsync(new PlantUmlBuilder().Build(cls), OutputFormat.Svg);
			cls.SvgDiagram = Encoding.UTF8.GetString(bytes);
		}

		public async Task FillClassesAsync(Dictionary<string, Class> data)
		{
			var tasks = data.Values.Select(cls => RenderClass(cls));
			await Task.WhenAll(tasks);
		}
	}
}
