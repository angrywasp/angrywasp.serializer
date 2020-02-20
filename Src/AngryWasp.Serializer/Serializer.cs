using System;
using System.Collections.Generic;
using System.Reflection;
using AngryWasp.Logger;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer
{
	public static class Serializer
	{
		private static Dictionary<Type, object> serializers = new Dictionary<Type, object>();
		private static Dictionary<Type, object> binarySerializers = new Dictionary<Type, object>();
		private static List<string> loadedAssemblies = new List<string>();

		private static bool isInitialized = false;

		public static void Initialize()
		{
			if (isInitialized)
				return;

			AddSerializerAssembly(Assembly.GetExecutingAssembly());

			isInitialized = true;
		}

		public static void AddSerializerAssembly(string serializerAssembly)
		{
			try
			{
				if (loadedAssemblies.Contains(serializerAssembly))
					return;

				Assembly a = ReflectionHelper.Instance.LoadAssemblyFile(serializerAssembly);

				if (a == null)
				{
					Log.Instance.Write(Log_Severity.Error, $"Serializer.AddSerializerAssembly - Could not find assembly file '{serializerAssembly}'");
					return;
				}

				AddSerializerAssembly(a);

				loadedAssemblies.Add(serializerAssembly);
			}
			catch (Exception ex)
			{
				Log.Instance.WriteFatalException(ex, $"Serializer.AddSerializerAssembly - Loading Failed '{serializerAssembly}'");
			}
		}

		public static void AddSerializerAssembly(Assembly a)
		{
			List<Type> types = new List<Type>();
			types.AddRange(ReflectionHelper.Instance.GetTypesInheritingOrImplementing(a, typeof(ISerializer<>)));
			types.AddRange(ReflectionHelper.Instance.GetTypesInheritingOrImplementing(a, typeof(IBinarySerializer<>)));

			foreach (Type type in types)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type i in interfaces)
				{
					Type g = i.IsGenericType ? i.GetGenericTypeDefinition() : i;
					if (g == typeof(ISerializer<>))
					{
						serializers.Add(i.GetGenericArguments()[0], Activator.CreateInstance(type));
						break;
					}

					if (g == typeof(IBinarySerializer<>))
					{
						binarySerializers.Add(i.GetGenericArguments()[0], Activator.CreateInstance(type));
						break;
					}
				}
			}
		}

		public static bool HasSerializer(Type type, bool preferBinarySerialization) => preferBinarySerialization ? HasBinarySerializer(type) : HasXmlSerializer(type);

		public static bool HasXmlSerializer(Type type) => serializers.ContainsKey(type);

		public static bool HasBinarySerializer(Type type) => binarySerializers.ContainsKey(type);

		public static object GetSerializer(Type type, Dictionary<Type, object> serializerCollection)
		{
			if (!serializerCollection.ContainsKey(type))
				return null;

			return serializerCollection[type];
		}

		#region ISerializer implementation

		public static string Serialize(Type type, object value)
		{
			object serializer = GetSerializer(type, serializers);

			if (serializer == null)
				return string.Empty;

			MethodInfo method = serializer.GetType().GetMethod("Serialize");
			return (string)method.Invoke(serializer, new object[] { value });
		}

		public static object Deserialize(Type type, string value)
		{
			if (type == null)
				throw new NotImplementedException("Cannot deserialize Type. Value is null");

			object serializer = GetSerializer(type, serializers);

			if (serializer == null)
				throw new NotImplementedException("BinarySerializer for Type '" + type.Name + "' is not implemented");

			if (value == "null")
				return null;

			MethodInfo method = serializer.GetType().GetMethod("Deserialize");
			return method.Invoke(serializer, new object[] { value });
		}

		#endregion

		#region IBinarySerializer implementation

		public static BinarySerializerData BinarySerialize(Type type, object value)
		{
			object serializer = GetSerializer(type, binarySerializers);

			if (serializer == null)
				throw new NotImplementedException("BinarySerializer for Type '" + type.Name + "' is not implemented");

			MethodInfo method = serializer.GetType().GetMethod("Serialize");
			return (BinarySerializerData)method.Invoke(serializer, new object[] { value });
		}

		public static object BinaryDeserialize(Type type, BinarySerializerData data)
		{
			object serializer = GetSerializer(type, binarySerializers);

			if (serializer == null)
				throw new NotImplementedException("BinarySerializer for Type '" + type.Name + "' is not implemented");

			if (data.Length == 0)
				return null;

			MethodInfo method = serializer.GetType().GetMethod("Deserialize");
			return method.Invoke(serializer, new object[] { data });
		}

		#endregion
	}
}