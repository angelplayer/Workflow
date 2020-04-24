
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Buffers;

namespace MiadChan.Workflow.Example
{
    public struct Edge {
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

            
            var output = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(output, options)) {
                
                JsonDocument document = JsonDocument.Parse(workflow);
                List<Edge> edges = new List<Edge>();

                string id = document.RootElement.GetProperty("Id").GetString();
                int version = document.RootElement.GetProperty("Version").GetInt16();

                writer.WriteStartObject();
                
                writer.WritePropertyName("Id");
                writer.WriteStringValue(id);

                writer.WritePropertyName("Id");
                writer.WriteNumberValue(version);

                writer.WritePropertyName("Nodes");
                writer.WriteStartArray();

                    var steps = document.RootElement.GetProperty("Steps").EnumerateArray();
                    foreach(var step in steps) {
                        writer.WriteStartObject();

                        writer.WritePropertyName("Id");
                        var stepId = step.GetProperty("Id").GetString();
                        writer.WriteStringValue(stepId);

                        writer.WritePropertyName("kind");
                        writer.WriteStringValue("task");

                        writer.WritePropertyName("Props");
                        writer.WriteStartObject();
                        writer.WriteEndObject();

                        if(step.TryGetProperty("NextStepId", out var to)) {
                            edges.Add(new Edge {From = stepId, To = to.GetString()});
                        }

                    writer.WriteEndObject();
                    }    

                writer.WriteEndArray();

                writer.WritePropertyName("edges");
                writer.WriteStartArray();
                    foreach(var edge in edges) {
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
            
            return Encoding.UTF8.GetString(output.GetSpan());
        }
    }
}