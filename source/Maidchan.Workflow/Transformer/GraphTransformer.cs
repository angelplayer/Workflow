
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System;


using Maidchan.Workflow.Attributes;
using MiadChan.Workflow.Models;

namespace MiadChan.Workflow.Transformer
{
    public class GraphTransformer
    {
        public static string ConvertToDegreD3Object(string workflow)
        {
            var options = new JsonWriterOptions()
            {
                Indented = true
            };


            var output = new MemoryStream();
            using (var writer = new Utf8JsonWriter(output, options))
            {

                JsonDocument document = JsonDocument.Parse(workflow);
                List<Edge> edges = new List<Edge>();

                string id = document.RootElement.GetProperty("Id").GetString();
                int version = document.RootElement.GetProperty("Version").GetInt16();
                

                writer.WriteStartObject();

                writer.WritePropertyName("id");
                writer.WriteStringValue(id);

                writer.WritePropertyName("version");
                writer.WriteNumberValue(version);

                writer.WritePropertyName("nodes");
                writer.WriteStartArray();

                var steps = document.RootElement.GetProperty("Steps").EnumerateArray();
                foreach (var step in steps)
                {
                    writer.WriteStartObject();

                    // Consider capitalize problem
                    writer.WritePropertyName("id");
                    var stepId = step.GetProperty("Id").GetString();
                    writer.WriteStringValue(stepId);

                    writer.WritePropertyName("kind");
                    writer.WriteStringValue("task");

                     writer.WritePropertyName("label");
                    writer.WriteStringValue("Task " + stepId);

                    var stepType = step.GetProperty("StepType").GetString();
                    // writer.WritePropertyName("stepType");
                    // writer.WriteStringValue(stepType);

                    WriteStepTypeParams(writer, stepType, step);

                    if (step.TryGetProperty("NextStepId", out var to))
                    {
                        edges.Add(new Edge { From = stepId, To = to.GetString() });
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();

                writer.WritePropertyName("edges");
                writer.WriteStartArray();
                foreach (var edge in edges)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("from");
                    writer.WriteStringValue(edge.From);
                    writer.WritePropertyName("to");
                    writer.WriteStringValue(edge.To);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
                writer.Flush();
            }

            return Encoding.UTF8.GetString(output.GetBuffer());
        }

        public static void WriteStepTypeParams(Utf8JsonWriter writer, string stepType) {
            WriteStepTypeParams(writer, stepType, null);
        }

        public static void WriteStepTypeParams(Utf8JsonWriter writer, string stepType, JsonElement? stepElement)
        {

            Type type = Type.GetType(stepType);
            if (type == null)
            {
                throw new Exception($"Unreconigzed task {stepType}");
            }

            writer.WritePropertyName("stepType");
            writer.WriteStringValue(type.Name);

            var stepAttr = type.GetCustomAttribute<StepTypeAttribute>();
            if (stepAttr != null)
            {
                if (!string.IsNullOrEmpty(stepAttr.Description))
                {
                    writer.WritePropertyName("description");
                    writer.WriteStringValue(stepAttr.Description);
                }
            }

            var propInfos = type.GetProperties();
            writer.WritePropertyName("Props");
            if(!stepElement.HasValue) {
                WritePropMetadata(writer, propInfos);
            } else {
                WritePropsValueOnly(writer, propInfos, stepElement.Value);
            }
           

        }

        private static void WritePropMetadata(Utf8JsonWriter writer, PropertyInfo[] propInfos) {
            writer.WriteStartArray();
            foreach (var item in propInfos)
            {
                var inputAttr = item.GetCustomAttribute<InputAttribute>();
                if (inputAttr != null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(item.Name);

                    writer.WritePropertyName("datatype");
                    writer.WriteStringValue(inputAttr.Kind.ToString());

                    if (!string.IsNullOrEmpty(inputAttr.HelpText))
                    {
                        writer.WritePropertyName("help");
                        writer.WriteStringValue(inputAttr.HelpText);
                    }
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
        }

        private static void WritePropsValueOnly(Utf8JsonWriter writer, PropertyInfo[] propInfos, JsonElement document) {
            var dict = new Dictionary<string, string>();
            var keyValue = document.GetProperty("Inputs").EnumerateObject();
            foreach(var kv in keyValue) {
                dict.Add(kv.Name, kv.Value.GetString());
            }

            writer.WriteStartObject();
            foreach (var item in propInfos)
            {
                var inputAttr = item.GetCustomAttribute<InputAttribute>();
                if (inputAttr != null)
                {
                    if (dict.ContainsKey(item.Name))
                    {
                        writer.WritePropertyName(item.Name);
                        writer.WriteStringValue(dict[item.Name]);
                    }
                }
            }
            writer.WriteEndObject();
        }

        public static string WorkflowFromGraph(WorkflowDataModel dto, IDictionary<string, System.Type> stepTypeDict) 
        {
          var options = new JsonWriterOptions()
          {
              Indented = true
          };

          var output = new MemoryStream();
          using (var writer = new Utf8JsonWriter(output, options))
          {
          writer.WriteStartObject();

          writer.WritePropertyName("Id");
          writer.WriteStringValue(dto.Id);

          writer.WritePropertyName(nameof(dto.Version));
          writer.WriteStringValue(dto.Version.ToString());
          writer.WritePropertyName("DataType");
          writer.WriteStringValue("");

          writer.WritePropertyName("Steps");
        
          writer.WriteStartArray();

            foreach (var node in dto.Nodes)
            {
              writer.WriteStartObject();

              writer.WritePropertyName("Id");
              writer.WriteStringValue(node.Id);

              writer.WritePropertyName("StepType");
              writer.WriteStringValue($"{stepTypeDict[node.StepType].FullName}, {stepTypeDict[node.StepType].Assembly.GetName().Name}");

              var enumerate = node.Props.EnumerateObject();
              if(enumerate.Count() > 0) {
                writer.WritePropertyName("Inputs");
                writer.WriteStartObject();
                foreach (var prop in enumerate){
                  writer.WritePropertyName(prop.Name);
                  writer.WriteStringValue(prop.Value.GetString());
                }
                writer.WriteEndObject();

              var hasNextStep = dto.Edges.Where(x => x.From == node.Id);
              foreach(var nextStep in hasNextStep) {
                writer.WritePropertyName("NextStepId");
                writer.WriteStringValue(nextStep.To);
            }
          }
              writer.WriteEndObject();
            }
          writer.WriteEndArray();

          writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(output.GetBuffer());
      }
    }
}