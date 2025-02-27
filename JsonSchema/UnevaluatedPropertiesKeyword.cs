﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;
// ReSharper disable AccessToModifiedClosure

namespace Json.Schema;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
[SchemaPriority(30)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(PropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(PatternPropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(AdditionalPropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(ContainsKeyword))]
[DependsOnAnnotationsFrom(typeof(UnevaluatedPropertiesKeyword))]
[JsonConverter(typeof(UnevaluatedPropertiesKeywordJsonConverter))]
public class UnevaluatedPropertiesKeyword : IJsonSchemaKeyword, ISchemaContainer, IEquatable<UnevaluatedPropertiesKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "unevaluatedProperties";

	/// <summary>
	/// The schema by which to evaluate additional properties.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="UnevaluatedPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value"></param>
	public UnevaluatedPropertiesKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Object)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		context.Options.LogIndentLevel++;
		var overallResult = true;
		var evaluatedProperties = new List<string>();
		var annotations = context.LocalResult.GetAllAnnotations(PropertiesKeyword.Name).ToList();
		if (!annotations.Any())
			context.Log(() => $"No annotation from {PropertiesKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {PropertiesKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}");
			evaluatedProperties.AddRange(annotations.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<string>())));
		}
		annotations = context.LocalResult.GetAllAnnotations(PatternPropertiesKeyword.Name).ToList();
		if (!annotations.Any())
			context.Log(() => $"No annotation from {PatternPropertiesKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {PatternPropertiesKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}");
			evaluatedProperties.AddRange(annotations.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<string>())));
		}
		annotations = context.LocalResult.GetAllAnnotations(AdditionalPropertiesKeyword.Name).ToList();
		if (!annotations.Any())
			context.Log(() => $"No annotation from {AdditionalPropertiesKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {AdditionalPropertiesKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}");
			evaluatedProperties.AddRange(annotations.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<string>())));
		}
		annotations = context.LocalResult.GetAllAnnotations(ContainsKeyword.Name).ToList();
		if (!annotations.Any())
			context.Log(() => $"No annotation from {ContainsKeyword.Name}.");
		else
		{
			context.Log(() => $"Annotation from {ContainsKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}");
			evaluatedProperties.AddRange(annotations.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<string>())));
		}
		annotations = context.LocalResult.GetAllAnnotations(Name).ToList();
		if (!annotations.Any())
			context.Log(() => $"No annotation from {Name}.");
		else
		{
			context.Log(() => $"Annotation from {Name}: {annotations.ToJsonArray().AsJsonString()}");
			evaluatedProperties.AddRange(annotations.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<string>())));
		}

		var obj = (JsonObject)context.LocalInstance!;
		if (!obj.VerifyJsonObject()) return;

		var unevaluatedProperties = obj.Where(p => !evaluatedProperties.Contains(p.Key)).ToArray();
		evaluatedProperties.Clear();
		foreach (var property in unevaluatedProperties)
		{
			if (!obj.TryGetPropertyValue(property.Key, out var item))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				continue;
			}

			context.Log(() => $"Evaluating property '{property.Key}'.");
			context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{property.Key}")), item ?? JsonNull.SignalNode,
				context.EvaluationPath.Combine(Name), Schema);
			context.Evaluate();
			var localResult = context.LocalResult.IsValid;
			overallResult &= localResult;
			context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
			if (localResult)
				evaluatedProperties.Add(property.Key);
		}
		context.Options.LogIndentLevel--;

		context.LocalResult.SetAnnotation(Name, JsonSerializer.SerializeToNode(evaluatedProperties));
		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(UnevaluatedPropertiesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Schema, other.Schema);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as UnevaluatedPropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
{
	public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new UnevaluatedPropertiesKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UnevaluatedPropertiesKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}