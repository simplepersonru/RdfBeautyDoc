using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
	internal class PlantUML(string remoteUrl)
	{

		private async Task RenderClass(Class cls)
		{
			var factory = new RendererFactory();
			var renderer = factory.CreateRenderer(new PlantUmlSettings { RemoteUrl = remoteUrl });
			var bytes = await renderer.RenderAsync("", OutputFormat.Svg);
			cls.SvgDiagram = Encoding.UTF8.GetString(bytes);
		}

		public async Task FillClassesAsync(Dictionary<string, Class> data)
		{

		}
	}
}
