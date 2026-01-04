using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

        void ParentClass(Class cls, bool first = true)
        {
            if (!first)
                Class(cls);

            if (cls.SubClass == null)
                return;

            _main.AppendLine($"{PlantUmlId(cls.SubClass)} <|-down- {PlantUmlId(cls)}");
            ParentClass(cls.SubClass, first: false);
        }
        void Enum(Class cls)
        {
            _decl.AppendLine($"""enum "{cls.Id}" as {PlantUmlId(cls)} [[/{cls.Href}]] """);

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
                    Class(prop.Range, useProperties:false);

                _main.AppendLine($"{PlantUmlId(cls)}::{prop.Name} -- {PlantUmlId(prop.Range)}");
            }
        }
        void Class(Class cls, bool useProperties = true)
        {
            _decl.AppendLine($"""class "{cls.Id}" as {PlantUmlId(cls)} [[/{cls.Href}]] """);

            if (useProperties)
            {
                foreach (var prop in cls.Properties)
                {
                    string rangeModifier = prop.Value.Range.Stereotype switch
                    {
                        Stereotype.Class => "~",
                        Stereotype.Enum => "#",
                        _ => "+",
                    };

                    // для Primitive Id не включать namespace всегда
                    _main.AppendLine($"{PlantUmlId(cls)} : {rangeModifier}{prop.Value.NamespacedId} : {prop.Value.Range.Id}");
                }
            }
        }

        public string Build(Class cls)
        {
            _decl.AppendLine("skinparam groupInheritance 6");
            _decl.AppendLine("set separator none");
            _decl.AppendLine(
                        """
						annotation "Легенда" {
						  #ссылка на enum
						  ~ссылка на класс
						  +простое свойство
						}
						""");

            Class(cls);
            ClassRelations(cls);
            ParentClass(cls);

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
            if (cls.Stereotype != Stereotype.Class) 
                return;
            var factory = new RendererFactory();
            var renderer = factory.CreateRenderer(new PlantUmlSettings { RemoteUrl = remoteUrl });
            var bytes = await renderer.RenderAsync(new PlantUmlBuilder().Build(cls), OutputFormat.Svg);

            cls.SvgDiagram = Encoding.UTF8.GetString(bytes);
        }

        public async Task FillClassesAsync(Dictionary<string, Class> data)
        {
            var throttler = new SemaphoreSlim(50);

            var tasks = data.Values.Select(async v =>
            {
                await throttler.WaitAsync();
                try
                {
                    await RenderClass(v);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
