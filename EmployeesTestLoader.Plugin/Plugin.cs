using Newtonsoft.Json.Linq;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;





namespace EmployeesTestLoaderPlugin
{

    [Author(Name = "Vladimir Pykhtarev")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading employees from json");
            try
            {
                // Чтение JSON-файла
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("EmployeesTestLoader.Plugin.users.json");
                var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                // Парсинг в JObject
                var jObject = JObject.Parse(json);

                // Выбираем нужные поля и создаем DTO список загруженных рабочих(это плохой DTO в нем есть логика, лучше переписать его на обычный, а логику и валидацию вынестм отдельно)
                var employeesListFromJson = jObject["users"]
                     .Select(x =>
                     {
                         var dto = new EmployeesDTO
                         {
                             Name = $"{(string)x["firstName"]} {(string)x["lastName"]}"
                         };

                         string phone = (string)x["phone"];
                         if (!string.IsNullOrEmpty(phone))
                             dto.AddPhone(phone);

                         return dto;
                     }).ToList();


                logger.Info($"Loaded {employeesListFromJson.Count()} employees from json");
                var employeesList = args.Concat(employeesListFromJson);
                return employeesList.Cast<DataTransferObject>();
            }
            catch (Exception ex)
            {
                logger.Info("Fail loading employees from json");
                logger.Error(ex.Message);
                logger.Trace(ex.StackTrace);

            }
            return args;
        }
    }
}

