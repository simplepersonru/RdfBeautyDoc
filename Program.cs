using PlantUml.Net;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RdfsBeautyDoc
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string path = "C:\\reposroot\\redkit-lab\\dmsutils\\cimparser\\scripts\\ck-rdf.xml";
			var classes = XmlParse.Work(path);

			// 2. Создаем генератор
			var generator = new SiteGenerator(
				data: classes,
				outputPath: "output");

			// 3. Генерируем сайт
			generator.GenerateAsync().GetAwaiter().GetResult();
		}
	}
}
