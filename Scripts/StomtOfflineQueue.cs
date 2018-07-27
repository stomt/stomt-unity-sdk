using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Stomt
{
	public class StomtOfflineQueue<T>
	{
		private static bool Loaded = false;
		private static string FileName = "Queue.blob";
		private static List<T> Queue = new List<T>();

		public static void add(T item) {
			if (!StomtOfflineQueue<T>.Loaded) StomtOfflineQueue<T>.load();

			StomtOfflineQueue<T>.Queue.Add(item);
			StomtOfflineQueue<T>.save();
		}

		public static bool has() {
			if (!StomtOfflineQueue<T>.Loaded) StomtOfflineQueue<T>.load();

			return StomtOfflineQueue<T>.Queue.Count > 0;
		}

		public static T pop() {
			if (!StomtOfflineQueue<T>.Loaded) StomtOfflineQueue<T>.load();
			if (!StomtOfflineQueue<T>.has()) return default(T);

			T item = StomtOfflineQueue<T>.Queue[0];
			StomtOfflineQueue<T>.Queue.Remove(item);
			StomtOfflineQueue<T>.save();

			return item;
		}

		public static void clear() {
			StomtOfflineQueue<T>.Queue.Clear();
			StomtOfflineQueue<T>.save();
		}

		private static List<T> load() {
			if (File.Exists(Application.persistentDataPath + "/" + typeof(T).Name + FileName))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + "/" + typeof(T).Name + FileName, FileMode.Open);
				StomtOfflineQueue<T>.Queue = (List<T>)bf.Deserialize(file);
				file.Close();
				StomtOfflineQueue<T>.Loaded = true;
			}
			return StomtOfflineQueue<T>.Queue;
		}

		private static void save() {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.persistentDataPath + "/" + typeof(T).Name + FileName);
			bf.Serialize(file, StomtOfflineQueue<T>.Queue);
			file.Close();
		}
	}
}
