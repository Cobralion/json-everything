﻿using System.Text.RegularExpressions;
using JsonEverythingNet.Shared;
using Markdig;
using Markdig.SyntaxHighlighting;

namespace JsonEverythingNet.Services
{
	public static class AnchorRegistry
	{
		private static readonly Dictionary<string, string> _registry = new();
		private static readonly Dictionary<string, string> _firstLinks = new();

		public static async Task RegisterDocs(HttpClient client)
		{
			await Task.WhenAll(
				RegisterAnchors(client, "json-more"),
				RegisterAnchors(client, "examples/more/enums"),
				RegisterAnchors(client, "release-notes/json-more"),

				RegisterAnchors(client, "json-patch"),
				RegisterAnchors(client, "release-notes/json-patch"),

				RegisterAnchors(client, "json-path"),
				RegisterAnchors(client, "release-notes/json-path"),

				RegisterAnchors(client, "json-pointer"),
				RegisterAnchors(client, "release-notes/json-pointer"),

				RegisterAnchors(client, "json-logic"),
				RegisterAnchors(client, "release-notes/json-logic"),

				RegisterAnchors(client, "schema-basics"),
				RegisterAnchors(client, "schema-vocabs"),
				RegisterAnchors(client, "examples/schema/external-schemas"),
				RegisterAnchors(client, "examples/schema/managing-options"),
				RegisterAnchors(client, "examples/schema/version-selection"),
				RegisterAnchors(client, "release-notes/json-schema"),

				RegisterAnchors(client, "schema-datagen"),
				RegisterAnchors(client, "release-notes/json-schema-datageneration"),

				RegisterAnchors(client, "schema-generation"),
				RegisterAnchors(client, "examples/schemagen/attribute"),
				RegisterAnchors(client, "examples/schemagen/generator"),
				RegisterAnchors(client, "examples/schemagen/intent"),
				RegisterAnchors(client, "examples/schemagen/refiner"),
				RegisterAnchors(client, "release-notes/json-schema-generation"),

				RegisterAnchors(client, "vocabs-data-2022"),
				RegisterAnchors(client, "examples/schemadata/data-ref"),
				RegisterAnchors(client, "examples/schemadata/external-ref"),
				//RegisterAnchors(client, "examples/schemadata/schema-ref"),
				RegisterAnchors(client, "release-notes/json-schema-data"),

				RegisterAnchors(client, "vocabs-unique-keys"),
				RegisterAnchors(client, "release-notes/json-schema-unique-keys"),

				RegisterAnchors(client, "release-notes/json-schema-openapi")
			);
		}

		private static async Task RegisterAnchors(HttpClient client, string page)
		{
			try
			{
				var markdown = await client.GetStringAsync($"/md/{page}.md");
				var pipeline = new MarkdownPipelineBuilder()
					.UseAdvancedExtensions()
					.UseSyntaxHighlighting()
					.Build();
				var html = Markdown.ToHtml(markdown, pipeline);

				var matches = RegexPatterns.HeaderPattern.Matches(html);
				var first = true;

				foreach (Match match in matches)
				{
					var href = match.Groups[2].Value;

					_registry[href] = page;
					if (!first) continue;

					_firstLinks[page] = href;
					first = false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static string? GetPageForFragment(string fragment)
		{
			_registry.TryGetValue(fragment, out var ownerDoc);

			return ownerDoc;
		}

		public static string GetFirstFragment(string page)
		{
			_firstLinks.TryGetValue(page, out var link);

			return link!;
		}

		public static string GetHrefFromText(string text)
		{
			return text.Replace(" ", "-").ToLowerInvariant();
		}
	}
}