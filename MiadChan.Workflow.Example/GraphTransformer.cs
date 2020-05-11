
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;
using Maidchan.Workflow.Attributes;
using System.Reflection;

namespace MiadChan.Workflow.Example
{
    public struct Edge
    {
        public string From;
        public string To;
    }

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
    }
}