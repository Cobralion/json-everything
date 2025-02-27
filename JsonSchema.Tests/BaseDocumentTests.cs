﻿using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class BaseDocumentTests
	{
		[Test]
		public void SchemasEmbeddedInJsonCanBeReferenced_Valid()
		{
			JsonSchema targetSchema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer);

			var json = new JsonObject
			{
				["prop1"] = "foo",
				["prop2"] = new JsonArray
				{
					"bar",
					JsonSerializer.SerializeToNode(targetSchema)
				}
			};

			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.List
			};

			var jsonBaseDoc = new JsonNodeBaseDocument(json, new Uri("http://localhost:1234/doc"));
			options.SchemaRegistry.Register(jsonBaseDoc);

			JsonSchema subjectSchema = new JsonSchemaBuilder()
				.Ref("http://localhost:1234/doc#/prop2/1");

			JsonNode instance = 42;

			var result = subjectSchema.Evaluate(instance, options);

			result.AssertValid();
		}

		[Test]
		public void SchemasEmbeddedInJsonCanBeReferenced_Invalid()
		{
			JsonSchema targetSchema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer);

			var json = new JsonObject
			{
				["prop1"] = "foo",
				["prop2"] = new JsonArray
				{
					"bar",
					JsonSerializer.SerializeToNode(targetSchema)
				}
			};

			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.List
			};

			var jsonBaseDoc = new JsonNodeBaseDocument(json, new Uri("http://localhost:1234/doc"));
			options.SchemaRegistry.Register(jsonBaseDoc);

			JsonSchema subjectSchema = new JsonSchemaBuilder()
				.Ref("http://localhost:1234/doc#/prop2/1");

			JsonNode instance = "baz"!;

			var result = subjectSchema.Evaluate(instance, options);

			result.AssertInvalid();
		}
	}
}
