using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace COM3D2.LiveLink.Plugin
{
	internal sealed class LiveLinkPluginConfig
	{
		[ConfigEntry]
		public string ServerAddress = "com3d2.livelink";



		private List<ConfigField> configFields;
		public void BindToConfigFile(ConfigFile configFile)
		{
			if (configFile == null) throw new System.ArgumentNullException(nameof(configFile));

			configFields = new List<ConfigField>();

			MethodInfo[] configFileMethods = typeof(ConfigFile).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			MethodInfo genericBindMethod = null;
			foreach (var method in configFileMethods)
			{
				if (method.Name != nameof(ConfigFile.Bind)) continue;
				if (method.GetParameters().Length != 3) continue;
				genericBindMethod = method;
				break;
			}

			string section = "";
			foreach (var configField in GetAllConfigEntries())
			{
				section = configField.Attribute.Section ?? section;
				string key = configField.Attribute.Key ?? configField.FieldInfo.Name;

				var definition = new ConfigDefinition(section, key);
				object defaultValue = configField.FieldInfo.GetValue(this);


				// public ConfigEntry<T> Bind<T>(ConfigDefinition configDefinition, T defaultValue, ConfigDescription configDescription = null)
				MethodInfo bindMethod = genericBindMethod.MakeGenericMethod(configField.Type);
				configField.Entry = bindMethod.Invoke(
					configFile, 
					new object[] { definition, defaultValue, configField.Attribute.Description }
				) as ConfigEntryBase;
			}

			configFile.SettingChanged += OnSettingsChanged;
		}

		private void OnSettingsChanged(object source, SettingChangedEventArgs eventArgs)
		{
			ConfigField configField = configFields.Find( (x) => x.Entry == eventArgs.ChangedSetting );
			if (configField == null) return;

			configField.FieldInfo.SetValue(this, configField.Entry.BoxedValue);
		}






		private class ConfigEntryAttribute : Attribute
		{
			public string Section;
			public string Key = null;
			public ConfigDescription Description;
			public ConfigEntryAttribute() { }
			public ConfigEntryAttribute(string description)
			{
				Description = new ConfigDescription(description);
			}
		}
		private class ConfigField
		{
			public Type Type => this.FieldInfo.FieldType;
			public FieldInfo FieldInfo;
			public ConfigEntryAttribute Attribute;
			public ConfigEntryBase Entry;
			public ConfigField(FieldInfo fieldInfo, ConfigEntryAttribute attribute)
			{
				FieldInfo = fieldInfo;
				Attribute = attribute;
			}
		}
		private IEnumerable<ConfigField> GetAllConfigEntries()
		{
			var fields = typeof(LiveLinkPluginConfig).GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				var attributes = field.GetCustomAttributes(false);
				foreach (var attribute in attributes)
				{
					if (attribute is ConfigEntryAttribute configEntryAttribute)
					{
						yield return new ConfigField(field, configEntryAttribute);
						break;
					}
				}
			}
		}
	}
}
