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
		void ParentClass(Class? cls)
		{
			if (cls == null)
				return;

			Class(cls);
			ParentClass(cls.SubClass);
		}
		void Enum(Class cls)
		{
			foreach (var prop in cls.Properties)
			{

			}
		}
		void Class(Class cls)
		{
			_decl.AppendLine($"class {cls.Id}");
			foreach (var prop in cls.Properties)
			{

			}
		}

		public string Build(Class cls)
		{
			_decl.AppendLine("left to right direction");
			_decl.AppendLine("skinparam groupInheritance 6");
			Class(cls);
			ParentClass(cls.SubClass);
			foreach (var propKeyValue in cls.Properties)
			{
				var prop = propKeyValue.Value;
				switch (prop.Range.Stereotype)
				{
					case Stereotype.Enum:
						Enum(prop.Range);
						break;
					case Stereotype.Class:
						Class(prop.Range);
						break;
					default:
						break;
				}
			}

			return $"{_decl.ToString()}{_main.ToString()}";
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
