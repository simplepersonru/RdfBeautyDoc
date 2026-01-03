using PlantUml.Net;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RdfsBeautyDoc
{
	internal class Program
	{
		public class Options
		{
			public required string RdfsPath { get; set; }
			public required string PlantumlRemoteUrl { get; set; }
			public required string OutputPath { get; set; }
			public required string DocTitle { get; set; }
			public required string DocDescription { get; set; }
			public required string CommonNamespace { get; set; }
			public bool UseNamespaceForProperties { get; set; } = false;
		}
		static string GetEnv(string env)
		{
			string? val = Environment.GetEnvironmentVariable(env);
			if (val == null)
				throw new Exception($"Не определен ENV {env}");
			
			return val;
		}
		static async Task Main(string[] args)
		{
			var options = new Options
			{
				RdfsPath = GetEnv("RDFSDOC_PATH_TO_RDFS"),
				PlantumlRemoteUrl = GetEnv("RDFSDOC_PLANTUML_URL"),
				OutputPath = GetEnv("RDFSDOC_OUTPUT_PATH"),
				DocTitle = GetEnv("RDFSDOC_TITLE"),
				DocDescription = GetEnv("RDFSDOC_DESCRIPTION"),
				CommonNamespace = GetEnv("RDFSDOC_COMMON_NAMESPACE"),
				UseNamespaceForProperties = Convert.ToBoolean(Environment.GetEnvironmentVariable("RDFSDOC_USE_NAMESPACE_FOR_PROPERTIES"))
			};

			var classes = new XmlParse(options).Work();

			var umlrender = new PlantUML(options.PlantumlRemoteUrl);
			await umlrender.FillClassesAsync(classes);

			var generator = new SiteGenerator(
				data: classes,
				options: options);

			// 3. Генерируем сайт
			await generator.GenerateAsync();
		}
	}
}
