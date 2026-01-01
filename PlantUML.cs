using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
	internal class PlantUML
	{
		static async Task Work()
		{
			var factory = new RendererFactory();
			var renderer = factory.CreateRenderer(new PlantUmlSettings { RemoteUrl = "https://plantuml.simpleperson.ru" });
			var bytes = await renderer.RenderAsync("""
left to right direction
skinparam groupInheritance 6
set namespaceSeparator none
class "ts:GlobalSettings // Общие настройки" as ts.GlobalSettings [[https://google.com]]
ts.GlobalSettings : Integer threshold = 70 // Пороговое значение для успешного завершения тренировки, %
ts.GlobalSettings : Bool editableDescriptionOfOperation = false // Редактировать описание
ts.GlobalSettings : #ts::ResponceToBlocking responceToBlocking = 0 // Действия при срабатывании блокировки
ts.GlobalSettings::responceToBlocking -- ts.ResponceToBlocking
ts.GlobalSettings : Integer blockingActivationWithWarningResponce = -20 // Штрафной балл при срабатывании блокировки в режиме Предупреждение
ts.GlobalSettings : Integer switchDeviceWithBlocking = -20 // Штрафной балл при переключении КА с блокировкой привода
ts.GlobalSettings : Integer switchDeviceWithRemovedOperationalCurrent = -20 // Штрафной балл при переключении КА со снятым оперативным током
ts.GlobalSettings : Integer switchDeviceWithoutDU = -20 // Штрафной балл при переключении КА без захвата ДУ
ts.GlobalSettings : Integer analogValueDeviations = 0 // Отклонение аналоговых значений
ts.GlobalSettings : Integer notProvidedOperations = 0 // Не предусмотренные операции
ts.GlobalSettings : +ts::PenaltyPoint penaltyPoints  // Штрафные баллы
ts.GlobalSettings : +ts::Deviation deviations  // Отклонения аналоговых значений
enum ts.MalfunctionType {
   #General = 0 // Общая неисправность
   #InsulationDamage = 1 // Повреждение изоляции
   #RziaCircuits = 2 // Неисправность цепей РЗиА
}
""", OutputFormat.Svg);
			File.WriteAllBytes("out.svg", bytes);
		}
	}
}
