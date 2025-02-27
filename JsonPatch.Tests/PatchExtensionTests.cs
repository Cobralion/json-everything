﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Json.Patch.Tests;

public class PatchExtensionTests
{
	private class TestModel
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public int[]? Numbers { get; set; }
		public string[]? Strings { get; set; }
		public List<TestModel>? InnerObjects { get; set; }
		public JsonElement? Attributes { get; set; }
	}

	[Test]
	public void CreatePatch_Test()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid(),
			Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]").RootElement
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("40664cc7-864f-4eed-939c-78076a252df0"),
			Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]").RootElement
		};
		var patchExpected =
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"40664cc7-864f-4eed-939c-78076a252df0\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/1/test\",\"value\":\"test32132\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/2/test\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/2/test1\",\"value\":\"test321\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/2\",\"value\":3}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/4\",\"value\":{\"test\":[1,2,3]}}]";

		var patch = initial.CreatePatch(expected);

		Assert.AreEqual(patchExpected, JsonSerializer.Serialize(patch));
	}

	[Test]
	public void CreatePatch_Test2()
	{
		var initial = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]");
		var expected = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/1/test\",\"value\":\"test32132\"},{\"op\":\"remove\",\"path\":\"/2/test\"},{\"op\":\"add\",\"path\":\"/2/test1\",\"value\":\"test321\"},{\"op\":\"replace\",\"path\":\"/3/test/2\",\"value\":3},{\"op\":\"add\",\"path\":\"/4\",\"value\":{\"test\":[1,2,3]}}]"
		);

		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void CreatePatch_ChangeTypeInArray()
	{
		var initial = JsonDocument.Parse("[{\"test\":true},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]},{\"test\":[1,2,3]}]");
		var expected = JsonDocument.Parse("[{\"test\":false},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":{\"test\":123}},{\"test\":[1,2,3]}]");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/0/test\",\"value\":false},{\"op\":\"replace\",\"path\":\"/1/test\",\"value\":\"test32132\"},{\"op\":\"remove\",\"path\":\"/2/test\"},{\"op\":\"add\",\"path\":\"/2/test1\",\"value\":\"test321\"},{\"op\":\"replace\",\"path\":\"/3/test/2\",\"value\":3},{\"op\":\"replace\",\"path\":\"/4/test\",\"value\":{\"test\":123}},{\"op\":\"add\",\"path\":\"/5\",\"value\":{\"test\":[1,2,3]}}]"
		);

		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void CreatePatch_ChangeTypeInObject()
	{
		var initial = JsonDocument.Parse("{\"test\":true, \"test2\":\"string\", \"test3\":{\"test123\":123}}");
		var expected = JsonDocument.Parse("{\"test\":false, \"test2\":123, \"test3\":[123]}");
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(
			"[{\"op\":\"replace\",\"path\":\"/test\",\"value\":false},{\"op\":\"replace\",\"path\":\"/test2\",\"value\":123},{\"op\":\"replace\",\"path\":\"/test3\",\"value\":[123]}]"
		);

		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void Add_Test()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid()
		};
		var target = new TestModel
		{
			Id = initial.Id,
			Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]").RootElement
		};
		var patchExpectedStr = "[{\"op\":\"add\",\"path\":\"/Attributes\",\"value\":[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]}]";
		var expected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr)!;
		var patch = initial.CreatePatch(target, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

		VerifyPatches(expected, patch);
	}

	[Test]
	public void Remove_Test()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid(),
			Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]").RootElement
		};
		var expected = new TestModel
		{
			Id = initial.Id
		};
		var patchExpectedStr = "[{\"op\":\"remove\",\"path\":\"/Attributes\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr)!;

		var patch = initial.CreatePatch(expected, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

		VerifyPatches(patchExpected, patch);
	}

	[Test]
	public void Replace_Test()
	{
		var initial = new TestModel
		{
			Id = Guid.NewGuid()
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("a299e216-dbbe-40e4-b4d4-556d7e7e9c35")
		};
		var patchExpectedStr = "[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"a299e216-dbbe-40e4-b4d4-556d7e7e9c35\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr);
		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void AddArray_Test()
	{
		var initial = JsonDocument.Parse("[1,2,3]");
		var expected = JsonDocument.Parse("[1,2,3,4]");
		var patchExpectedStr = "[{\"op\":\"add\",\"path\":\"/3\",\"value\":4}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr);
		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void RemoveArray_Test()
	{
		var initial = JsonDocument.Parse("[1,2,3]");
		var expected = JsonDocument.Parse("[1,2]");
		var patchExpectedStr = "[{\"op\":\"remove\",\"path\":\"/2\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr);
		var patch = initial.CreatePatch(expected);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void ReplaceArray_Test()
	{
		var initial = JsonDocument.Parse("[1,2,3]");
		var expected = JsonDocument.Parse("[1,2,1]");
		var patch = initial.CreatePatch(expected);
		var patchExpectedStr = "[{\"op\":\"replace\",\"path\":\"/2\",\"value\":1}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr);

		VerifyPatches(patchExpected!, patch);
	}

	[Test]
	public void ComplexObject_Test()
	{
		var initial = new TestModel
		{
			Id = Guid.Parse("aa7daced-c9fa-489b-9bc1-540b21d277a1"),
			Attributes = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]").RootElement,
			Name = "Test",
			Numbers = new[] { 1, 2, 3 },
			Strings = new[] { "test1", "test2" },
			InnerObjects = new List<TestModel>
			{
				new()
				{
					Id = Guid.Parse("b2cab2a0-ec23-405a-a5a8-975448a10334"),
					Name = "TestNameInner1",
					Numbers = new[] {3, 2, 1},
					Strings = new[] {"Test3", "test4"}
				}
			}
		};
		var expected = new TestModel
		{
			Id = Guid.Parse("4801bd62-a8ec-4ef2-ae3c-52b9f541625f"),
			Attributes = JsonDocument.Parse("[{\"test1\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,1,3]}]").RootElement,
			Name = "Test4",
			Numbers = new[] { 1, 2, 3, 4 },
			Strings = new[] { "test2", "test2" },
			InnerObjects = new List<TestModel>
			{
				new()
				{
					Id = Guid.Parse("bed584b0-7ccc-4336-adba-d0d7f7c3c3f2"),
					Name = "TestNameInner1",
					Numbers = new[] {1, 2, 1},
					Strings = new[] {"Test3", "test4", "test5"}
				}
			}
		};
		var patchExpectedStr =
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"4801bd62-a8ec-4ef2-ae3c-52b9f541625f\"}," +
			"{\"op\":\"replace\",\"path\":\"/Name\",\"value\":\"Test4\"}," +
			"{\"op\":\"add\",\"path\":\"/Numbers/3\",\"value\":4}," +
			"{\"op\":\"replace\",\"path\":\"/Strings/0\",\"value\":\"test2\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Id\",\"value\":\"bed584b0-7ccc-4336-adba-d0d7f7c3c3f2\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Numbers/0\",\"value\":1}," +
			"{\"op\":\"add\",\"path\":\"/InnerObjects/0/Strings/2\",\"value\":\"test5\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/4\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/1\",\"value\":1}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/0/test\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/0/test1\",\"value\":\"test123\"}]";
		var patchExpected = JsonSerializer.Deserialize<JsonPatch>(patchExpectedStr);

		var patchBackExpectedStr =
			"[{\"op\":\"replace\",\"path\":\"/Id\",\"value\":\"aa7daced-c9fa-489b-9bc1-540b21d277a1\"}," +
			"{\"op\":\"replace\",\"path\":\"/Name\",\"value\":\"Test\"}," +
			"{\"op\":\"remove\",\"path\":\"/Numbers/3\"}," +
			"{\"op\":\"replace\",\"path\":\"/Strings/0\",\"value\":\"test1\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Id\",\"value\":\"b2cab2a0-ec23-405a-a5a8-975448a10334\"}," +
			"{\"op\":\"replace\",\"path\":\"/InnerObjects/0/Numbers/0\",\"value\":3}," +
			"{\"op\":\"remove\",\"path\":\"/InnerObjects/0/Strings/2\"}," +
			"{\"op\":\"remove\",\"path\":\"/Attributes/0/test1\"}," +
			"{\"op\":\"add\",\"path\":\"/Attributes/0/test\",\"value\":\"test123\"}," +
			"{\"op\":\"replace\",\"path\":\"/Attributes/3/test/1\",\"value\":2},{\"op\":\"add\",\"path\":\"/Attributes/4\",\"value\":{\"test\":[1,2,3]}}]";
		var patchBackExpected = JsonSerializer.Deserialize<JsonPatch>(patchBackExpectedStr);

		var patch = initial.CreatePatch(expected, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
		var patchBack = expected.CreatePatch(initial, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

		VerifyPatches(patchExpected!, patch);
		VerifyPatches(patchBackExpected!, patchBack);
	}

	[Test]
	public void CreatePatch_ClearArray()
	{
		var model = new TestModel
		{
			Numbers = new[] { 1, 2, 1 },
			Strings = new[] { "asdf " },
			InnerObjects = new List<TestModel> { new TestModel { Id = Guid.NewGuid() }, new TestModel { Id = Guid.NewGuid() } }
		};
		var model2 = new TestModel
		{
			Numbers = Array.Empty<int>(),
			Strings = Array.Empty<string>(),
			InnerObjects = new List<TestModel>()
		};

		var patch = model.CreatePatch(model2);

		var final = patch.Apply(model);

		Assert.AreEqual(0, final!.Numbers!.Length);
		Assert.AreEqual(0, final.Strings!.Length);
		Assert.AreEqual(0, final.InnerObjects!.Count);
	}

	[Test]
	public void CreatePatch_RemoveArrayItem()
	{
		var model = new TestModel
		{
			Numbers = new[] { 1, 2, 3 },
			Strings = new[] { "123", "asdf" },
			InnerObjects = new List<TestModel> { new TestModel { Id = Guid.NewGuid() }, new TestModel { Id = Guid.NewGuid() } }
		};
		var model2 = new
		{
			Numbers = new[] { 1, 3 },
			Strings = new[] { "asdf" },
			InnerObjects = new List<TestModel> { model.InnerObjects[1] }
		};

		var patch = model.CreatePatch(model2);

		var final = patch.Apply(model);

		Assert.AreEqual(2, final!.Numbers!.Length);
		Assert.AreEqual(1, final.Numbers[0]);
		Assert.AreEqual(3, final.Numbers[1]);

		Assert.AreEqual(1, final.Strings!.Length);
		Assert.AreEqual("asdf", final.Strings[0]);

		Assert.AreEqual(1, final.InnerObjects!.Count);
		Assert.AreEqual(model.InnerObjects[1].Id, final.InnerObjects[0].Id);
	}

	[Test]
	public void ApplyPatch_Respect_SerializationOptions()
	{
		var model = new TestModel
		{
			Numbers = new int[0],
		};

		var patchStr = "[{\"op\":\"add\",\"path\":\"/numbers/-\",\"value\":5}]";
		var patch = JsonSerializer.Deserialize<JsonPatch>(patchStr)!;

		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
		var final = patch.Apply(model, options);

		Assert.AreEqual(5, final?.Numbers?[0]);
	}

	private static void OutputPatch(JsonPatch patch)
	{
		Console.WriteLine(JsonSerializer.Serialize(patch, new JsonSerializerOptions { WriteIndented = true }));
	}

	private static void VerifyPatches(JsonPatch expected, JsonPatch actual)
	{
		OutputPatch(expected);
		OutputPatch(actual);

		Assert.AreEqual(expected, actual);
	}
}